using System;
using System.ComponentModel.DataAnnotations.Schema;
// L'ENTITÉ BASE ENTITY EST NÉCESSAIRE. Si elle est dans le même namespace, 
// cette ligne est nécessaire pour référencer la classe User
using PressingG3.core.Entities;

namespace PressingG3.core.Entities
{
    // Attention : Assurez-vous que BaseEntity hérite correctement de ce namespace !
    public class AuditLog : BaseEntity
    {
        public Guid UserId { get; set; }

        // Ajout du ? pour satisfaire le compilateur et la base de données (CS8618)
        public virtual User? User { get; set; }

        public string ActionType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? ChangeDetails { get; set; }
    }
}