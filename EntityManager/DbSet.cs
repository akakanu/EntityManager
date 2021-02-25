using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using static EntityManager.SGBD.Sgbd;

namespace EntityManager
{
    public class DbSet<T>
    {
        string condition = "";
        List<DbParameter> parameters = new List<DbParameter>();

        public List<T> ToList()
        {
            List<T> result = new List<T>();
            string query = null;
            try
            {
                System.Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    System.Type table = arguments[0];
                    query = "select ";
                    foreach (PropertyInfo colonne in table.GetProperties(DataBase.flag))
                    {
                        query += DataBase.SGBD.ColonnName(colonne) + ", ";
                    }
                    query = query.Trim().Substring(0, query.Trim().Length - 1) + " from " + DataBase.SGBD.TableName(table);
                    if (parameters.Count > 0)
                    {
                        query += condition;
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
                                                        //foreign = new DbSet<ManyToOne>().Where(x=> x.;
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
            return result;
        }

        public DbSet<T> Where(Expression<Func<T, bool>> expression)
        {
            try
            {
                Expression left = null;
                Expression right = null;
                BinaryExpression be = expression.Body as BinaryExpression;
                if (be != null)
                {
                    left = be.Left;
                    right = be.Right;
                }
                else
                {
                    MethodCallExpression mc = expression.Body as MethodCallExpression;
                    if (mc != null && mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                    {
                        left = mc.Object; // "left"
                        right = mc.Arguments[0]; // "right"
                    }
                }
                if (left != null && right != null)
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
                        condition += (condition == "" ? " where " : " and ") + colonneName + " " + GetSymboleOperant(expression.Body.NodeType) + " :" + parameterName;
                        parameters.Add(DataBase.SGBD.GetParameter(parameterName, value));
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return this;
        }

        private string GetSymboleOperant(ExpressionType operant)
        {
            string symbole = "=";
            try
            {
                switch (operant)
                {
                    case ExpressionType.NotEqual:
                        return "!=";
                    case ExpressionType.Equal:
                        return "=";
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return symbole;
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
