using EntityManager;
using EntityManager.SGBD;
using GestionEmploye.Modele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Context
{
    class DBContext : DataBase
    {
       // static string connexionstring = "PORT=5432;TIMEOUT=15;POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=20;COMMANDTIMEOUT=20;COMPATIBLE= 2.0.14.3;DATABASE=gestionemploye;HOST=localhost;PASSWORD=sudo;USER ID=postgres";
        static string connexionstring = "server=localhost;port=3306;database=gestionemploye;user=root;password=root";
        public DBContext() : base(Sgbd.Type.MYSQL, connexionstring)
        {

        }
        public Employe Employe { get; set; }
    }
}
