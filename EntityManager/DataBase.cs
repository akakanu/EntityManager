using EntityManager.Annotations;
using EntityManager.SGBD;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace EntityManager
{
    public class DataBase
    {
        BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        Sgbd sgbd;
        Sgbd.Type type;
        public DataBase(Sgbd.Type type, string connexionstring)
        {
            this.type = type;
            try
            {
                switch (type)
                {

                    case Sgbd.Type.POSTGRES:
                        sgbd = new Postgres(connexionstring);
                        break;
                    case Sgbd.Type.MYSQL:
                        sgbd = new Mysql(connexionstring);
                        break;
                }

                foreach (PropertyInfo field in GetType().GetProperties())
                {
                    string query = "create table " + TableName(field.PropertyType) + "(";
                    string id = null;
                    foreach (PropertyInfo info in field.PropertyType.GetProperties(flag))
                    {
                        Object key = info.GetCustomAttribute(typeof(Id));
                        if (key != null)
                        {
                            id = ColonnName(info);
                        }
                        query += ColonnName(info) + " " + TypeName(info) + ",";

                    }
                    switch (this.type)
                    {
                        case Sgbd.Type.POSTGRES:
                            query = query.Substring(0, query.Length - 1) + ")";
                            break;
                        case Sgbd.Type.MYSQL:
                            query += " Primary key ("+ id + "))";
                            break;
                    }
                    Console.WriteLine(query);
                    using (DbConnection dbcon = sgbd.GetConnection())
                    {
                        if (dbcon != null)
                        {
                            DbCommand command = sgbd.GetCommand(query, dbcon);
                            try
                            {
                                command.ExecuteNonQuery();
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
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = (key != null ? (key as Id).AutoIncrement ? "serial" : "integer" : "integer");
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "int(8)" + (key != null ? (key as Id).AutoIncrement ? " NOT NULL AUTO_INCREMENT" : " NOT NULL" : "");
                        break;
                }
            }
            else if (type == typeof(String))
            {
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = "character varying";
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "varchar(255)";
                        break;
                }

            }
            else if (type == typeof(Boolean))
            {
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = "boolean";
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "boolean";
                        break;
                }

            }
            else if (type == typeof(Double))
            {
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = "double precision";
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "double";
                        break;
                }

            }
            else if (type == typeof(DateTime))
            {
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = "date";
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "date";
                        break;
                }

            }
            else if (type == typeof(Int64))
            {
                switch (this.type)
                {
                    case Sgbd.Type.POSTGRES:
                        result = (key != null ? (key as Id).AutoIncrement ? "bigserial" : "bigint" : "bigint");
                        break;
                    case Sgbd.Type.MYSQL:
                        result = "bigint(8)";
                        break;
                }

            }
            switch (this.type)
            {
                case Sgbd.Type.POSTGRES:
                    return result + (key != null ? " primary key" : "");
                default:
                    return result;
            }
        }
    }
}
