namespace Application_Livraison_Backend.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public decimal Prix { get; set; }
        public string Description { get; set; }
        public int Quantite { get; set; }

        public ICollection<Commande> Commandes { get; set; }
    }
}
