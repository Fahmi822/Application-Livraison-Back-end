namespace Application_Livraison_Backend.Dtos

{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Mdp { get; set; }
    }
}
