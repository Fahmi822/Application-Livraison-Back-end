namespace Application_Livraison_Backend.Dtos
{
    
public class CommandeAssignableDto
    {
        public int Id { get; set; }
        public string Statut { get; set; }
        public DateTime DateCommande { get; set; }
        public string ClientNom { get; set; }
        public string AdresseLivraison { get; set; }
        public decimal Montant { get; set; }
    }
}
