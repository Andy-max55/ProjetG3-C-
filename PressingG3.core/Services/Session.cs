using PressingG3.core.Entities;

namespace PressingG3.core.Services
{
    // "static" veut dire : accessible partout sans être recréé
    public static class Session
    {
        // Ici, on stocke l'utilisateur connecté actuellement
        public static User? CurrentUser { get; set; }

        // Petit utilitaire pour vérifier si on est connecté
        public static bool IsLoggedIn => CurrentUser != null;
    }
}