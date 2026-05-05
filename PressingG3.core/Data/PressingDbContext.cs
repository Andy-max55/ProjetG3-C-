using Microsoft.EntityFrameworkCore;
using PressingG3.core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PressingG3.core.Entities;
using PressingG3.core.Services; // Pour accéder à la Session
using Microsoft.EntityFrameworkCore.ChangeTracking; // Pour traquer les changements


namespace PressingG3.core.Data
{
    public class PressingDbContext : DbContext
    {
        // Voici nos futures tables SQL
        public DbSet<Client> Clients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Configuration de la connexion SQLite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Cela va créer un fichier "presspro.db" à côté de l'exécutable
            optionsBuilder.UseSqlite("Data Source=presspro.db");

            // Active le Lazy Loading (chargement automatique des données liées)
            optionsBuilder.UseLazyLoadingProxies();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Liste temporaire pour les logs
            var auditEntries = new List<AuditLog>();

            // On boucle sur tous les objets qui ont été ajoutés, modifiés ou supprimés
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue; // On n'audit pas les logs eux-mêmes, ni les entités non modifiées

                var auditEntry = new AuditLog
                {
                    UserId = Session.CurrentUser?.Id ?? Guid.Empty, // Qui a fait l'action (Guid.Empty si déconnecté)
                    ActionType = entry.State.ToString(),
                    EntityName = entry.Entity.GetType().Name,
                    CreatedAt = DateTime.Now
                };

                // On essaie de récupérer l'ID de l'entité modifiée (pour la traçabilité)
                if (entry.Entity is BaseEntity baseEntity)
                {
                    auditEntry.EntityId = baseEntity.Id.ToString();
                }

                // On enregistre les détails de la modification pour le log
                var changes = entry.Properties
                                   .Where(p => p.IsModified)
                                   .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} -> {p.CurrentValue}")
                                   .ToList();

                auditEntry.ChangeDetails = string.Join(" | ", changes);

                // On ajoute le log à la liste
                auditEntries.Add(auditEntry);
            }

            // D'abord, on enregistre les modifications des entités normales
            var result = await base.SaveChangesAsync(cancellationToken);

            // Ensuite, on enregistre les logs
            if (auditEntries.Any())
            {
                await AuditLogs.AddRangeAsync(auditEntries);
                await base.SaveChangesAsync(cancellationToken); // Seconde sauvegarde pour les logs
            }

            return result;
        }
    }
}