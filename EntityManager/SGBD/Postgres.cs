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

        public override string TypeName(PropertyInfo colonne)
        {
            System.Type type = colonne.PropertyType;
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

        public override string CreateTable(System.Type table)
        {
            string query = null;
            try
            {
                List<PropertyInfo> contraintes = new List<PropertyInfo>();
                query = "create table if not exists " + TableName(table) + "(";
                foreach (PropertyInfo colonne in table.GetProperties(flag))
                {
                    string type = TypeName(colonne);
                    Object key = colonne.GetCustomAttribute(typeof(Id));
                    if (key != null)
                    {
                        if ((key as Id).AutoIncrement)
                        {
                            if (colonne.PropertyType == typeof(Int16) || colonne.PropertyType == typeof(Int32))
                            {
                                type = "serial";
                            }
                            else if (colonne.PropertyType == typeof(Int64))
                            {
                                type = "bigserial";
                            }
                        }
                        type += " primary key";
                    }
                    Object obj = colonne.GetCustomAttribute(typeof(JoinColumn));
                    if (obj != null)
                    {
                        contraintes.Add(colonne);
                        PropertyInfo id = Key(colonne.PropertyType);
                        if (id != null)
                        {
                            type = TypeName(id);
                        }
                    }
                    query += ColonnName(colonne) + " " + type + ",";
                }
                foreach (PropertyInfo contraint in contraintes)
                {
                    query += "constraint " + TableName(table) + "_" + ColonnName(contraint) + "_fkey Foreign key (" + ColonnName(contraint) + ") References " + TableName(contraint.PropertyType) + "(" + ReferenceName(contraint) + ") match simple on update cascade on delete no action,";
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
            
            return new NpgsqlParameter(champ, valeur);
        }
    }
}
