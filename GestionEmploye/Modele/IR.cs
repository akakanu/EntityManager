using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Modele
{
    [Table(Name = "ir", Schema = "public")]
    public class IR
    {
        [Id]
        [Colonne(Name = "id")]

        public int Id { get; set; }
        [Colonne(Name = "bornemax")]
        public double BorneMax { get; set; }

        [Colonne(Name = "bornemin")]
        public double BorneMin { get; set; }

        [Colonne(Name = "taux")]
        public double Taux { get; set; }



    }
}
