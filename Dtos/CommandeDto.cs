using Application_Livraison_Backend.DTOs;
using System;
using System.Collections.Generic;

namespace Application_Livraison_Backend.Dtos
{
    public class CommandeDto
    {
        public  int ClientId { get; set; }
        public  List<int> ProduitsIds { get; set; }
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string Statut { get; set; }
        public DateTime DateCommande { get; set; }

        public string ClientNomComplet { get; set; }  // Ou séparer en Nom et Prénom

        // Produits info
        public List<ProduitDto> Produits { get; set; }

        // Livraison info (optionnel)
        public int? LivreurId { get; set; }
        public string LivreurNom { get; set; }
    }

}
