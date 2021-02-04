using EntityManager.Annotations;
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
                foreach (PropertyInfo field in GetType().GetProperties())
                {
                    string query = "create table if not exist " + TableName(field.PropertyType) + "(";
                    foreach (PropertyInfo info in field.PropertyType.GetProperties(flag))
                    {
                        query += ColonnName(info) + " " + TypeName(info) + ",";

                    }
                    query = query.Substring(0, query.Length - 1);
                    query += ")";
                    Console.WriteLine(query);

                }
            }
            catch (Exception e)
            {

            }
        }

        private string TableName(Type type)
        {
            Object obj = type.GetCustomAttribute(typeof(Table));
            if (obj != null ? obj is Table : false)
            {
                return (obj as Table).Name;
            }

            return null;
        }

        private string ColonnName(PropertyInfo property)
        {
            Object obj = property.GetCustomAttribute(typeof(Colonne));
            if (obj != null ? obj is Colonne : false)
            {
                return (obj as Colonne).Name;
            }
            return null;
        }

        private string TypeName(PropertyInfo prop)
        {
            Object key = prop.GetCustomAttribute(typeof(Id));
            Type type = prop.PropertyType;
            string result = null;
            if (type == typeof(Int16) || type == typeof(Int32))
            {
                result = (key != null ? (key as Id).AutoIncrement ? "serial" : "integer" : "integer");
            }
            else if (type == typeof(String))
            {
                result = "character varying";
            }
            else if (type == typeof(Boolean))
            {
                result = "boolean";
            }
            else if (type == typeof(Double))
            {
                result = "double precision";
            }
            else if (type == typeof(DateTime))
            {
                result = "date";
            }
            else if (type == typeof(Int64))
            {
                result = (key != null ? (key as Id).AutoIncrement ? "bigserial" : "bigint" : "bigint");
            }
            return result + (key != null ? " primary key" : "");
        }
    }
}
