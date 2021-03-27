using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using static EntityManager.SGBD.Sgbd;

namespace EntityManager
{
    public class DbSet<T> : AbstractDbSet
    {
        string colonne = "";
        string condition = "";
        string orderBy = "";
        string groupBy = "";
        List<DbParameter> parameters = new List<DbParameter>();

        private void Reset()
        {
            colonne = "";
            condition = "";
            orderBy = "";
            groupBy = "";
            parameters = new List<DbParameter>();
        }

        public override Object One(Object value)
        {
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    PropertyInfo key = DataBase.SGBD.Key(table);
                    if (key != null)
                    {
                        string keyName = DataBase.SGBD.ColonnName(key);
                        condition = " where " + keyName + " = :" + keyName;
                        parameters.Clear();
                        parameters.Add(DataBase.SGBD.GetParameter(":" + keyName, value));
                        List<T> result = ToList();
                        if (result != null ? result.Count > 0 : false)
                        {
                            return result[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public List<T> ToList()
        {
            List<T> result = new List<T>();
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    if (colonne == "")
                    {
                        foreach (PropertyInfo colonne in table.GetProperties(DataBase.flag))
                        {
                            this.colonne += DataBase.SGBD.ColonnName(colonne) + ", ";
                        }
                        colonne = colonne.Trim().Substring(0, colonne.Trim().Length - 1);
                    }
                    string query = "select " + colonne + " from " + DataBase.SGBD.TableName(table);
                    if (parameters.Count > 0)
                    {
                        query += condition;
                    }
                    if (orderBy != "")
                    {
                        query += orderBy;
                    }
                    if (groupBy != "")
                    {
                        query += groupBy;
                    }
                    Console.WriteLine(query);
                    using (DbConnection dbcon = DataBase.SGBD.GetConnection())
                    {
                        if (dbcon != null)
                        {
                            DbCommand command = DataBase.SGBD.GetCommand(query, dbcon);
                            try
                            {
                                command.Parameters.AddRange(parameters.ToArray());
                                using (DbDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        T entity = (T)Activator.CreateInstance(table);
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string colonname = reader.GetName(i);
                                            PropertyInfo colonne = DataBase.SGBD.GetProperty(table, colonname);
                                            if (colonne != null)
                                            {
                                                if (colonne.GetCustomAttribute(typeof(JoinColumn)) != null)
                                                {
                                                    Object foreign = Activator.CreateInstance(colonne.PropertyType);

                                                    Object manytoone = colonne.GetCustomAttribute(typeof(ManyToOne));
                                                    bool load = false;
                                                    if (manytoone != null ? (manytoone as ManyToOne).Fetch == FetchType.EAGER : false)
                                                    {
                                                        load = true;
                                                    }
                                                    if (load)
                                                    {
                                                        AbstractDbSet dbSet = DataBase.GetInstance.GetDbSet(colonne.PropertyType);
                                                        if (dbSet != null)
                                                        {
                                                            foreign = dbSet.One(reader[i]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        PropertyInfo fkey = DataBase.SGBD.Key(colonne.PropertyType);
                                                        if (fkey != null)
                                                        {
                                                            fkey.SetValue(foreign, reader[i]);
                                                        }
                                                    }

                                                    colonne.SetValue(entity, foreign);
                                                }
                                                else
                                                {
                                                    colonne.SetValue(entity, reader[i]);
                                                }

                                            }

                                        }
                                        result.Add(entity);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            Reset();
            return result;
        }

        public DbSet<T> Where(Expression<Func<T, bool>> expression)
        {
            try
            {
                AddExpression(expression.Body);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return this;
        }

        public DbSet<T> OrderBy(Expression<Func<T, Object>> expression, OrderBY orderBy)
        {
            try
            {
                if (expression.Body is UnaryExpression)
                {
                    UnaryExpression lbex = expression.Body as UnaryExpression;
                    MemberExpression operand = lbex.Operand as MemberExpression;
                    string propertyName = operand.Member.Name;
                    System.Type[] arguments = this.GetType().GetGenericArguments();
                    if (arguments.Length > 0)
                    {
                        System.Type table = arguments[0];
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        string colonneName = DataBase.SGBD.ColonnName(colonne);
                        this.orderBy += (this.orderBy == "" ? " order by " + colonneName : "," + colonneName) + "" + (orderBy == OrderBY.DESC ? " desc" : "");

                    }

                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return this;
        }

        public DbSet<T> Select(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.NOTHING);
        }
        public DbSet<T> AVG(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.AVG);
        }
        public DbSet<T> SUM(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.SUM);
        }
        public DbSet<T> MIN(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.MIN);
        }
        public DbSet<T> MAX(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.MAX);
        }
        public DbSet<T> COUNT(Expression<Func<T, Object>> expression)
        {
            return Select(expression, Agregat.COUNT);
        }

        private DbSet<T> Select(Expression<Func<T, Object>> expression, Agregat agregat)
        {
            try
            {
                if (expression.Body is UnaryExpression)
                {
                    UnaryExpression lbex = expression.Body as UnaryExpression;
                    MemberExpression operand = lbex.Operand as MemberExpression;
                    string propertyName = operand.Member.Name;
                    System.Type[] arguments = this.GetType().GetGenericArguments();
                    if (arguments.Length > 0)
                    {
                        System.Type table = arguments[0];
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        string colonneName = DataBase.SGBD.ColonnName(colonne);
                        switch (agregat)
                        {
                            case Agregat.AVG:
                                colonneName = "AVG(" + colonneName + ")";
                                break;
                            case Agregat.SUM:
                                colonneName = "SUM(" + colonneName + ")";
                                break;
                            case Agregat.MIN:
                                colonneName = "MIN(" + colonneName + ")";
                                break;
                            case Agregat.MAX:
                                colonneName = "MAX(" + colonneName + ")";
                                break;
                            case Agregat.COUNT:
                                colonneName = "COUNT(" + colonneName + ")";
                                break;
                        }
                        this.colonne += (this.colonne == "" ? "" + colonneName : ", " + colonneName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return this;
        }
        public DbSet<T> GroupBy(Expression<Func<T, Object>> expression)
        {
            try
            {
                if (expression.Body is UnaryExpression)
                {
                    UnaryExpression lbex = expression.Body as UnaryExpression;
                    MemberExpression operand = lbex.Operand as MemberExpression;
                    string propertyName = operand.Member.Name;
                    System.Type[] arguments = this.GetType().GetGenericArguments();
                    if (arguments.Length > 0)
                    {
                        System.Type table = arguments[0];
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        string colonneName = DataBase.SGBD.ColonnName(colonne);
                        this.groupBy += (this.groupBy == "" ? " group by " + colonneName : ", " + colonneName);

                    }

                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return this;
        }

        public void AddExpression(Expression expression)
        {
            try
            {
                Expression[] result = GetExpressions(expression);
                Expression left = result[0];
                Expression right = result[1];
                if (left != null && right != null)
                {

                    if (left is MemberExpression)
                    {
                        AddCondition(left, right, expression.NodeType);
                    }
                    else
                    {
                        if (left is BinaryExpression)
                        {
                            if (condition.Contains(" where ") && !condition.EndsWith(" and ") && !condition.EndsWith(" or "))
                            {
                                condition += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddExpression(left);
                        }
                        if (right is BinaryExpression)
                        {
                            if (condition.Contains(" where ") && !condition.EndsWith(" and ") && !condition.EndsWith(" or "))
                            {
                                condition += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddExpression(right);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        private Expression[] GetExpressions(Expression expression)
        {
            Expression[] result = new Expression[2];
            try
            {
                Expression left = null;
                Expression right = null;
                BinaryExpression be = expression as BinaryExpression;
                if (be != null)
                {
                    left = be.Left;
                    right = be.Right;
                }
                else
                {
                    MethodCallExpression mc = expression as MethodCallExpression;
                    if (mc != null && mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                    {
                        left = mc.Object; // "left"
                        right = mc.Arguments[0]; // "right"
                    }
                }
                result[0] = left;
                result[1] = right;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return result;
        }

        private void AddCondition(Expression left, Expression right, ExpressionType operant)
        {
            try
            {
                string propertyName = (left as MemberExpression).Member.Name;
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    PropertyInfo colonne = table.GetProperty(propertyName);
                    string colonneName = DataBase.SGBD.ColonnName(colonne);
                    Object value = (right as ConstantExpression).Value;
                    string parameterName = GetParameterName(colonneName, colonneName);
                    condition += (condition == "" ? " where " : " ") + colonneName + " " + GetSymboleOperant(operant) + " :" + parameterName;
                    parameters.Add(DataBase.SGBD.GetParameter(parameterName, value));
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        private string GetSymboleOperant(ExpressionType operant)
        {
            try
            {
                switch (operant)
                {
                    case ExpressionType.NotEqual:
                        return "!=";
                    case ExpressionType.Equal:
                        return "=";
                    case ExpressionType.AndAlso:
                        return "and";
                    case ExpressionType.OrElse:
                        return "or";
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return "";
        }

        private string GetParameterName(string colonneName, string parametre)
        {
            try
            {
                if (condition.Contains(":" + parametre))
                {
                    string end = parametre.Replace(colonneName, "").Trim();
                    if (end != "")
                    {
                        end = Convert.ToInt32(end) + 1 + "";
                    }
                    else
                    {
                        end = "1";
                    }
                    return GetParameterName(colonneName, colonneName + end);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return parametre;
        }

    }
}
