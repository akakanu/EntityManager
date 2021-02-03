using EntityManager;
using GestionEmploye.Modele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Context
{
    class DBContext:DataBase
    {
        public DBContext():base()
        {

        }
        public Employe Employe { get; set; }
    }
}
