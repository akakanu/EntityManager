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
        public abstract DbParameter GetParameter(string champ, Object valeur);

        public abstract string CreateTable(System.Type table);

        public abstract string TypeName(PropertyInfo colonne);



        public string TableName(System.Type table)
        {
            Object obj = table.GetCustomAttribute(typeof(Table));
            if (obj != null ? obj is Table : false)
            {
                return (obj as Table).Name;
            }

            return null;
        }

        public string ColonnName(PropertyInfo colonne)
        {
            Object obj = colonne.GetCustomAttribute(typeof(Colonne));
            if (obj != null ? obj is Colonne : false)
            {
                return (obj as Colonne).Name;
            }
            else
            {
                obj = colonne.GetCustomAttribute(typeof(JoinColumn));
                if (obj != null ? obj is JoinColumn : false)
                {
                    return (obj as JoinColumn).Name;
                }
            }
            return null;
        }

        public string ReferenceName(PropertyInfo colonne)
        {

            Object obj = colonne.GetCustomAttribute(typeof(JoinColumn));
            if (obj != null ? obj is JoinColumn : false)
            {
                return (obj as JoinColumn).Reference;
            }

            return null;
        }

        public PropertyInfo Key(System.Type table)
        {

            foreach (PropertyInfo colonne in table.GetProperties(flag))
            {
                Object obj = colonne.GetCustomAttribute(typeof(Id));
                if (obj != null)
                {
                    return colonne;
                }

            }
            return null;

        }

        public PropertyInfo GetProperty(System.Type table, string colonname)
        {

            foreach (PropertyInfo colonne in table.GetProperties(flag))
            {
                Object obj = colonne.GetCustomAttribute(typeof(Colonne));
                if (obj != null ? (obj as Colonne).Name == colonname : false)
                {
                    return colonne;
                }
                obj = colonne.GetCustomAttribute(typeof(JoinColumn));
                if (obj != null ? (obj as JoinColumn).Name == colonname : false)
                {
                    return colonne;
                }

            }
            return null;

        }

        public enum Type { POSTGRES, MYSQL, SQLSERVER, ACCESS, ORACLE }

        public enum FetchType { LAZY, EAGER }

        public enum OrderBY
        {
            ASC, DESC
        }

        public enum Agregat
        {
           NOTHING, COUNT, MIN, MAX, AVG, SUM
        }

    }
}
