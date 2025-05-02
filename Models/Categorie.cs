using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Models
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Nom { get; set; }

        public  string Description { get; set; }

        // Relation avec Produit
        public ICollection<Produit> Produits { get; set; }
    }
}
