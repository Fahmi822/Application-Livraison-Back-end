namespace Application_Livraison_Backend.Models
{
    public class Client : Utilisateur
    {
        public string Adresse { get; set; }

        public ICollection<Commande> Commandes { get; set; }
    }
}
