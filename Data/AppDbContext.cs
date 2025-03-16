using Application_Livraison_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Application_Livraison_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Livraison> Livraisons { get; set; }
        public DbSet<Produit> Produits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Discriminator for inheritance mapping
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("UserType")  // Discriminator column
                .HasValue<Admin>("Admin")
                .HasValue<Client>("Client")
                .HasValue<Livreur>("Livreur");

            // Relationship between Commande and Client
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict deletion of a client if there are orders

            // Relationship between Commande and Livreur
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.Livreur)
                .WithMany()
                .HasForeignKey(c => c.LivreurId)
                .OnDelete(DeleteBehavior.SetNull);  // Allow a null Livreur

            // Many-to-many relation between Commande and Produit
            modelBuilder.Entity<Commande>()
                .HasMany(c => c.Produits)
                .WithMany(p => p.Commandes)
                .UsingEntity(j => j.ToTable("CommandeProduit"));

            // Relationship between Livraison and Commande
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.Commande)
                .WithOne(c => c.Livraison)
                .HasForeignKey<Livraison>(l => l.CommandeId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent deleting a command if there is a delivery

            // Relationship between Livraison and Livreur
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.Livreur)
                .WithMany()
                .HasForeignKey(l => l.LivreurId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Livreur if a delivery exists

            // Ensure unique email for Utilisateur
            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
