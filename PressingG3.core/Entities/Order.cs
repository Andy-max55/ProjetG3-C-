using System;
using System.Collections.Generic;
using System.Linq; // Nécessaire pour le calcul de la somme
using PressingG3.core.Enums;

namespace PressingG3.core.Entities
{
    public class Order : BaseEntity
    {
        public string NumeroTicket { get; set; } // Ex: "T-1024"

        // Lien vers le Client (Clé étrangère)
        public Guid ClientId { get; set; }
        public virtual Client Client { get; set; }

        public OrderStatus Statut { get; set; } = OrderStatus.Depose;
        public DateTime? DatePrevu { get; set; }

        // Liste des vêtements dans ce ticket
        public virtual ICollection<OrderItem> Articles { get; set; } = new List<OrderItem>();

        // Logique métier : Calculer le total automatiquement
        // Ce n'est pas stocké en base, c'est calculé à la volée (Propriété intelligente)
        public decimal MontantTotal => Articles.Sum(x => x.Prix * x.Quantite);
    }
}