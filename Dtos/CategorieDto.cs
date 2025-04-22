using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Dtos
{
    public class CategorieDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; }

        public string Description { get; set; }
    }
}
