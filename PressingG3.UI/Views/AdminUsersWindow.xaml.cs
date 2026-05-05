using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Enums;

namespace PressingG3.UI.Views
{
    public partial class AdminUsersWindow : Window
    {
        private User? _selectedUser;

        public AdminUsersWindow()
        {
            InitializeComponent();
            ChargerUsers();
        }

        private void ChargerUsers()
        {
            using (var context = new PressingDbContext())
            {
                GridUsers.ItemsSource = context.Users.Where(u => u.IsActive).ToList();
            }
        }

        // SAUVEGARDER (AJOUT OU MODIF)
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Vérifications basiques
            if (string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtPassword.Text) || CmbRole.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir l'identifiant, le mot de passe et le rôle.");
                return;
            }

            try
            {
                using (var context = new PressingDbContext())
                {
                    // Récupération sécurisée du rôle (Fixe les erreurs de conversion)
                    if (CmbRole.SelectedItem is not ComboBoxItem selectedRoleItem) return;

                    string roleString = selectedRoleItem.Content.ToString()!; // Le '!' est utilisé pour dire au compilateur qu'on est sûr que c'est une string

                    if (!Enum.TryParse<UserRole>(roleString, out UserRole roleEnum))
                    {
                        MessageBox.Show("Erreur de conversion de rôle interne.");
                        return;
                    }

                    if (_selectedUser == null)
                    {
                        // --- CRÉATION ---
                        if (context.Users.Any(u => u.Username == TxtUsername.Text))
                        {
                            MessageBox.Show("Cet identifiant existe déjà !");
                            return;
                        }

                        var newUser = new User
                        {
                            Username = TxtUsername.Text,
                            Password = TxtPassword.Text,
                            Role = roleEnum,
                            IsActive = true
                        };
                        context.Users.Add(newUser);
                    }
                    else
                    {
                        // --- MODIFICATION ---
                        var userToUpdate = context.Users.Find(_selectedUser.Id);
                        if (userToUpdate != null)
                        {
                            userToUpdate.Username = TxtUsername.Text;
                            userToUpdate.Password = TxtPassword.Text;
                            userToUpdate.Role = roleEnum;
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Utilisateur enregistré avec succès !");

                    BtnNew_Click(null, null);
                    ChargerUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        // REMPLIR LE FORMULAIRE AU CLIC
        private void GridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = GridUsers.SelectedItem as User;
            if (_selectedUser != null)
            {
                TxtUsername.Text = _selectedUser.Username;
                TxtPassword.Text = _selectedUser.Password;

                // Sélectionner le bon rôle dans la liste déroulante
                foreach (ComboBoxItem item in CmbRole.Items)
                {
                    if (item.Content.ToString() == _selectedUser.Role.ToString())
                    {
                        CmbRole.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // BOUTON NOUVEAU (VIDER CHAMPS)
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            _selectedUser = null;
            TxtUsername.Text = string.Empty;
            TxtPassword.Text = string.Empty;
            CmbRole.SelectedIndex = -1;
            GridUsers.SelectedItem = null;
        }

        // SUPPRIMER (DÉSACTIVER)
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null) return;

            if (MessageBox.Show($"Supprimer l'accès pour {_selectedUser.Username} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var context = new PressingDbContext())
                {
                    var u = context.Users.Find(_selectedUser.Id);
                    if (u != null)
                    {
                        u.IsActive = false;
                        context.SaveChanges();
                    }
                }
                BtnNew_Click(null, null);
                ChargerUsers();
            }
        }
    }
}