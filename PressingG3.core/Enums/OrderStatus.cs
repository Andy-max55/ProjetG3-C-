namespace PressingG3.core.Enums
{
    public enum OrderStatus
    {
        Depose,         // 0 - Le client vient de déposer
        EnLavage,       // 1 - En machine
        Repassage,      // 2 - En finition
        Pret,           // 3 - Sur le cintre, attend le client
        Livre,          // 4 - Rendu au client
        Paye            // 5 - Transaction clôturée
    }
}