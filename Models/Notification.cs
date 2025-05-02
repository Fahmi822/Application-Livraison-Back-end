namespace Application_Livraison_Backend.Models
{
    public class Notification
    {
        public  int Id { get; set; }
        public string Message { get; set; }
        public  DateTime Date { get; set; }
        public   bool EstVue { get; set; } = false; // Est-ce que l'admin l'a vue ?
        public  string Type { get; set; } // "Annulation" ou "Réclamation"
        public int? CommandeId { get; set; } // liée à une commande
    }

}
