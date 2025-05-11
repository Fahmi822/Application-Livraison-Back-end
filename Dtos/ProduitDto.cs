using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.DTOs
{
    public class ProduitDto
    {
        public int? Id { get; set; }  // Nullable pour les créations

        [Required(ErrorMessage = "Le nom du produit est obligatoire")]
        [StringLength(100, MinimumLength = 3,
             ErrorMessage = "Le nom doit contenir entre 3 et 100 caractères")]
        public required string Nom { get; set; }

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [Range(0.01, 100000,
             ErrorMessage = "Le prix doit être entre 0.01 et 100 000")]
        public required decimal Prix { get; set; }

        [StringLength(1000,
             ErrorMessage = "La description ne peut excéder 1000 caractères")]
        public  string Description { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(0, 10000,
             ErrorMessage = "La quantité doit être entre 0 et 10 000")]
        public int Quantite { get; set; }

        public  int CategorieId { get; set; }

        // Correction: Soit vous gardez IFormFile sans StringLength, soit vous utilisez string pour le chemin
        public  IFormFile ImgUp { get; set; } // Pour l'upload de fichier

        public  string Img { get; set; } // Pour le chemin de l'image stockée
    }
}