using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Enums;

namespace PressingG3.UI.Views
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();
            ChargerStatistiques(); // On charge les chiffres dès l'ouverture de la fenêtre
        }

        // Cette méthode calcule les chiffres et met à jour l'écran
        private void ChargerStatistiques()
        {
            try
            {
                using (var context = new PressingDbContext())
                {
                    // 1. Calculer la Recette du Jour
                    // On prend toutes les commandes créées aujourd'hui (à partir de minuit)
                    DateTime aujourdhui = DateTime.Today;

                    var commandesDuJour = context.Orders
                                                 .Include(o => o.Articles) // Important pour avoir le prix
                                                 .Where(o => o.CreatedAt >= aujourdhui)
                                                 .ToList();

                    decimal recette = commandesDuJour.Sum(o => o.MontantTotal);
                    LblRecetteJour.Text = $"{recette:N0} FCFA";

                    // 2. Compter les tickets "En Lavage"
                    // On compte ceux qui sont soit "EnLavage", soit "Repassage"
                    int enCours = context.Orders.Count(o => o.Statut == OrderStatus.EnLavage || o.Statut == OrderStatus.Repassage);
                    LblEnLavage.Text = $"{enCours} Tickets";

                    // 3. Compter les Retards
                    // Commandes non livrées dont la date prévue est dépassée
                    int retards = context.Orders.Count(o => o.Statut != OrderStatus.Livre &&
                                                            o.Statut != OrderStatus.Paye &&
                                                            o.DatePrevu < DateTime.Now);
                    LblRetards.Text = retards.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des stats : " + ex.Message);
            }
        }

        // BOUTON : Vue d'ensemble (Sert à rafraîchir les chiffres)
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ChargerStatistiques();
        }

        // BOUTON : Gérer Utilisateurs (Ouvre la fenêtre de gestion du personnel)
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            var usersWindow = new AdminUsersWindow();
            usersWindow.ShowDialog();
        }

        // BOUTON : Journaux (Pas encore codé)
        private void BtnLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Module 'Journaux' en construction...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // BOUTON : Configuration (Pas encore codé)
        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Module 'Configuration Prix' en construction...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // BOUTON : Quitter
        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}