using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    public class Livraison
    {
        public int Id { get; set; }
        public string Statut { get; set; }
        public DateTime DateLivraison { get; set; }
        public bool RecRecu { get; set; }

        [ForeignKey("Commande")]
        public int CommandeId { get; set; }
        public Commande Commande { get; set; }

        [ForeignKey("Livreur")]
        public int LivreurId { get; set; }
        public Livreur Livreur { get; set; }
    }
}
