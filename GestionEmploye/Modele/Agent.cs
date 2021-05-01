using EntityManager;
using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EntityManager.SGBD.Sgbd;

namespace GestionEmploye.Modele
{
    [Table(Name = "agent", Schema = "public")]
    public class Agent : Entity
    {
        [Id]
        [Colonne(Name = "id")]
        public int Id { get; set; }
        [Colonne(Name = "primeresponsabilite")]
        public double PrimeResponsabilite { get; set; }
        [JoinColumn(Name = "employe", Reference = "id")]
        [ManyToOne(Fetch = FetchType.LAZY)]
        public Employe Employe { get; set; }
    }
}
