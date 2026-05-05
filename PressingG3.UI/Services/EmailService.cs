using System;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Windows;
using PressingG3.core.Entities;

namespace PressingG3.UI.Services
{
    public static class EmailService
    {
        // ⚠️ PARAMÈTRES SMTP - À REMPLACER ABSOLUMENT PAR LES VÔTRES !
        private const string SMTP_HOST = "smtp.gmail.com"; // Hôte (Ex: smtp.office365.com pour Outlook)
        private const int SMTP_PORT = 587; // Port standard pour TLS/STARTTLS
        private const string SENDER_EMAIL = "tallaisidor10@gmail.com";
        private const string SENDER_PASSWORD = "olgs hidd qsqk dbuj";

        /// <summary>
        /// Envoie la facture et le ticket d'une commande par email.
        /// </summary>
        /// <param name="commande">L'objet Commande complet.</param>
        /// <param name="emailDestinataire">L'adresse email du client.</param>
        /// <param name="cheminFacture">Chemin du PDF de la facture.</param>
        /// <param name="cheminTicket">Chemin du PDF du ticket.</param>
        /// <returns>True si l'envoi est réussi, False sinon.</returns>
        /// // Ajoute cette méthode dans ta classe EmailService
        // Méthode corrigée à placer dans EmailService.cs
        public static bool SendReadyNotification(Order order, string destinataireEmail)
        {
            try
            {
                // 1. Configuration du message simple
                var message = new System.Net.Mail.MailMessage();

                // CORRECTION 1 : L'expéditeur doit être SENDER_EMAIL (l'adresse authentifiée)
                message.From = new System.Net.Mail.MailAddress(SENDER_EMAIL, "Pressing G3");
                message.To.Add(new System.Net.Mail.MailAddress(destinataireEmail));

                message.Subject = $"Votre commande N°{order.NumeroTicket} est prête ! ✅";

                // 2. Le corps du mail (inchangé)
                string corpsMessage = $"Bonjour {order.Client.NomComplet},\n\n" +
                                      $"Nous avons le plaisir de vous informer que votre commande (Ticket N°{order.NumeroTicket}) est prête.\n\n" +
                                      $"👕 Vous pouvez passer la récupérer dès maintenant.\n" +
                                      $"⚠️ IMPORTANT : Merci de vous munir de votre ticket de dépôt pour le retrait.\n\n" +
                                      $"Cordialement,\n" +
                                      $"L'équipe Pressing G3";

                message.Body = corpsMessage;
                message.IsBodyHtml = false;

                // 3. Configuration SMTP (On utilise les constantes globales)
                var smtp = new System.Net.Mail.SmtpClient(SMTP_HOST)
                {
                    Port = SMTP_PORT,
                    // CORRECTION 2 : On utilise SENDER_EMAIL et SENDER_PASSWORD
                    Credentials = new System.Net.NetworkCredential(SENDER_EMAIL, SENDER_PASSWORD),
                    EnableSsl = true,
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                // On affiche l'erreur détaillée pour aider au diagnostic
                System.Windows.MessageBox.Show($"Erreur d'envoi : {ex.Message}", "Erreur SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public static bool SendOrderDocuments(Order commande, string emailDestinataire, string cheminFacture, string cheminTicket)
        {
            // Valide si l'email de destination est fourni
            if (string.IsNullOrWhiteSpace(emailDestinataire))
            {
                // Si l'email est vide, on arrête sans erreur
                return false;
            }

            try
            {
                // Création du client SMTP
                using (SmtpClient smtpClient = new SmtpClient(SMTP_HOST, SMTP_PORT))
                {
                    smtpClient.EnableSsl = true;
                    // Les identifiants
                    smtpClient.Credentials = new NetworkCredential(SENDER_EMAIL, SENDER_PASSWORD);

                    // Création du message
                    MailMessage mail = new MailMessage(SENDER_EMAIL, emailDestinataire);
                    mail.Subject = $"Ticket N°{commande.NumeroTicket} - Votre Commande Pressing G3";

                    mail.Body = $"Bonjour {commande.Client.NomComplet},\n\n" +
                                $"Votre commande de pressing N°{commande.NumeroTicket} a été enregistrée avec succès.\n" +
                                $"Veuillez trouver la facture et le ticket en pièce jointe.\n\n" +
                                $"Date prévue de retrait : {commande.DatePrevu:dd/MM/yyyy}.\n\n" +
                                $"Merci de votre confiance,\nL'équipe Pressing G3.";

                    // Joindre les PDF générés
                    if (File.Exists(cheminFacture))
                        mail.Attachments.Add(new Attachment(cheminFacture));

                    if (File.Exists(cheminTicket))
                        mail.Attachments.Add(new Attachment(cheminTicket));

                    smtpClient.Send(mail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // En cas d'échec, on affiche l'erreur spécifique à l'utilisateur
                MessageBox.Show($"Erreur d'envoi d'email : {ex.Message}", "Erreur Email", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}