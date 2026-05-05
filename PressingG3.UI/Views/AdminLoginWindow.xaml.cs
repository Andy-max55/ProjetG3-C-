using System.Windows;

namespace PressingG3.UI.Views
{
    public partial class AdminLoginWindow : Window
    {
        public AdminLoginWindow()
        {
            InitializeComponent();
        }

        private void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {
            // Simulation de sécurité (On connectera à la BDD après)
            if (TxtPassword.Password == "admin123")
            {
                // 1. On ferme la fenêtre de login
                this.Hide();

                // 2. On ouvre le Tableau de bord Admin
                var adminDashboard = new AdminDashboardWindow();

                // 3. On attend qu'il soit fermé pour revenir
                adminDashboard.ShowDialog();

                // 4. Une fois l'admin fermé, on ferme aussi le login proprement
                this.Close();
            }
            else
            {
                MessageBox.Show("Accès Refusé.");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}