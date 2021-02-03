using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Modele
{
    [Table(Name = "employe", Schema = "public")]
    public class Employe
    {
        [Colonne(Name ="id")]
        public int Id { get; set; }
        [Colonne(Name = "nom")]
        public string Nom { get; set; }
        [Colonne(Name = "prenom")]
        public string Prenom { get; set; }
        [Colonne(Name = "matricule")]
        public string Matricule { get; set; }
        [Colonne(Name = "salaire")]
        public double salaire { get; set; }

    }
}
