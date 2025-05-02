using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    public class Livraison
    {
        public required  int Id { get; set; }
        public required string Statut { get; set; }
        public required DateTime DateLivraison { get; set; }
        public required bool RecRecu { get; set; }

        [ForeignKey("Commande")]
        public required int CommandeId { get; set; }
        public required Commande Commande { get; set; }

        [ForeignKey("Livreur")]
        public required int LivreurId { get; set; }
        public required Livreur Livreur { get; set; }
    }
}
