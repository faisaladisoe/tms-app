using Microsoft.EntityFrameworkCore;
using TransportManagementSystem.Models;

namespace TransportManagementSystem.Data
{
    public class TmsDbContext : DbContext
    {
        public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }
        public DbSet<Expedition> Expeditions => Set<Expedition>();
        public DbSet<Truck> Trucks => Set<Truck>();
        public DbSet<Models.Route> Routes => Set<Models.Route>();
        public DbSet<Operation> Operations => Set<Operation>();
        public DbSet<Product> Products => Set<Product>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Expedition model
            modelBuilder.Entity<Expedition>(e =>
            {
                e.Property(p => p.Name).IsRequired();

                e.HasIndex(p => p.Id);
                e.HasIndex(p => p.Name).IsUnique();
            });

            // Truck model
            modelBuilder.Entity<Truck>(e =>
            {
                e.Property(p => p.Type).IsRequired();
                e.Property(p => p.Tonnage).IsRequired();
                e.Property(p => p.Volume).IsRequired();
                e.Property(p => p.ExpeditionId).IsRequired();

                e.HasIndex(p => p.Id);
                e.HasIndex(p => p.Type).IsUnique();
            });

            // Route model
            modelBuilder.Entity<Models.Route>(e =>
            {
                e.Property(p => p.Name).IsRequired();
                e.Property(p => p.Abbr).IsRequired();
                e.Property(p => p.Distance).IsRequired();

                e.HasIndex(p => p.Id);
                e.HasIndex(p => new { p.Name, p.Abbr }).IsUnique();
                e.HasIndex(p => p.Name).IsUnique();
                e.HasIndex(p => p.Abbr).IsUnique();
            });

            // Operation model
            modelBuilder.Entity<Operation>(e =>
            {
                e.Property(p => p.Rate).IsRequired();
                e.Property(p => p.ExpeditionId).IsRequired();
                e.Property(p => p.RouteId).IsRequired();

                e.HasIndex(p => p.Id);
            });

            // Product model
            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.Code).IsRequired();
                e.Property(p => p.Description).IsRequired();
                e.Property(p => p.Size).IsRequired();
                e.Property(p => p.Dimension).IsRequired();
                e.Property(p => p.BoxPerPallet).IsRequired();
                e.Property(p => p.GrossWeight).IsRequired();

                e.HasIndex(p => p.Id);
                e.HasIndex(p => p.Code).IsUnique();
            });
            
            // Expedition -> Truck (one-to-many, 1:N)
            modelBuilder.Entity<Models.Expedition>()
                .HasMany(e => e.Trucks)
                .WithOne(t => t.Expedition)
                .HasForeignKey(t => t.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Expedition -> Operation (one-to-many, 1:N)
            modelBuilder.Entity<Models.Expedition>()
                .HasMany(e => e.Operations)
                .WithOne(o => o.Expedition)
                .HasForeignKey(o => o.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Route -> Operation (one-to-many, 1:N)
            modelBuilder.Entity<Models.Route>()
                .HasMany(r => r.Operations)
                .WithOne(o => o.Route)
                .HasForeignKey(o => o.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Operation composite key (ExpeditionId, RouteId)
            modelBuilder.Entity<Operation>()
                .HasIndex(o => new { o.ExpeditionId, o.RouteId })
                .IsUnique();

            // Explicit FK constraints
            modelBuilder.Entity<Operation>()
                .HasOne(o => o.Expedition)
                .WithMany(e => e.Operations)
                .HasForeignKey(o => o.ExpeditionId);

            modelBuilder.Entity<Operation>()
                .HasOne(o => o.Route)
                .WithMany(r => r.Operations)
                .HasForeignKey(o => o.RouteId);
        }
    }
}
