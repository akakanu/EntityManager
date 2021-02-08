using EntityManager.Annotations;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace EntityManager.SGBD
{
    public class Postgres : Sgbd
    {
        public Postgres(string connexionstring) : base(connexionstring)
        {

        }

        public override DbCommand GetCommand(string command, DbConnection connection)
        {
            return new NpgsqlCommand(command, connection as NpgsqlConnection);
        }

        public override DbConnection GetConnection()
        {
            NpgsqlConnection npg = new NpgsqlConnection(connexionstring);
            try
            {
                npg.Open();
                return npg;
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
                result = "integer";
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
                result = "bigint";

            }
            return result;
        }

        public override string CreateTable(PropertyInfo proterty)
        {
            string query = null;
            try
            {
                List<PropertyInfo> contrainte = new List<PropertyInfo>();

                query = "create table if not exists " + TableName(proterty.PropertyType) + "(";
                foreach (PropertyInfo info in proterty.PropertyType.GetProperties(flag))
                {
                    string type = TypeName(info);
                    Object key = info.GetCustomAttribute(typeof(Id));
                    if (key != null)
                    {
                        if ((key as Id).AutoIncrement)
                        {
                            if (info.PropertyType == typeof(Int16) || info.PropertyType == typeof(Int32))
                            {
                                type = "serial";
                            }
                            else if (info.PropertyType == typeof(Int64))
                            {
                                type = "bigserial";
                            }
                        }
                        type += " primary key";
                    }
                    Object obj = info.GetCustomAttribute(typeof(JoinColumn));
                    if (obj != null)
                    {
                        contrainte.Add(info);
                        PropertyInfo id = Key(info.PropertyType);
                        if (id != null)
                        {
                            type = TypeName(id);
                        }
                    }
                    query += ColonnName(info) + " " + type + ",";
                }
                foreach (PropertyInfo item in contrainte)
                {
                    query += "constraint " + TableName(proterty.PropertyType) + "_" + ColonnName(item) + "_fkey Foreign key (" + ColonnName(item) + ") References " + TableName(item.PropertyType) + "(" + ReferenceName(item) + ") match simple on update cascade on delete no action,";
                }
                query = query.Substring(0, query.Length - 1) + ")";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return query;
        }


    }
}
