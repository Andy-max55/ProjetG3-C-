using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Services;
using PressingG3.core.Enums; // J'ai ajouté ça pour faciliter l'accès aux rôles
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PressingG3.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            CreerAdminParDefaut();
            this.KeyDown += new KeyEventHandler(LoginWindow_KeyDown);
        }
        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Vérifie si la touche 'A' est enfoncée ET si la touche 'Control' est enfoncée
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // On empêche le système de traiter l'événement A
                e.Handled = true;

                // Ouvre la fenêtre d'administration
                var adminLoginWindow = new Views.AdminLoginWindow(); // Assurez-vous que le nom est correct
                adminLoginWindow.ShowDialog();
            }
        }

        private void CreerAdminParDefaut()
        {
            using (var context = new PressingDbContext())
            {
                context.Database.EnsureCreated();

                if (!context.Users.Any())
                {
                    context.Users.Add(new core.Entities.User
                    {
                        Username = "admin",
                        Password = "admin",
                        Role = UserRole.Administrateur,
                        IsActive = true // Important : on le met actif par défaut
                    });
                    context.SaveChanges();
                }
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new PressingDbContext())
            {
                // 1. On cherche l'utilisateur (et on vérifie qu'il est Actif !)
                var user = context.Users.FirstOrDefault(u => u.Username == TxtUsername.Text && u.Password == TxtPassword.Password);

                if (user != null)
                {
                    if (user.IsActive == false)
                    {
                        MessageBox.Show("Ce compte a été désactivé par l'administrateur.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // 2. SUCCÈS : On enregistre la session
                    Session.CurrentUser = user;

                    // 3. AIGUILLAGE INTELLIGENT (C'est ici que tout se joue)
                    if (user.Role == UserRole.Lavage)
                    {
                        // C'est un Laveur -> Direction l'Atelier
                        var atelierWindow = new ProductionWindow();
                        atelierWindow.Show();
                    }
                    else
                    {
                        // C'est un Admin ou un Réceptionniste -> Direction la Caisse
                        var dashboardWindow = new MainWindow();
                        dashboardWindow.Show();
                    }

                    // 4. On ferme la fenêtre de login
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // LE PASSAGE SECRET (J'ai gardé ta version Ctrl+Shift+A mais améliorée)
        // Assure-toi que dans le XAML tu as bien mis : PreviewKeyDown="Window_KeyDown"
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Raccourci F12 (Plus simple et marche à tous les coups)
            if (e.Key == Key.F12)
            {
                e.Handled = true;
                OuvrirAdminSecret();
                return;
            }

            // Ton Raccourci Ctrl + Shift + A
            if (e.Key == Key.A &&
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                e.Handled = true; // Empêche le 'a' de s'écrire dans la case
                OuvrirAdminSecret();
            }
        }

        private void OuvrirAdminSecret()
        {
            this.Hide();
            var adminWindow = new AdminLoginWindow();
            adminWindow.Owner = this;
            adminWindow.ShowDialog();
            this.Show(); // Réaffiche le login normal quand l'admin est fermé
        }
    }
}