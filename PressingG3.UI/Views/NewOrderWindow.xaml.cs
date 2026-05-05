using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Enums;
using PressingG3.UI.Services;
using System;
using System.Collections.ObjectModel; // Pour la liste dynamique
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PressingG3.UI.Views
{
    public partial class NewOrderWindow : Window
    {
        // Cette liste est spéciale : quand on la modifie, l'écran se met à jour tout seul
        private ObservableCollection<OrderItem> _panier = new ObservableCollection<OrderItem>();
        private decimal _totalGeneral = 0;

        public NewOrderWindow()
        {
            InitializeComponent();

            // On branche la liste visuelle sur notre liste de données
            ListPanier.ItemsSource = _panier;

            // Astuce pour afficher le prix quand on choisit un vêtement
            CmbArticleType.SelectionChanged += CmbArticleType_SelectionChanged;
        }

        // 1. Quand on change de vêtement dans la liste déroulante
        private void CmbArticleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbArticleType.SelectedItem is ComboBoxItem selectedItem)
            {
                // On récupère le prix caché dans la balise "Tag" du XAML
                TxtPrice.Text = selectedItem.Tag.ToString();
            }
        }

        // 2. Bouton "AJOUTER"
        private void BtnAddArticle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérifications de base
                if (CmbArticleType.SelectedItem == null)
                {
                    MessageBox.Show("Sélectionnez un type de vêtement.");
                    return;
                }

                // On récupère les valeurs
                string description = ((ComboBoxItem)CmbArticleType.SelectedItem).Content.ToString();
                decimal prix = decimal.Parse(TxtPrice.Text);
                int qte = int.Parse(TxtQuantity.Text);

                // On crée l'objet Ligne
                var ligne = new OrderItem
                {
                    Description = description,
                    Categorie = "Standard", // On pourra améliorer ça plus tard
                    Prix = prix,
                    Quantite = qte,
                    Remarque = TxtNotes.Text
                };

                // Ajout au panier visuel
                _panier.Add(ligne);

                // Mise à jour du total
                CalculerTotal();

                // On remet à zéro pour le prochain article
                TxtNotes.Text = "";
                TxtQuantity.Text = "1";
            }
            catch (Exception)
            {
                MessageBox.Show("Vérifiez le prix et la quantité (chiffres uniquement).");
            }
        }

        // 3. Calcul du Total
        private void CalculerTotal()
        {
            _totalGeneral = _panier.Sum(x => x.Prix * x.Quantite);
            TxtTotalGeneral.Text = $"{_totalGeneral:N0} FCFA"; // N0 met les espaces (ex: 1 000)
        }

        // 4. Bouton "VALIDER LA COMMANDE" (Le plus important)
        // Dans le fichier NewOrderWindow.xaml.cs :

        // Dans le fichier NewOrderWindow.xaml.cs :
        // Dans le fichier NewOrderWindow.xaml.cs :

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. VALIDATIONS STRICTES ---
            if (_panier.Count == 0)
            {
                MessageBox.Show("Le panier est vide !");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtClientName.Text) || string.IsNullOrWhiteSpace(TxtClientPhone.Text))
            {
                MessageBox.Show("Le nom et le téléphone du client sont obligatoires.", "Erreur");
                return;
            }
            // Vérification du format du téléphone (au moins 9 chiffres)
            if (!System.Text.RegularExpressions.Regex.IsMatch(TxtClientPhone.Text, @"^\d{9,}$"))
            {
                MessageBox.Show("Le numéro de téléphone est invalide (au moins 9 chiffres).", "Erreur");
                return;
            }
            // Vérification basique de l'email si un texte est saisi
            if (!string.IsNullOrWhiteSpace(TxtClientEmail.Text) && !TxtClientEmail.Text.Contains("@"))
            {
                MessageBox.Show("Le format de l'adresse email semble incorrect.", "Erreur");
                return;
            }


            // --- 2. TRAITEMENT & SAUVEGARDE DB ---
            try
            {
                // Récupération sécurisée du champ Email
                string emailSaisi = TxtClientEmail.Text.Trim();

                using (var context = new PressingDbContext())
                {
                    // A. GESTION DU CLIENT (Ajout/Mise à jour de l'email)
                    var client = context.Clients.FirstOrDefault(c => c.Telephone == TxtClientPhone.Text);

                    if (client == null)
                    {
                        // Nouveau client
                        client = new core.Entities.Client { NomComplet = TxtClientName.Text, Telephone = TxtClientPhone.Text, Email = emailSaisi };
                        context.Clients.Add(client);
                    }
                    else
                    {
                        // Client existant : mise à jour
                        client.NomComplet = TxtClientName.Text;
                        client.Email = emailSaisi; // <--- SAUVEGARDE EMAIL
                    }

                    // B. CRÉATION DU TICKET & SAUVEGARDE
                    string ticketNum = $"T-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
                    var commande = new core.Entities.Order
                    {
                        NumeroTicket = ticketNum,
                        Client = client,
                        Statut = core.Enums.OrderStatus.Depose,
                        DatePrevu = DateTime.Now.AddDays(2),
                        CreatedAt = DateTime.Now
                    };

                    // C. SAUVEGARDE COMMANDE
                    foreach (var item in _panier)
                    {
                        item.Order = commande;
                        context.OrderItems.Add(item);
                    }
                    context.Orders.Add(commande);
                    context.SaveChanges();

                    // --- 3. GÉNÉRATION DES DOCUMENTS ET ENVOI AUTOMATIQUE ---

                    string cheminFacture = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PressingG3_Factures", $"Facture_{commande.NumeroTicket}.pdf");
                    string cheminTicket = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PressingG3_Tickets", $"Ticket_{commande.NumeroTicket}.pdf");

                    // Assurez-vous que les dossiers existent
                    Directory.CreateDirectory(Path.GetDirectoryName(cheminFacture)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(cheminTicket)!);

                    // Génération des PDF
                    PressingG3.UI.Services.PdfService.GenererFacture(commande);
                    PressingG3.UI.Services.PdfService.GenererTicket(commande);

                    // Envoi de l'email (si l'adresse a été fournie et est valide)
                    bool emailSent = false;
                    if (!string.IsNullOrWhiteSpace(client.Email))
                    {
                        emailSent = PressingG3.UI.Services.EmailService.SendOrderDocuments(commande, client.Email, cheminFacture, cheminTicket);
                    }

                    // Confirmation finale
                    string msg = $"✅ Commande enregistrée : {ticketNum}\nTotal : {_totalGeneral:N0} FCFA";
                    if (emailSent)
                        msg += "\nEmail du ticket envoyé au client.";
                    else if (!string.IsNullOrWhiteSpace(client.Email) && !emailSent)
                        msg += "\nÉCHEC de l'envoi de l'email (Vérifiez vos identifiants SMTP !).";

                    MessageBox.Show(msg);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur technique lors de la validation : {ex.Message}");
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // On récupère l'article lié au bouton cliqué
            var button = sender as Button;
            var articleASupprimer = button.DataContext as OrderItem;

            if (articleASupprimer != null)
            {
                // On l'enlève de la liste
                _panier.Remove(articleASupprimer);

                // On recadre le total
                CalculerTotal();
            }
        }

        // Modifier une ligne (On la supprime de la liste et on la remet dans le formulaire)
        private void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var articleAModifier = button.DataContext as OrderItem;

            if (articleAModifier != null)
            {
                // 1. On remet les infos dans les champs de gauche pour correction

                // Pour le Type, on essaie de retrouver le bon item dans la ComboBox
                foreach (ComboBoxItem item in CmbArticleType.Items)
                {
                    if (item.Content.ToString() == articleAModifier.Description)
                    {
                        CmbArticleType.SelectedItem = item;
                        break;
                    }
                }

                // On remet le prix, la quantité et la note
                TxtPrice.Text = articleAModifier.Prix.ToString();
                TxtQuantity.Text = articleAModifier.Quantite.ToString();
                TxtNotes.Text = articleAModifier.Remarque;

                // 2. On supprime l'ancienne ligne du panier (pour ne pas l'avoir en double)
                _panier.Remove(articleAModifier);

                // 3. Recalcul du total
                CalculerTotal();

                MessageBox.Show("L'article a été remis dans le formulaire.\nModifiez-le puis cliquez sur 'AJOUTER' à nouveau.", "Modification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}