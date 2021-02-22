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
            IR ir = new IR { Id = 3, BorneMax = 12, BorneMin = 9, Taux = 15 };
            db.Delete(ir);
            Console.WriteLine(ir.Id);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
