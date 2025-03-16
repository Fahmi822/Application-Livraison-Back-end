using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{
    
    public class Client : Utilisateur  // Vérifiez bien que 'Utilisateur' est votre classe de base
    {
       
        public string Adresse { get; set; }
    }
}
