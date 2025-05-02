using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Dtos
{
    public class CategorieDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Nom { get; set; }

        public required string Description { get; set; }
    }
}
