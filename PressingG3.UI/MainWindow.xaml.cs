using System.Windows;
using System.Windows.Input;
using PressingG3.UI.Views;
using PressingG3.UI.Services;

namespace PressingG3.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ABONNEMENT : On écoute le "Haut-parleur"
            // Note : On utilise directement NotificationArea (créé par le XAML), pas besoin de le déclarer !
            OrderEvents.OnOrderCompleted += (order) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    // Si la notification est cachée ou inexistante, on s'assure qu'elle s'affiche
                    if (NotificationArea != null)
                    {
                        NotificationArea.ShowNotification(order);
                    }
                });
            };
        }

        // --- NAVIGATION ---

        private void BtnNewOrder_Click(object sender, RoutedEventArgs e)
        {
            var newOrderWindow = new Views.NewOrderWindow();
            newOrderWindow.ShowDialog();
        }

        private void BtnPickup_Click(object sender, RoutedEventArgs e)
        {
            var pickupWindow = new Views.PickupWindow();
            pickupWindow.ShowDialog();
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            var clientsWindow = new Views.CustomersWindow();
            clientsWindow.ShowDialog();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new Views.HistoryWindow();
            historyWindow.ShowDialog();
        }
        private void BtnTestProduction_Click(object sender, RoutedEventArgs e)
        {
            // IMPORTANT : On utilise .Show() et pas .ShowDialog()
            // .Show() permet de continuer à utiliser la fenêtre principale en même temps !
            var productionWindow = new Views.ProductionWindow();
            productionWindow.Show();
        }
    }
}