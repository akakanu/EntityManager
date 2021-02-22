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

                foreach (PropertyInfo table in GetType().GetProperties())
                {
                    string query = sgbd.CreateTable(table);
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

        public Object Insert(Object obj)
        {
            string query = null;
            string valeur = "";
            string champs = "";
            PropertyInfo key = null;
            try
            {
                List<DbParameter> parameters = new List<DbParameter>();

                query = "insert into " + sgbd.TableName(obj.GetType()) + " (";
                foreach (PropertyInfo colonne in obj.GetType().GetProperties(flag))
                {
                    Object annotation = colonne.GetCustomAttribute(typeof(Id));
                    if (annotation != null ? !(annotation as Id).AutoIncrement : true)
                    {
                        champs += sgbd.ColonnName(colonne) + ",";
                        valeur += ":" + sgbd.ColonnName(colonne) + ",";
                        parameters.Add(sgbd.GetParameter(sgbd.ColonnName(colonne), colonne.GetValue(obj)));
                    }
                    else
                    {
                        key = colonne;
                    }

                }
                query += champs.Substring(0, champs.Length - 1) + ") values (" + valeur.Substring(0, valeur.Length - 1) + ")";
                Console.WriteLine(query);

                using (DbConnection dbcon = sgbd.GetConnection())
                {
                    if (dbcon != null)
                    {
                        DbCommand command = sgbd.GetCommand(query, dbcon);
                        try
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                            int result = command.ExecuteNonQuery();
                            if (result == 1 && key != null)
                            {
                                query = "select " + sgbd.ColonnName(key) + " from " + sgbd.TableName(obj.GetType()) + " order by " + sgbd.ColonnName(key) + " desc limit 1";
                                command = sgbd.GetCommand(query, dbcon);
                                try
                                {
                                    using (DbDataReader reader = command.ExecuteReader())
                                    {
                                        if (reader.HasRows ? reader.Read() : false)
                                        {
                                            Object id = reader[0];
                                            key.SetValue(obj, id);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return obj;
        }

        public Object Update(Object objet)
        {

            string query = null;
            PropertyInfo key = null;
            try
            {
                List<DbParameter> parameters = new List<DbParameter>();
                query = "update " + sgbd.TableName(objet.GetType()) + " set ";
                foreach (PropertyInfo colonne in objet.GetType().GetProperties(flag))
                {
                    if (colonne.GetCustomAttribute(typeof(Id)) != null)
                    {
                        key = colonne;
                    }
                    query += sgbd.ColonnName(colonne) + " = :" + sgbd.ColonnName(colonne) + ", ";
                    parameters.Add(sgbd.GetParameter(sgbd.ColonnName(colonne), colonne.GetValue(objet)));
                }
                query = query.Trim().Substring(0, query.Trim().Length - 1);
                if (key != null)
                {
                    query += " where " + sgbd.ColonnName(key) + " = :" + sgbd.ColonnName(key);
                    parameters.Add(sgbd.GetParameter(sgbd.ColonnName(key), key.GetValue(objet)));
                }

                Console.WriteLine(query);

                using (DbConnection dbcon = sgbd.GetConnection())
                {
                    if (dbcon != null)
                    {
                        DbCommand command = sgbd.GetCommand(query, dbcon);
                        try
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                            int result = command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return objet;
        }

        public Boolean Delete(Object objet)
        {
            string query = null;
            try
            {
                PropertyInfo key = sgbd.Key(objet.GetType());
                if(key != null)
                {
                    query = "delete from " + sgbd.TableName(objet.GetType()) + " where " + sgbd.ColonnName(key) + " = :" + sgbd.ColonnName(key);

                    Console.WriteLine(query);

                    using (DbConnection dbcon = sgbd.GetConnection())
                    {
                        if (dbcon != null)
                        {
                            DbCommand command = sgbd.GetCommand(query, dbcon);
                            try
                            {
                                command.Parameters.Add(sgbd.GetParameter(sgbd.ColonnName(key), key.GetValue(objet)));
                                int result = command.ExecuteNonQuery();

                                return true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }
                }
                
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
