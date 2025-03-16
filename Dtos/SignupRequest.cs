namespace Application_Livraison_Backend.Dtos
{
    public class SignupRequest
    {
        public string Nom { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string Mdp { get; set; }  // Mot de passe
    }
}
