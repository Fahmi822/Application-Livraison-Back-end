using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.DTOs;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application_Livraison_Backend.Controllers
{
    [ApiController]
    [Route("api/livreur")]
    [Authorize(Roles = "Livreur")]
    public class LivreurController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LivreurController> _logger;

        public LivreurController(AppDbContext context, ILogger<LivreurController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentLivreurId()
        {
            var livreurIdClaim = User.FindFirstValue("ClientId");
            if (string.IsNullOrEmpty(livreurIdClaim))
            {
                _logger.LogError("Claim ClientId non trouvé dans le token");
                throw new UnauthorizedAccessException("Authentification invalide");
            }

            if (!int.TryParse(livreurIdClaim, out int livreurId))
            {
                _logger.LogError($"Format d'ID invalide: {livreurIdClaim}");
                throw new UnauthorizedAccessException("Identifiant incorrect");
            }

            return livreurId;
        }

        [HttpGet("commandes-assignees")]
        public async Task<IActionResult> GetCommandesAssignees()
        {
            try
            {
                var livreurId = GetCurrentLivreurId();

                var commandes = await _context.Commandes
                    .Where(c => c.LivreurId == livreurId && c.Statut == "En cours de livraison")
                    .Include(c => c.Client)
                    .Include(c => c.Produits)
                    .Select(c => new CommandeDto
                    {
                        Id = c.Id,
                        ClientNomComplet = c.Client.Nom,
                        DateCommande = c.DateCommande,
                        Montant = c.Montant,
                        Statut = c.Statut,
                        Produits = c.Produits.Select(p => new ProduitDto
                        {
                            Id = p.Id,
                            Nom = p.Nom,
                            Prix = p.Prix
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(commandes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Erreur d'authentification");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commandes");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        // 2. Voir les commandes disponibles pour prise en charge
        [HttpGet("commandes-disponibles")]
        public async Task<IActionResult> GetCommandesDisponibles()
        {
            var commandes = await _context.Commandes
                .Where(c => c.Statut == "Prête" && c.LivreurId == null)
                .Select(c => new CommandeAssignableDto
                {
                    Id = c.Id,
                    Statut = c.Statut,
                    DateCommande = c.DateCommande,
                    ClientNom = $"{c.Client.Nom}",
                    AdresseLivraison = c.Client.Adresse,
                    Montant = c.Montant
                })
                .ToListAsync();

            return Ok(commandes);
        }

        // 3. Confirmer la prise en charge d'une commande assignée
        [HttpPost("confirmer-commande/{commandeId}")]
        public async Task<IActionResult> ConfirmerCommande(int commandeId)
        {
            var livreurId = GetCurrentLivreurId();

            var commande = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.Livreur)
                .FirstOrDefaultAsync(c => c.Id == commandeId && c.LivreurId == livreurId);

            if (commande == null)
                return NotFound(new { message = "Commande non trouvée ou non assignée à ce livreur." });

            var livraison = new Livraison
            {
                Statut = "En cours",
                DateLivraison = DateTime.Now.AddHours(2),
                RecRecu = false,
                CommandeId = commandeId,
                LivreurId = livreurId
            };

            _context.Livraisons.Add(livraison);
            commande.Statut = "En livraison";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Livraison créée avec succès",
                livraisonId = livraison.Id,
                commande = new
                {
                    Id = commande.Id,
                    ClientId = commande.ClientId,
                    ClientNomComplet = $"{commande.Client.Nom}",
                    DateCommande = commande.DateCommande,
                    Montant = commande.Montant,
                    Statut = commande.Statut,
                    Produits = commande.Produits.Select(p => new {
                        p.Id,
                        p.Nom,
                        p.Prix
                    })
                }
            });
        }

        // 4. Marquer une livraison comme terminée
        [HttpPut("livrer/{livraisonId}")]
        public async Task<IActionResult> MarquerCommeLivree(int livraisonId)
        {
            var livreurId = GetCurrentLivreurId();

            var livraison = await _context.Livraisons
                .Include(l => l.Commande)
                .FirstOrDefaultAsync(l => l.Id == livraisonId && l.LivreurId == livreurId);

            if (livraison == null)
                return NotFound(new { message = "Livraison non trouvée." });

            livraison.Statut = "Livrée";
            livraison.RecRecu = true;
            livraison.DateLivraison = DateTime.Now;

            if (livraison.Commande != null)
            {
                livraison.Commande.Statut = "Livrée";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Livraison marquée comme terminée",
                commandeId = livraison.CommandeId,
                statut = livraison.Statut
            });
        }
    }
}