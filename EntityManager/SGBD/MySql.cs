using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
    }
}
