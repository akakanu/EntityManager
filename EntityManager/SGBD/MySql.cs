using EntityManager.Annotations;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace EntityManager.SGBD
{
    public class Mysql : Sgbd
    {

        public Mysql(string connexionstring) : base(connexionstring)
        {

        }

        public override DbCommand GetCommand(string command, DbConnection connection)
        {
            return new MySqlCommand(command, connection as MySqlConnection);
        }

        public override DbConnection GetConnection()
        {
            MySqlConnection msq = new MySqlConnection(connexionstring);
            try
            {
                msq.Open();
                return msq;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public override string TypeName(PropertyInfo prop)
        {
            
            System.Type type = prop.PropertyType;
            string result = null;
            if (type == typeof(Int16) || type == typeof(Int32))
            {
                result = "int(8)";
            }
            else if (type == typeof(String))
            {
                result = "varchar(255)";
            }
            else if (type == typeof(Boolean))
            {
                result = "boolean";
            }
            else if (type == typeof(Double))
            {
                result = "double";
            }
            else if (type == typeof(DateTime))
            {
                result = "date";
            }
            else if (type == typeof(Int64))
            {
                result = "bigint(8)";
            }
            return result;
        }
        public override string CreateTable(System.Type table)
        {
            string query = null;
            try
            {
                List<PropertyInfo> contrainte = new List<PropertyInfo>();

                query = "create table if not exists " + TableName(table) + "(";
                string id = null;
                foreach (PropertyInfo info in table.GetProperties(flag))
                {
                    string type = TypeName(info);
                    Object key = info.GetCustomAttribute(typeof(Id));
                    if (key != null)
                    {
                        id = ColonnName(info);
                        type += (key != null ? (key as Id).AutoIncrement ? " NOT NULL AUTO_INCREMENT" : " NOT NULL" : "");
                    }
                    Object obj = info.GetCustomAttribute(typeof(JoinColumn));
                    if (obj != null)
                    {
                        contrainte.Add(info);
                        PropertyInfo id1 = Key(info.PropertyType);
                        if (id1 != null)
                        {
                            type = TypeName(id1);
                        }
                    }
                    query += ColonnName(info) + " " + type + ",";
                }
                query += " Primary key (" + id + "), ";
                foreach (PropertyInfo item in contrainte)
                {
                    query += "FOREIGN KEY (" + ColonnName(item) + ") REFERENCES " + TableName(item.PropertyType) + "(" + ReferenceName(item) + ") ,";
                }
                query = query.Substring(0, query.Length - 1) + ")";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return query;
        }
        public override DbParameter GetParameter(string champ, object valeur)
        {
            
            return new MySqlParameter(champ, valeur);
        }
    }
}
