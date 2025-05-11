using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    public class Livraison
    {
        public int Id { get; set; }
        public required string Statut { get; set; }
        public required DateTime DateLivraison { get; set; }
        public required bool RecRecu { get; set; }

        [ForeignKey("Commande")]
        public  int CommandeId { get; set; }
        public  Commande Commande { get; set; }

        [ForeignKey("Livreur")]
        public required int LivreurId { get; set; }
        public Livreur Livreur { get; set; }
    }
}
