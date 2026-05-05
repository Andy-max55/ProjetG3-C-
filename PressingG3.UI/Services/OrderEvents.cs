using System;
using PressingG3.core.Entities;

namespace PressingG3.UI.Services
{
    // Cette classe sert de "Haut-parleur" entre tes fenêtres
    public static class OrderEvents
    {
        // L'événement auquel on peut s'abonner (le signal)
        public static event Action<Order>? OnOrderCompleted;

        // La méthode pour déclencher le signal (crier "C'est prêt !")
        public static void RaiseOrderCompleted(Order order)
        {
            OnOrderCompleted?.Invoke(order);
        }
    }
}