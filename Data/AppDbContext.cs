using Application_Livraison_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Application_Livraison_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Livreur> Livreurs { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Livraison> Livraisons { get; set; }
        public DbSet<Produit> Produits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Commande>()
                .HasMany(c => c.Produits)
                .WithMany(p => p.Commandes);

            base.OnModelCreating(modelBuilder);
        }
    }
}
