using Application_Livraison_Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public bool EstVue { get; set; } = false;
    public string Type { get; set; } // "Annulation", "Assignation", "Livraison", etc.

    // Relation avec Commande
    public int? CommandeId { get; set; }
    [ForeignKey("CommandeId")]
    public Commande? Commande { get; set; }

    // Liens vers les utilisateurs
    public int? ExpediteurId { get; set; }
    public string ExpediteurType { get; set; } // "Admin", "Client", "Livreur"
    public int? DestinataireId { get; set; }
    public string DestinataireType { get; set; }
}