using System.Collections.Generic;

namespace PressingG3.core.Entities
{
    public class Client : BaseEntity
    {
        public string NomComplet { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Adresse { get; set; }

        // Relation : Un client a un historique de commandes
        public virtual ICollection<Order> Commandes { get; set; } = new List<Order>();
    }
}