using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application_Livraison_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommandeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("passer")]
        public async Task<IActionResult> PasserCommande([FromBody] CommandeDto commandeDto)
        {
            // Vérification de l'existence du client
            var client = await _context.Utilisateurs.OfType<Client>()
                .FirstOrDefaultAsync(c => c.Id == commandeDto.ClientId);

            if (client == null)
                return NotFound(new { message = "Client non trouvé." });

            // Récupération des produits
            var produits = await _context.Produits
                .Where(p => commandeDto.ProduitsIds.Contains(p.Id))
                .ToListAsync();

            if (produits.Count != commandeDto.ProduitsIds.Count)
                return BadRequest(new { message = "Un ou plusieurs produits sont introuvables." });

            // Calcul du montant total
            decimal montantTotal = produits.Sum(p => p.Prix); // Assurez-vous que chaque produit a un prix.

            // Création de la commande
            var commande = new Commande
            {
                ClientId = client.Id,
                Produits = produits,
                DateCommande = DateTime.Now,
                Montant = montantTotal,
                Statut = "En attente" // Statut par défaut
            };

            // Ajout et sauvegarde de la commande
            _context.Commandes.Add(commande);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Commande passée avec succès.",
                commandeId = commande.Id,
                montant = commande.Montant,
                statut = commande.Statut,
                date = commande.DateCommande
            });
        }

        // Optionnel : récupérer toutes les commandes
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var commandes = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.Produits)
                .Include(c => c.Livreur)  // Si le livreur est assigné à la commande
                .ToListAsync();

            return Ok(commandes);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetCommandesByClient(int clientId)
        {
            var commandes = await _context.Commandes
                .Where(c => c.ClientId == clientId)
                .Include(c => c.Produits)
                .ToListAsync();

            return Ok(commandes);
        }
        [HttpPut("annuler/{commandeId}")]
        public async Task<IActionResult> AnnulerCommande(int commandeId)
        {
            var commande = await _context.Commandes.FindAsync(commandeId);

            if (commande == null)
                return NotFound(new { message = "Commande non trouvée." });

            if (commande.Statut == "Annulée")
                return BadRequest(new { message = "Commande déjà annulée." });

            commande.Statut = "Annulée";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Commande annulée avec succès." });
        }

        [HttpPut("assigner-livreur/{commandeId}")]
        public async Task<IActionResult> AssignerLivreur(int commandeId, [FromQuery] int livreurId)
        {
            var commande = await _context.Commandes.FindAsync(commandeId);
            var livreur = await _context.Utilisateurs.OfType<Livreur>().FirstOrDefaultAsync(l => l.Id == livreurId);

            if (commande == null)
                return NotFound(new { message = "Commande non trouvée." });

            if (livreur == null)
                return NotFound(new { message = "Livreur non trouvé." });

            commande.LivreurId = livreurId;
            commande.Statut = "En cours de livraison";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Livreur assigné avec succès à la commande." });
        }
        [HttpPut("livrer/{commandeId}")]
        public async Task<IActionResult> MarquerCommeLivree(int commandeId)
        {
            var commande = await _context.Commandes.FindAsync(commandeId);

            if (commande == null)
                return NotFound(new { message = "Commande non trouvée." });

            commande.Statut = "Livrée";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Commande marquée comme livrée." });
        }
        [HttpGet("livreur/{livreurId}")]
        public async Task<IActionResult> GetCommandesLivreur(int livreurId)
        {
            var commandes = await _context.Commandes
                .Where(c => c.LivreurId == livreurId)
                .Include(c => c.Produits)
                .ToListAsync();

            return Ok(commandes);
        }
        [HttpPut("admin/assigner-livreur/{commandeId}")]
        public async Task<IActionResult> AssignerLivreurParAdmin(int commandeId, [FromQuery] int livreurId)
        {
            var commande = await _context.Commandes.FindAsync(commandeId);
            if (commande == null)
                return NotFound(new { message = "Commande introuvable." });

            var livreur = await _context.Utilisateurs
                .OfType<Livreur>()
                .FirstOrDefaultAsync(l => l.Id == livreurId);
            if (livreur == null)
                return NotFound(new { message = "Livreur introuvable." });

            commande.LivreurId = livreurId;
            commande.Statut = "En cours de livraison";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Commande assignée au livreur avec succès." });
        }

    }
}

