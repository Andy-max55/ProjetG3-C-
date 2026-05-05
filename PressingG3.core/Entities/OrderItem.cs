using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressingG3.core.Entities
{
    public class OrderItem : BaseEntity
    {
        public string Description { get; set; } // Ex: "Costume 3 pièces"
        public string Categorie { get; set; }   // Ex: "Délicat"
        public decimal Prix { get; set; }
        public int Quantite { get; set; } = 1;
        public string? Remarque { get; set; }   // Ex: "Tache de gras manche droite"

        // Lien vers la Commande parente
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }

        [NotMapped]
        public decimal TotalLigne => Prix * Quantite;

    }
}