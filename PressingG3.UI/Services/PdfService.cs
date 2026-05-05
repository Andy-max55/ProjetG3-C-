using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PressingG3.core.Entities;
using System.IO;
using System.Diagnostics;
using System;
using System.Linq;

namespace PressingG3.UI.Services
{
    public static class PdfService
    {
        // Méthode principale : Génère la Facture A4
        public static void GenererFacture(Order commande)
        {
            // 1. Définir le document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // En-tête (Logo + Infos)
                    page.Header().Row(row =>
                    {
                        // Gauche : Logo Entreprise
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PRESSING G3").SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                            col.Item().Text("Yaoundé, Cameroun").FontSize(10);
                            col.Item().Text("Tél: 699 00 00 00").FontSize(10);
                            col.Item().Text("Email: contact@pressingg3.cm").FontSize(10);
                        });

                        // Droite : Infos Facture
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("FACTURE / REÇU").FontSize(20).SemiBold().FontColor(Colors.Grey.Darken2);
                            col.Item().AlignRight().Text($"N° Ticket : {commande.NumeroTicket}").FontSize(12);
                            col.Item().AlignRight().Text($"Date : {commande.CreatedAt:dd/MM/yyyy HH:mm}");
                        });
                    });

                    // Contenu
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Infos Client
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                        {
                            c.Item().Text("CLIENT").SemiBold().FontSize(10).FontColor(Colors.Grey.Medium);
                            c.Item().Text(commande.Client.NomComplet.ToUpper()).Bold().FontSize(14);
                            c.Item().Text($"Tél: {commande.Client.Telephone}");
                        });

                        col.Item().Height(1, Unit.Centimetre); // Espace

                        // Tableau des articles
                        col.Item().Table(table =>
                        {
                            // Définition des colonnes
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Description large
                                columns.RelativeColumn();  // Prix
                                columns.RelativeColumn();  // Qté
                                columns.RelativeColumn();  // Total
                            });

                            // En-tête du tableau
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Désignation");
                                header.Cell().Element(CellStyle).AlignRight().Text("Prix Unit.");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Qté");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(5);
                                }
                            });

                            // Lignes du tableau
                            foreach (var item in commande.Articles)
                            {
                                table.Cell().Element(CellStyle).Text(item.Description);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Prix:N0}");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantite}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(item.Prix * item.Quantite):N0}");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(5);
                                }
                            }
                        });

                        col.Item().Height(1, Unit.Centimetre);

                        // Total
                        col.Item().AlignRight().Row(row =>
                        {
                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().Text("TOTAL À PAYER :").FontSize(12);
                                c.Item().Text("Date Prévue :").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(100).AlignRight().Column(c =>
                            {
                                c.Item().Text($"{commande.MontantTotal:N0} FCFA").Bold().FontSize(16).FontColor(Colors.Red.Medium);
                                c.Item().Text($"{commande.DatePrevu:dd/MM/yyyy}").FontSize(10);
                            });
                        });
                    });

                    // Pied de page
                    page.Footer().AlignCenter().Column(col =>
                    {
                        col.Item().Text("Merci de votre confiance !").FontSize(12).Italic();
                        col.Item().Text("En cas de perte du ticket, la carte d'identité sera exigée.").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            // 2. Sauvegarder et Ouvrir
            string dossier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PressingG3_Factures");
            Directory.CreateDirectory(dossier); // Crée le dossier s'il n'existe pas

            string cheminFichier = Path.Combine(dossier, $"Facture_{commande.NumeroTicket}.pdf");

            document.GeneratePdf(cheminFichier);

            // Ouvrir le PDF automatiquement
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(cheminFichier) { UseShellExecute = true };
            p.Start();
        }
        // AJOUTER CETTE MÉTHODE DANS PdfService.cs

        public static void GenererRecuPaiement(Order commande)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape()); // Format A5 Paysage (économique et pro)
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // En-tête
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PRESSING G3").SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                            col.Item().Text("REÇU DE PAIEMENT & LIVRAISON").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"N° : {commande.NumeroTicket}").FontSize(12);
                            col.Item().Text($"Date Paiement : {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                    });

                    // Contenu
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Infos Client
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(r =>
                        {
                            r.RelativeItem().Text($"CLIENT : {commande.Client.NomComplet.ToUpper()}");
                            r.RelativeItem().AlignRight().Text($"TÉL : {commande.Client.Telephone}");
                        });

                        col.Item().Height(10);

                        // Mention PAYÉ (Le "Tampon")
                        col.Item().Stack(stack =>
                        {
                            // Le texte de fond
                            stack.Item().AlignCenter().Text("TRANSACTION CLÔTURÉE").FontSize(10).FontColor(Colors.Grey.Lighten1);

                            // Le gros tampon vert par dessus
                            stack.Item().AlignCenter().Border(3).BorderColor(Colors.Green.Medium).PaddingVertical(5).PaddingHorizontal(20).Text("PAYÉ & LIVRÉ")
                                .FontSize(24).Bold().FontColor(Colors.Green.Medium);
                        });

                        col.Item().Height(10);

                        // Détail chiffré
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Désignation");
                                header.Cell().AlignRight().Text("Montant");
                            });

                            table.Cell().Text("Prestations de Pressing (Voir détail ticket)");
                            table.Cell().AlignRight().Text($"{commande.MontantTotal:N0} FCFA");

                            table.Cell().Text("TVA (0%)");
                            table.Cell().AlignRight().Text("0 FCFA");

                            table.Cell().Element(c => c.BorderTop(1)).Text("NET PAYÉ").Bold();
                            table.Cell().Element(c => c.BorderTop(1)).AlignRight().Text($"{commande.MontantTotal:N0} FCFA").Bold().FontColor(Colors.Green.Medium);
                        });
                    });

                    // Pied de page
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Signature Client : _________________").FontSize(8);
                        row.RelativeItem().AlignRight().Text("Cachet du Pressing").FontSize(8);
                    });
                });
            });

            // Sauvegarde et Ouverture
            string dossier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PressingG3_Recus");
            Directory.CreateDirectory(dossier);
            string chemin = Path.Combine(dossier, $"Recu_{commande.NumeroTicket}.pdf");

            document.GeneratePdf(chemin);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(chemin) { UseShellExecute = true };
            p.Start();
        }

        // Méthode pour le Ticket Client (Format 80mm pour imprimante thermique)
        public static void GenererTicket(Order commande)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(80, 200, Unit.Millimetre)); // Format Ticket
                    page.Margin(5, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("PRESSING G3").Bold().FontSize(14);
                        col.Item().AlignCenter().Text("Ticket Client").FontSize(8);
                        col.Item().PaddingVertical(5).LineHorizontal(1);
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Ticket: {commande.NumeroTicket}");
                        col.Item().Text($"Date: {commande.CreatedAt:dd/MM HH:mm}");
                        col.Item().Text($"Client: {commande.Client.NomComplet}");

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        foreach (var item in commande.Articles)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{item.Quantite}x {item.Description}");
                                row.AutoItem().Text($"{(item.Prix * item.Quantite):N0}");
                            });
                        }

                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        col.Item().AlignRight().Text($"TOTAL: {commande.MontantTotal:N0} FCFA").Bold().FontSize(12);
                        col.Item().AlignCenter().Text($"Retrait le: {commande.DatePrevu:dd/MM}").Bold();
                    });
                });
            });

            string dossier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PressingG3_Tickets");
            Directory.CreateDirectory(dossier);
            string chemin = Path.Combine(dossier, $"Ticket_{commande.NumeroTicket}.pdf");
            document.GeneratePdf(chemin);

            // Ouvrir aussi le ticket
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(chemin) { UseShellExecute = true };
            p.Start();
        }
    }
}