using System;
using System.ComponentModel.DataAnnotations;

// ATTENTION : Ici j'utilise le nom exact de votre projet
namespace PressingG3.core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}