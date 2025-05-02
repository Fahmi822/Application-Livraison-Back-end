using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    public class Commande
    {
        public  int Id { get; set; }
        public  decimal Montant { get; set; }
        public  string Statut { get; set; }
        public DateTime DateCommande { get; set; }

        [ForeignKey("Client")]
        public  int ClientId { get; set; }
        public Client Client { get; set; }

        public ICollection<Produit> Produits { get; set; }

        public Livraison Livraison { get; set; }

        [ForeignKey("Livreur")]
        public int? LivreurId { get; set; }  // Clé étrangère vers Livreur (nullable si une commande peut ne pas être assignée à un livreur)
        public Livreur Livreur { get; set; }  // Navigation vers le Livreur
    }
}
