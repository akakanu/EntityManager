using EntityManager;
using GestionEmploye.Context;
using GestionEmploye.Modele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEmploye
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DBContext db = new DBContext();
            //var list = db.Agent.Select(x => new { x.Employe, x.Id }).Join(x => x.Employe, EntityManager.SGBD.Sgbd.Join.INNER).ToList().List<Employe>();
            var list = db.Agent.Select(x => new { x.PrimeResponsabilite, Y = AbstractDbSet.AVG(x.Employe.Id) }).Join(x=> x.Employe, EntityManager.SGBD.Sgbd.Join.INNER).GroupBy(x => x.PrimeResponsabilite).ToList().List();

            Console.WriteLine(list);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
