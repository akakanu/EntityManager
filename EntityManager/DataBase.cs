using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EntityManager
{
    public class DataBase
    {
        BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        public DataBase()
        {
            try
            {
                foreach(PropertyInfo field in GetType().GetProperties())
                {
                    Console.WriteLine(field);
                    foreach (PropertyInfo info in field.PropertyType.GetProperties(flag))
                    {
                        Console.WriteLine("colonne :" + info);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
