using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Enums;
using PressingG3.UI.Services; // Pour le PdfService

namespace PressingG3.UI.Views
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();

            // On met les dates par défaut (Ce mois-ci)
            DateDebut.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Le 1er du mois
            DateFin.SelectedDate = DateTime.Now;

            ChargerHistorique();
        }

        private void ChargerHistorique()
        {
            if (GridHistory == null) return;
            using (var context = new PressingDbContext())
            {
                // 1. On commence avec TOUTES les commandes
                var query = context.Orders
                                   .Include(o => o.Client)
                                   .Include(o => o.Articles) // Nécessaire pour le MontantTotal
                                   .AsQueryable(); // Permet d'ajouter des filtres petit à petit

                // 2. Filtre Date Début
                if (DateDebut.SelectedDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= DateDebut.SelectedDate.Value);
                }

                // 3. Filtre Date Fin (On ajoute 1 jour pour inclure la journée complète jusqu'à minuit)
                if (DateFin.SelectedDate.HasValue)
                {
                    var dateFinIncluse = DateFin.SelectedDate.Value.AddDays(1);
                    query = query.Where(o => o.CreatedAt < dateFinIncluse);
                }

                // 4. Filtre Statut
                if (CmbStatut.SelectedIndex > 0) // Si ce n'est pas "TOUS"
                {
                    string choix = ((ComboBoxItem)CmbStatut.SelectedItem).Content.ToString();

                    if (choix == "Livré / Payé")
                        query = query.Where(o => o.Statut == OrderStatus.Livre || o.Statut == OrderStatus.Paye);
                    else if (choix == "En Lavage")
                        query = query.Where(o => o.Statut == OrderStatus.EnLavage || o.Statut == OrderStatus.Depose);
                    else if (choix == "Prêt")
                        query = query.Where(o => o.Statut == OrderStatus.Pret);
                }

                // 5. Exécution de la requête
                var resultats = query.OrderByDescending(o => o.CreatedAt).ToList();

                // 6. Affichage
                GridHistory.ItemsSource = resultats;

                // 7. Calcul du total affiché
                decimal total = resultats.Sum(o => o.MontantTotal);
                LblTotalPeriode.Text = $"{total:N0} FCFA";
            }
        }

        // Déclenché à chaque changement de filtre
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ChargerHistorique();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            DateDebut.SelectedDate = null;
            DateFin.SelectedDate = null;
            CmbStatut.SelectedIndex = 0;
            ChargerHistorique();
        }

        // RÉIMPRESSION TICKET (Thermique)
        private void BtnReprint_Click(object sender, RoutedEventArgs e)
        {
            var commande = GridHistory.SelectedItem as Order;
            if (commande == null)
            {
                MessageBox.Show("Veuillez sélectionner une ligne.");
                return;
            }

            try { PdfService.GenererTicket(commande); }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        // RÉIMPRESSION FACTURE (A4)
        private void BtnReprintInvoice_Click(object sender, RoutedEventArgs e)
        {
            var commande = GridHistory.SelectedItem as Order;
            if (commande == null)
            {
                MessageBox.Show("Veuillez sélectionner une ligne.");
                return;
            }

            try { PdfService.GenererFacture(commande); }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }
    }
}