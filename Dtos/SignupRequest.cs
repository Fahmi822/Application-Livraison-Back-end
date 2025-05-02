
using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Dtos
{
    public class SignupRequest
    {
        public required string Nom { get; set; }
        public required string Email { get; set; }
        public required string Tel { get; set; }
        public required string Mdp { get; set; }
    }

}
