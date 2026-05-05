using System.Configuration;
using System.Data;
using System.Windows;
using QuestPDF.Infrastructure;

namespace PressingG3.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // On active la licence gratuite
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }

}
