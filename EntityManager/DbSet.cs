using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityManager
{
    public class DbSet<T>
    {
        
        public List<T> ToList()
        {
            string query = null;
            try
            {
                Type[] arguments = this.GetType().GetGenericArguments();
                if(arguments.Length > 0)
                {
                    Type table = arguments[0];
                    query = "select ";
                    foreach (PropertyInfo colonne in table.GetProperties(DataBase.flag))
                    {
                        query += DataBase.sgbd.ColonnName(colonne) + ", "; 
                    }
                    query = query.Trim().Substring(0, query.Trim().Length - 1) + " from " + DataBase.sgbd.TableName(table);
                    Console.WriteLine(query);
                }
               
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return new List<T>();
        }
    }
}
