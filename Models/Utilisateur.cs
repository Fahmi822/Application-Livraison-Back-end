using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Models
{
    public class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Tel { get; set; }

        [Required]
        public string Mdp { get; set; }
    }
}
