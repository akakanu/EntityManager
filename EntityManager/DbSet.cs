using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static EntityManager.SGBD.Sgbd;

namespace EntityManager
{
    public class DbSet<T> : AbstractDbSet where T : Entity
    {
        string colonne = "";
        string condition = "";
        string orderBy = "";
        string groupBy = "";
        string having = "";
        string join = "";
        List<DbParameter> parametersWhere = new List<DbParameter>();
        List<DbParameter> parametersHaving = new List<DbParameter>();
        System.Type contrainte = null;

        private void Reset()
        {
            colonne = "";
            condition = "";
            orderBy = "";
            groupBy = "";
            having = "";
            join = "";
            parametersWhere = new List<DbParameter>();
            parametersHaving = new List<DbParameter>();
            contrainte = null;
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
                        parametersWhere.Clear();
                        parametersWhere.Add(DataBase.SGBD.GetParameter(":" + keyName, value));
                        List<Object> result = ToList().List();
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

        public ResultQuery ToList()
        {
            List<Object> result = new List<Object>();
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = contrainte == null ? arguments[0] : contrainte;
                    string query = GetQuery();
                    Console.WriteLine(query);
                    using (DbConnection dbcon = DataBase.SGBD.GetConnection())
                    {
                        if (dbcon != null)
                        {
                            DbCommand command = DataBase.SGBD.GetCommand(query, dbcon);
                            try
                            {
                                command.Parameters.AddRange(parametersWhere.ToArray());
                                command.Parameters.AddRange(parametersHaving.ToArray());
                                using (DbDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        if (colonne != "" && contrainte == null)
                                        {
                                            if (reader.FieldCount > 1)
                                            {
                                                object[] data = new object[reader.FieldCount];
                                                for (int i = 0; i < reader.FieldCount; i++)
                                                {
                                                    data[i] = reader[i];
                                                }
                                                result.Add(data);
                                            }
                                            else
                                            {
                                                result.Add(reader[0]);
                                            }
                                        }
                                        else
                                        {
                                            Entity entity = (Entity)Activator.CreateInstance(table);
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
            finally
            {
                Reset();
            }
            return new ResultQuery(result);
        }

        public DbSet<T> Where(Expression<Func<T, bool>> expression)
        {
            try
            {
                AddWhere(expression.Body);
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
            try
            {
                contrainte = null;
                if (expression.Body is UnaryExpression)
                {
                    UnaryExpression lbex = expression.Body as UnaryExpression;
                    MemberExpression operand = lbex.Operand as MemberExpression;
                    AddSelect(operand);
                }
                else if (expression.Body is MemberExpression)
                {
                    MemberExpression exp = expression.Body as MemberExpression;
                    if (colonne == "")
                    {
                        contrainte = exp.Type;
                    }
                    foreach (PropertyInfo colonne in exp.Type.GetProperties(DataBase.flag))
                    {
                        AddSelect(exp.Type, colonne, Agregat.NOTHING);
                    }
                }
                else if (expression.Body is NewExpression)
                {
                    NewExpression lbex = expression.Body as NewExpression;
                    foreach (Expression argument in lbex.Arguments)
                    {
                        AddSelect(argument);
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
                    AddGroup(operand);
                }
                else if (expression.Body is NewExpression)
                {
                    NewExpression lbex = expression.Body as NewExpression;
                    foreach (Expression argument in lbex.Arguments)
                    {
                        AddGroup(argument);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return this;
        }

        public DbSet<T> Having(Expression<Func<T, bool>> expression)
        {
            try
            {
                AddHaving(expression.Body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return this;
        }

        public DbSet<T> Join(Expression<Func<T, Object>> expression, SGBD.Sgbd.Join join)
        {
            try
            {
                if (expression.Body is MemberExpression)
                {
                    MemberExpression operand = expression.Body as MemberExpression;
                    AddJoin(operand, join);
                }
                else if (expression.Body is NewExpression)
                {
                    NewExpression lbex = expression.Body as NewExpression;
                    foreach (Expression argument in lbex.Arguments)
                    {
                        AddJoin(argument, join);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return this;
        }

        private void AddSelect(Expression expression)
        {
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    if (expression is MemberExpression)
                    {
                        MemberExpression operand = expression as MemberExpression;
                        string propertyName = operand.Member.Name;
                        var exp = operand.Expression;
                        if (exp is MemberExpression)
                        {
                            MemberExpression foreing = exp as MemberExpression;
                            table = foreing.Type;
                        }
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        AddSelect(table, colonne, Agregat.NOTHING);
                    }
                    else if (expression is MethodCallExpression)
                    {
                        MethodCallExpression operand = expression as MethodCallExpression;
                        string methodeName = operand.Method.Name;
                        Agregat agregat = GetAgregat(methodeName);
                        var exp = operand.Arguments[0];
                        string propertyName = exp.Type.Name;
                        if (exp is UnaryExpression)
                        {
                            UnaryExpression lbex = exp as UnaryExpression;
                            MemberExpression ope = lbex.Operand as MemberExpression;
                            exp = ope.Expression;
                            propertyName = ope.Member.Name;
                        }
                        if (exp is MemberExpression)
                        {
                            MemberExpression foreing = exp as MemberExpression;
                            table = foreing.Type;
                        }
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        AddSelect(table, colonne, agregat);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Agregat GetAgregat(string name)
        {
            if (name.Equals(Agregat.AVG.ToString()))
            {
                return Agregat.AVG;
            }
            else if (name.Equals(Agregat.COUNT.ToString()))
            {
                return Agregat.COUNT;
            }
            else if (name.Equals(Agregat.MAX.ToString()))
            {
                return Agregat.MAX;
            }
            else if (name.Equals(Agregat.MIN.ToString()))
            {
                return Agregat.MIN;
            }
            else if (name.Equals(Agregat.SUM.ToString()))
            {
                return Agregat.SUM;
            }
            return Agregat.NOTHING;
        }

        private void AddSelect(System.Type table, PropertyInfo colonne, Agregat agregat)
        {
            try
            {
                string tableName = DataBase.SGBD.TableName(table);
                string colonneName = tableName + "." + DataBase.SGBD.ColonnName(colonne);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddGroup(Expression expression)
        {
            try
            {
                if (expression is MemberExpression)
                {
                    MemberExpression operand = expression as MemberExpression;
                    string propertyName = operand.Member.Name;
                    System.Type[] arguments = this.GetType().GetGenericArguments();
                    if (arguments.Length > 0)
                    {
                        System.Type table = arguments[0];
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        string tableName = DataBase.SGBD.TableName(table);
                        string colonneName = tableName + "." + DataBase.SGBD.ColonnName(colonne);
                        this.groupBy += (this.groupBy == "" ? " group by " + colonneName : ", " + colonneName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddHaving(Expression expression)
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
                        AddHaving(left, right, expression.NodeType);
                    }
                    else
                    {
                        if (left is BinaryExpression)
                        {
                            if (having.Contains(" having ") && !having.EndsWith(" and ") && !having.EndsWith(" or "))
                            {
                                having += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddHaving(left);
                        }
                        if (right is BinaryExpression)
                        {
                            if (having.Contains(" where ") && !having.EndsWith(" and ") && !having.EndsWith(" or "))
                            {
                                having += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddHaving(right);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddWhere(Expression expression)
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
                        AddWhere(left, right, expression.NodeType);
                    }
                    else
                    {
                        if (left is BinaryExpression)
                        {
                            if (condition.Contains(" where ") && !condition.EndsWith(" and ") && !condition.EndsWith(" or "))
                            {
                                condition += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddWhere(left);
                        }
                        if (right is BinaryExpression)
                        {
                            if (condition.Contains(" where ") && !condition.EndsWith(" and ") && !condition.EndsWith(" or "))
                            {
                                condition += " " + GetSymboleOperant(expression.NodeType);
                            }
                            AddWhere(right);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddWhere(Expression left, Expression right, ExpressionType operant)
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
                    string tableName = DataBase.SGBD.TableName(table);
                    condition += (condition == "" ? " where " : " ") + tableName + "." + colonneName + " " + GetSymboleOperant(operant) + " :" + parameterName;
                    parametersWhere.Add(DataBase.SGBD.GetParameter(parameterName, value));
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        private void AddHaving(Expression left, Expression right, ExpressionType operant)
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
                    string tableName = DataBase.SGBD.TableName(table);
                    having += (having == "" ? " having " : " ") + tableName + "." + colonneName + " " + GetSymboleOperant(operant) + " :" + parameterName;
                    parametersHaving.Add(DataBase.SGBD.GetParameter(parameterName, value));
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        private void AddJoin(Expression expression, SGBD.Sgbd.Join join)
        {
            try
            {
                if (expression is MemberExpression)
                {
                    MemberExpression operand = expression as MemberExpression;
                    string propertyName = operand.Member.Name;
                    System.Type[] arguments = this.GetType().GetGenericArguments();
                    if (arguments.Length > 0)
                    {
                        System.Type table = arguments[0];
                        PropertyInfo colonne = table.GetProperty(propertyName);
                        if (colonne.GetCustomAttribute(typeof(JoinColumn)) != null)
                        {
                            string tableName = DataBase.SGBD.TableName(table);
                            string colonneName = tableName + "." + DataBase.SGBD.ColonnName(colonne);
                            string liaison = " inner join ";
                            switch (join)
                            {
                                case SGBD.Sgbd.Join.LEFT:
                                    liaison = " left join ";
                                    break;
                                case SGBD.Sgbd.Join.RIGHT:
                                    liaison = " right join ";
                                    break;
                            }
                            string referenceTable = DataBase.SGBD.TableName(colonne.PropertyType);
                            string referenceName = referenceTable + "." + DataBase.SGBD.ReferenceName(colonne);
                            liaison += referenceTable + " " + referenceTable + " on " + colonneName + " = " + referenceName;
                            this.join += liaison;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetQuery()
        {
            string query = "";
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    string tableName = DataBase.SGBD.TableName(table);
                    if (colonne == "")
                    {
                        foreach (PropertyInfo colonne in table.GetProperties(DataBase.flag))
                        {
                            this.colonne += tableName + "." + DataBase.SGBD.ColonnName(colonne) + ", ";
                        }
                        colonne = colonne.Trim().Substring(0, colonne.Trim().Length - 1);
                    }
                    query = "select " + colonne + " from " + tableName + " " + tableName;
                    if (join != "")
                    {
                        query += join;
                    }
                    if (condition != "")
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
                    if (having != "")
                    {
                        query += having;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return query;
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

    public class ResultQuery
    {
        List<Object> data = new List<object>();

        public ResultQuery(List<Object> data)
        {
            this.data = data;
        }

        public List<Object> List()
        {
            return data;
        }

        public T One<T>() where T : Entity
        {
            Object result = null;
            try
            {
                if (data != null ? data.Count > 0 : false)
                {
                    result = data[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (T)result;
        }

        public List<T> List<T>() where T : Entity
        {
            try
            {
                return data.Cast<T>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}
