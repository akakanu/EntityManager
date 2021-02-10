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
            try
            {
                query = "insert into " + sgbd.TableName(obj.GetType()) +" (";
                foreach (PropertyInfo colonne in obj.GetType().GetProperties(flag))
                {
                    Object annotation = colonne.GetCustomAttribute(typeof(Id));
                    if(annotation != null?!(annotation as Id).AutoIncrement:true)
                    {
                        query += sgbd.ColonnName(colonne) + ",";
                    }
                    

                }
                Console.WriteLine(query);
            }catch(Exception ex)
            {

            }
            return null;
        }
    }
}
