using System;
using System.IO; // <--- C'est cette ligne qui manquait pour "Path" !
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PressingG3.core.Entities;
using PressingG3.UI.Services;

namespace PressingG3.UI.Controls
{
    public partial class OrderReadyNotification : UserControl
    {
        // On initialise à null pour calmer l'erreur CS8618
        public Order? Commande { get; private set; } = null;

        public OrderReadyNotification()
        {
            InitializeComponent();
        }

        public void ShowNotification(Order order)
        {
            this.Commande = order;

            if (order != null)
            {
                TxtNotificationDetails.Text = $"Ticket N°{order.NumeroTicket} pour {order.Client.NomComplet}.";
            }

            // 1. On rend le contrôle visible globalement
            this.Visibility = Visibility.Visible;

            // 2. IMPORTANT : On rend la bordure OPAQUE (visible)
            NotificationBorder.Opacity = 1;  // <--- C'EST CA QUI MANQUAIT !

            // 3. L'animation de descente (Slide Down)
            var sb = new Storyboard();

            // On part de -100 (hors écran en haut) vers 0 (position normale)
            var animationY = new DoubleAnimation(-100, 0, TimeSpan.FromSeconds(0.5));

            // Effet rebond sympa
            animationY.EasingFunction = new BounceEase { Bounciness = 2, EasingMode = EasingMode.EaseOut };

            Storyboard.SetTargetName(animationY, NotificationBorder.Name);
            Storyboard.SetTargetProperty(animationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            sb.Children.Add(animationY);
            sb.Begin(this);
        }

        private void HideNotification()
        {
            var sb = new Storyboard();

            // On remonte vers -100
            var animationY = new DoubleAnimation(0, -100, TimeSpan.FromSeconds(0.3));

            Storyboard.SetTargetName(animationY, NotificationBorder.Name);
            Storyboard.SetTargetProperty(animationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            sb.Children.Add(animationY);

            sb.Completed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;
                NotificationBorder.Opacity = 0; // On remet invisible pour la prochaine fois
            };

            sb.Begin(this);
        }

        private void BtnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            if (Commande == null) return;

            // On désactive le bouton pour éviter de cliquer 2 fois
            BtnSendEmail.IsEnabled = false;
            BtnSendEmail.Content = "Envoi...";

            if (string.IsNullOrWhiteSpace(Commande.Client.Email))
            {
                MessageBox.Show("Le client n'a pas d'adresse email enregistrée.", "Info");
                BtnSendEmail.IsEnabled = true;
                BtnSendEmail.Content = "✉️ ENVOYER MAIL";
                return;
            }

            // --- C'EST ICI QU'ON A CHANGÉ ---
            // On appelle la nouvelle méthode "SendReadyNotification" (sans chemin de fichier)
            bool success = EmailService.SendReadyNotification(Commande, Commande.Client.Email);

            if (success)
            {
                MessageBox.Show("Le client a été notifié avec succès !", "Mail envoyé");

                // On ferme la notification automatiquement après l'envoi
                HideNotification();
            }
            else
            {
                // Si ça rate, on réactive le bouton
                BtnSendEmail.IsEnabled = true;
                BtnSendEmail.Content = "✉️ ENVOYER MAIL";
            }
        }
    }
}