﻿using EntityManager.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmploye.Modele
{
    [Table(Name = "agent", Schema = "public")]
    public class Agent
    {
        [Id]
        [Colonne(Name = "id")]
        public int Id { get; set; }
        [Colonne(Name = "primeresponsabilite")]
        public double PrimeResponsabilite { get; set; }
        [JoinColumn(Name = "employe", Reference = "id")]
        public Employe Employe { get; set; }
    }
}
