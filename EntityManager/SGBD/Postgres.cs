using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
    }
}
