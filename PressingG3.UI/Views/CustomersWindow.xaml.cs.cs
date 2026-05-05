using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Entities;

namespace PressingG3.UI.Views
{
    public partial class CustomersWindow : Window
    {
        // Une liste pour stocker nos clients modifiables
        private List<ClientViewModel> _clientsAffiches = new List<ClientViewModel>();

        public CustomersWindow()
        {
            InitializeComponent();
            ChargerClients();
        }

        private void ChargerClients()
        {
            using (var context = new PressingDbContext())
            {
                // 1. On récupère les clients ET leurs commandes
                var clientsDB = context.Clients
                                       .Include(c => c.Commandes) // Important pour les stats
                                       .ThenInclude(o => o.Articles) // Pour calculer le total
                                       .OrderBy(c => c.NomComplet)
                                       .ToList();

                // 2. On transforme ça en objets faciles à afficher
                _clientsAffiches = clientsDB.Select(c => new ClientViewModel
                {
                    Id = c.Id,
                    NomComplet = c.NomComplet,
                    Telephone = c.Telephone,
                    Adresse = c.Adresse,
                    // Calcul des stats
                    NombreCommandes = c.Commandes.Count,
                    TotalDepense = c.Commandes.Sum(o => o.MontantTotal)
                }).ToList();

                // 3. Affichage
                GridClients.ItemsSource = _clientsAffiches;
                LblTotalClients.Text = _clientsAffiches.Count.ToString();
            }
        }

        // RECHERCHE
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.ToLower();

            var filtre = _clientsAffiches.Where(c =>
                (c.NomComplet != null && c.NomComplet.ToLower().Contains(query)) ||
                (c.Telephone != null && c.Telephone.Contains(query))
            ).ToList();

            GridClients.ItemsSource = filtre;
        }

        // SAUVEGARDE DES MODIFICATIONS
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new PressingDbContext())
                {
                    int compteur = 0;

                    // On parcourt notre liste affichée pour voir les changements
                    foreach (var clientVM in _clientsAffiches)
                    {
                        // On récupère le vrai client en base
                        var clientReel = context.Clients.Find(clientVM.Id);

                        if (clientReel != null)
                        {
                            // On vérifie si ça a changé
                            if (clientReel.NomComplet != clientVM.NomComplet ||
                                clientReel.Telephone != clientVM.Telephone ||
                                clientReel.Adresse != clientVM.Adresse)
                            {
                                // Mise à jour
                                clientReel.NomComplet = clientVM.NomComplet;
                                clientReel.Telephone = clientVM.Telephone;
                                clientReel.Adresse = clientVM.Adresse;
                                clientReel.UpdatedAt = DateTime.Now;
                                compteur++;
                            }
                        }
                    }

                    if (compteur > 0)
                    {
                        context.SaveChanges();
                        MessageBox.Show($"{compteur} client(s) mis à jour avec succès !");
                        ChargerClients(); // On recharge pour être propre
                    }
                    else
                    {
                        MessageBox.Show("Aucune modification détectée.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }

    // Petite classe spéciale pour l'affichage (ViewModel)
    public class ClientViewModel
    {
        public Guid Id { get; set; }
        public string NomComplet { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string? Adresse { get; set; }

        // Données calculées (Lecture seule)
        public int NombreCommandes { get; set; }
        public decimal TotalDepense { get; set; }
    }
}