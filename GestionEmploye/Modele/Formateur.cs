using EntityManager;
using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Modele
{
    [Table(Name = "formateur", Schema = "public")]
    public class Formateur : Entity
    {
        [Id]
        [Colonne(Name ="id")]
        public int Id { get; set; }

        [Colonne(Name ="heuresupp")]
        public double  HeureSupp { get; set; }

        [Colonne(Name ="remunerationhsupp")]
        public double RemunerationHSupp { get; set; }

        [JoinColumn(Name ="employe", Reference ="id")]
        public Employe Employe { get; set; }

    }
}
