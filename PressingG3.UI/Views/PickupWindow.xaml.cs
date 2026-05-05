using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Enums;

namespace PressingG3.UI.Views
{
    public partial class PickupWindow : Window
    {
        private List<Order> _allOrders = new List<Order>();
        private Order? _selectedOrder;

        public PickupWindow()
        {
            InitializeComponent();
            ChargerCommandes();
        }

        private void ChargerCommandes()
        {
            using (var context = new PressingDbContext())
            {
                // On charge toutes les commandes qui ne sont PAS ENCORE livrées/payées
                // Include(c => c.Client) est très important pour afficher le nom du client !
                _allOrders = context.Orders
                                    .Include(o => o.Client)
                                    .Include(o => o.Articles)
                                    .Where(o => o.Statut != OrderStatus.Livre && o.Statut != OrderStatus.Paye)
                                    .OrderByDescending(o => o.CreatedAt)
                                    .ToList();

                GridCommandes.ItemsSource = _allOrders;
            }
        }

        // 1. RECHERCHE EN TEMPS RÉEL
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.ToLower();

            // On filtre la liste en mémoire
            var filtered = _allOrders.Where(o =>
                o.NumeroTicket.ToLower().Contains(query) ||
                o.Client.NomComplet.ToLower().Contains(query)
            ).ToList();

            GridCommandes.ItemsSource = filtered;
        }

        // 2. SÉLECTION D'UNE COMMANDE
        private void GridCommandes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOrder = GridCommandes.SelectedItem as Order;

            if (_selectedOrder != null)
            {
                // On met à jour l'affichage à droite
                LblClientNom.Text = _selectedOrder.Client.NomComplet;
                LblTicketNum.Text = $"Ticket : {_selectedOrder.NumeroTicket}";

                // Pour avoir les articles, on doit parfois les recharger si le LazyLoading fait défaut ou pour être sûr
                using (var context = new PressingDbContext())
                {
                    // On recharge la commande complète avec ses articles
                    var fullOrder = context.Orders
                                           .Include(o => o.Articles)
                                           .FirstOrDefault(x => x.Id == _selectedOrder.Id);

                    if (fullOrder != null)
                    {
                        ListDetails.ItemsSource = fullOrder.Articles;
                        LblTotal.Text = $"{fullOrder.MontantTotal:N0} FCFA";

                        // Activer le bouton
                        BtnAction.IsEnabled = true;
                        BtnAction.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60")); // Vert
                    }
                }
            }
            else
            {
                // Remise à zéro si rien sélectionné
                LblClientNom.Text = "Sélectionnez une commande...";
                LblTicketNum.Text = "-";
                ListDetails.ItemsSource = null;
                LblTotal.Text = "0 FCFA";
                BtnAction.IsEnabled = false;
                BtnAction.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6")); // Gris
            }
        }

        // 3. ACTION ENCAISSER & LIVRER
        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var result = MessageBox.Show($"Confirmez-vous le paiement de {_selectedOrder.MontantTotal:N0} FCFA et la livraison ?",
                                         "Paiement", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (var context = new PressingDbContext())
                {
                    // 1. On charge la commande (AVEC le client et les articles pour le PDF)
                    var orderToUpdate = context.Orders
                                               .Include(o => o.Client)
                                               .Include(o => o.Articles)
                                               .FirstOrDefault(o => o.Id == _selectedOrder.Id);

                    if (orderToUpdate != null)
                    {
                        // 2. Mise à jour du statut
                        orderToUpdate.Statut = OrderStatus.Livre; // On considère que payé = livré
                        orderToUpdate.UpdatedAt = DateTime.Now;

                        context.SaveChanges();

                        // 3. GÉNÉRATION DU JUSTIFICATIF (PDF)
                        try
                        {
                            PressingG3.UI.Services.PdfService.GenererRecuPaiement(orderToUpdate);
                        }
                        catch (Exception pdfEx)
                        {
                            MessageBox.Show("Erreur PDF : " + pdfEx.Message);
                        }

                        MessageBox.Show("✅ Transaction terminée ! Le reçu a été généré.");

                        // 4. Rafraîchissement
                        ChargerCommandes();
                        GridCommandes_SelectionChanged(null, null);
                    }
                }
            }
        }
    }
}