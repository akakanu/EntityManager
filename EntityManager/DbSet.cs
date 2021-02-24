using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace EntityManager
{
    public class DbSet<T>
    {

        public List<T> ToList()
        {
            List<T> result = new List<T>();
            string query = null;
            try
            {
                Type[] arguments = this.GetType().GetGenericArguments();
                if (arguments.Length > 0)
                {
                    Type table = arguments[0];
                    query = "select ";
                    foreach (PropertyInfo colonne in table.GetProperties(DataBase.flag))
                    {
                        query += DataBase.SGBD.ColonnName(colonne) + ", ";
                    }
                    query = query.Trim().Substring(0, query.Trim().Length - 1) + " from " + DataBase.SGBD.TableName(table);
                    Console.WriteLine(query);
                    using (DbConnection dbcon = DataBase.SGBD.GetConnection())
                    {
                        if (dbcon != null)
                        {
                            DbCommand command = DataBase.SGBD.GetCommand(query, dbcon);
                            try
                            {
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
                                                    PropertyInfo fkey = DataBase.SGBD.Key(colonne.PropertyType);
                                                    if (fkey != null)
                                                    {
                                                        fkey.SetValue(foreign, reader[i]);
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
    }
}
