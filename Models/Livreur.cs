using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application_Livraison_Backend.Models
{

    public class Livreur : Utilisateur
    {
        
        public string Vehicule { get; set; }
    }
}
