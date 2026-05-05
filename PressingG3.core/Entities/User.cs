using PressingG3.core.Enums;

namespace PressingG3.core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;     // Identifiant
        public string Password { get; set; } = string.Empty;    // Mot de passe
        public UserRole Role { get; set; }        // Son poste
    }
}