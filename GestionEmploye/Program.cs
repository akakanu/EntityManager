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
            var list = db.IR.Select(x => x.BorneMin).COUNT(x => x.Id).GroupBy(x => x.BorneMin).ToList();

            Console.WriteLine(list);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
