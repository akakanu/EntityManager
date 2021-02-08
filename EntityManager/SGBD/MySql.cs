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
            Object key = prop.GetCustomAttribute(typeof(Id));
            System.Type type = prop.PropertyType;
            string result = null;
            if (type == typeof(Int16) || type == typeof(Int32))
            {
                result = "int(8)" + (key != null ? (key as Id).AutoIncrement ? " NOT NULL AUTO_INCREMENT" : " NOT NULL" : "");
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
        public override string CreateTable(PropertyInfo proterty)
        {
            string query = null;
            try
            {
                query = "create table if not exists " + TableName(proterty.PropertyType) + "(";
                string id = null;
                foreach (PropertyInfo info in proterty.PropertyType.GetProperties(flag))
                {
                    Object key = info.GetCustomAttribute(typeof(Id));
                    if (key != null)
                    {
                        id = ColonnName(info);
                    }
                    query += ColonnName(info) + " " + TypeName(info) + ",";
                }
                query += " Primary key (" + id + "))";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return query;
        }
    }
}
