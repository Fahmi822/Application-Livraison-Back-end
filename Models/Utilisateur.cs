using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Models
{
    public class Utilisateur
    {
        [Key]
        public  int Id { get; set; }

        [Required]
        public required string Nom { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Tel { get; set; }

        [Required]
        public required string Mdp { get; set; }
        public string Adresse { get; set; }
        public string Vehicule { get; set; }
        public string ImageUrl { get; set; }

        public required string Role { get; set; }
    }
}
