using Microsoft.EntityFrameworkCore;
using ScreenTimeMonitor.Models;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace ScreenTimeMonitor.Data
{
    /// <summary>
    /// Entity Framework DbContext for the Screen Time Monitor application
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<UsageSession> UsageSessions { get; set; } = null!;
        public DbSet<DailySummary> DailySummaries { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Store database in the app's local data folder
                var localFolder = ApplicationData.Current.LocalFolder.Path;
                var dbPath = Path.Combine(localFolder, "ScreenTimeMonitor.db");
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Application entity
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Category).HasDefaultValue("Uncategorized");
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure UsageSession entity
            modelBuilder.Entity<UsageSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.HasIndex(e => new { e.Date, e.ApplicationId });
                
                // Configure relationship
                entity.HasOne(e => e.Application)
                      .WithMany(a => a.UsageSessions)
                      .HasForeignKey(e => e.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DailySummary entity
            modelBuilder.Entity<DailySummary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.HasIndex(e => e.Date).IsUnique();
                entity.Property(e => e.ProductivityScore).HasDefaultValue(0.0);
            });

            // Seed data for common application categories
            SeedApplicationCategories(modelBuilder);
        }

        private static void SeedApplicationCategories(ModelBuilder modelBuilder)
        {
            // This would typically be done through migrations in a production app
            // For now, we'll handle category assignment in the service layer
        }

        /// <summary>
        /// Ensure the database is created and migrations are applied
        /// </summary>
        public async Task EnsureDatabaseCreatedAsync()
        {
            await Database.EnsureCreatedAsync();
        }
    }
}
