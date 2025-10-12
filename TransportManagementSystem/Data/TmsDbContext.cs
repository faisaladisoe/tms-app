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
