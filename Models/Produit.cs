using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    public class Produit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du produit est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut excéder 100 caractères")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal Prix { get; set; }

        [StringLength(1000, ErrorMessage = "La description ne peut excéder 1000 caractères")]
        public string Description { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(0, int.MaxValue, ErrorMessage = "La quantité ne peut être négative")]
        public int Quantite { get; set; }

        // Foreign key
        [Required(ErrorMessage = "La catégorie est obligatoire")]
        public int CategorieId { get; set; }

        // Navigation properties
        [ForeignKey("CategorieId")]
        public virtual Categorie Categorie { get; set; }

        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        public string Img { get; set; } // Chemin de l'image stockée

        [NotMapped]
        public IFormFile ImgUp { get; set; } // Pour l'upload (non stocké en base)
    }
}
