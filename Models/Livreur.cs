namespace Application_Livraison_Backend.Models
{
    public class Livreur : Utilisateur
    {
        public bool Disponibilite { get; set; }
        public string Vehicule { get; set; }

        public ICollection<Livraison> Livraisons { get; set; }
    }
}
