using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Enums;
using PressingG3.core.Services;
using PressingG3.UI.Services; // <--- 1. IMPORTANT : Pour accéder à OrderEvents

namespace PressingG3.UI.Views
{
    public partial class ProductionWindow : Window
    {
        public ProductionWindow()
        {
            InitializeComponent();

            if (Session.CurrentUser != null)
                LblUser.Text = $"Utilisateur : {Session.CurrentUser.Username} ({Session.CurrentUser.Role})";

            ChargerColonnes();
        }

        private void ChargerColonnes()
        {
            using (var context = new PressingDbContext())
            {
                // 1. Colonne "À LAVER"
                var todo = context.Orders.Include(o => o.Client).Include(o => o.Articles)
                                  .Where(o => o.Statut == OrderStatus.Depose)
                                  .OrderBy(o => o.DatePrevu)
                                  .ToList();
                ListTodo.ItemsSource = todo;

                // 2. Colonne "EN COURS"
                var inProgress = context.Orders.Include(o => o.Client).Include(o => o.Articles)
                                        .Where(o => o.Statut == OrderStatus.EnLavage || o.Statut == OrderStatus.Repassage)
                                        .OrderBy(o => o.UpdatedAt)
                                        .ToList();
                ListInProgress.ItemsSource = inProgress;

                // 3. Colonne "PRÊT"
                var ready = context.Orders.Include(o => o.Client).Include(o => o.Articles)
                                   .Where(o => o.Statut == OrderStatus.Pret)
                                   .OrderByDescending(o => o.UpdatedAt)
                                   .ToList();
                ListReady.ItemsSource = ready;
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Guid orderId = (Guid)button.Tag;
            ChangerStatut(orderId, OrderStatus.EnLavage);
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Guid orderId = (Guid)button.Tag;
            ChangerStatut(orderId, OrderStatus.Pret);
        }

        // --- ICI LA MODIFICATION MAJEURE ---
        private void ChangerStatut(Guid orderId, OrderStatus nouveauStatut)
        {
            using (var context = new PressingDbContext())
            {
                // On utilise Include ici car la notification aura besoin des infos du Client !
                var order = context.Orders
                                   .Include(o => o.Client)
                                   .FirstOrDefault(o => o.Id == orderId);

                if (order != null)
                {
                    order.Statut = nouveauStatut;
                    order.UpdatedAt = DateTime.Now;
                    context.SaveChanges();

                    // --- 2. LE DÉCLENCHEUR (ETAPE 3) ---
                    // Si on vient de passer la commande à "PRÊT", on envoie le signal !
                    if (nouveauStatut == OrderStatus.Pret)
                    {
                        OrderEvents.RaiseOrderCompleted(order);
                    }
                }
            }
            ChargerColonnes();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}