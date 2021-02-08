using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace EntityManager.SGBD
{
    public abstract class Sgbd
    {
        protected BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        public string connexionstring;

        public Sgbd(String connexionstring)
        {
            this.connexionstring = connexionstring;
        }

        public abstract DbConnection GetConnection();

        public abstract DbCommand GetCommand(string command, DbConnection connection);

        public abstract string CreateTable(PropertyInfo proterty);

        public abstract string TypeName(PropertyInfo field);
        

        protected string TableName(System.Type type)
        {
            Object obj = type.GetCustomAttribute(typeof(Table));
            if (obj != null ? obj is Table : false)
            {
                return (obj as Table).Name;
            }

            return null;
        }

        protected string ColonnName(PropertyInfo property)
        {
            Object obj = property.GetCustomAttribute(typeof(Colonne));
            if (obj != null ? obj is Colonne : false)
            {
                return (obj as Colonne).Name;
            }
            else
            {
                obj = property.GetCustomAttribute(typeof(JoinColumn));
                if (obj != null ? obj is JoinColumn : false)
                {
                    return (obj as JoinColumn).Name;
                }
            }
            return null;
        }

        protected string ReferenceName(PropertyInfo property)
        {

            Object obj = property.GetCustomAttribute(typeof(JoinColumn));
            if (obj != null ? obj is JoinColumn : false)
            {
                return (obj as JoinColumn).Reference;
            }

            return null;
        }

        protected PropertyInfo Key(System.Type type)
        {

            foreach (PropertyInfo info in type.GetProperties(flag))
            {
                Object obj = info.GetCustomAttribute(typeof(Id));
                if (obj != null)
                {
                    return info;
                }

            }
            return null;

        }

        public enum Type { POSTGRES, MYSQL, SQLSERVER, ACCESS, ORACLE }

    }
}
