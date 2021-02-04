using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace EntityManager.SGBD
{
    public abstract class Sgbd
    {
        public string connexionstring;

        public Sgbd(String connexionstring)
        {
            this.connexionstring = connexionstring;
        }

        public abstract DbConnection GetConnection();

        public abstract DbCommand GetCommand(string command, DbConnection connection);

        public enum Type { POSTGRES, MYSQL, SQLSERVER, ACCESS, ORACLE}

    }
}
