using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application_Livraison_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatistiquesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatistiquesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/statistiques/nombre-livreurs
        [HttpGet("nombre-livreurs")]
        public async Task<ActionResult<int>> GetNombreLivreurs()
        {
            return await _context.Utilisateurs.OfType<Livreur>().CountAsync();
        }

        // GET: api/statistiques/nombre-clients
        [HttpGet("nombre-clients")]
        public async Task<ActionResult<int>> GetNombreClients()
        {
            return await _context.Utilisateurs.OfType<Client>().CountAsync();
        }

        // GET: api/statistiques/nombre-commandes
        [HttpGet("nombre-commandes")]
        public async Task<ActionResult<int>> GetNombreCommandes()
        {
            return await _context.Commandes.CountAsync();
        }

        // GET: api/statistiques/revenu-total
        [HttpGet("revenu-total")]
        public async Task<ActionResult<decimal>> GetRevenuTotal()
        {
            return await _context.Commandes.SumAsync(c => c.Montant);
        }

        // GET: api/statistiques/repartition-categories
        [HttpGet("repartition-categories")]
        public async Task<ActionResult<IEnumerable<RepartitionCategorieDto>>> GetRepartitionCategories()
        {
            return await _context.Commandes
                .Include(c => c.Produits)
                .SelectMany(c => c.Produits)
                .GroupBy(p => new { p.Categorie.Id, p.Categorie.Nom })
                .Select(g => new RepartitionCategorieDto
                {
                    CategorieId = g.Key.Id,
                    CategorieNom = g.Key.Nom,
                    NombreProduits = g.Count(),
                    Pourcentage = Math.Round((decimal)g.Count() / _context.Produits.Count() * 100, 2)
                })
                .ToListAsync();
        }

        // GET: api/statistiques/commandes-par-jour
        [HttpGet("commandes-par-jour")]
        public async Task<ActionResult<IEnumerable<CommandesParJourDto>>> GetCommandesParJour([FromQuery] int jours = 7)
        {
            var dateLimite = DateTime.Now.AddDays(-jours);

            return await _context.Commandes
                .Where(c => c.DateCommande >= dateLimite)
                .GroupBy(c => c.DateCommande.Date)
                .Select(g => new CommandesParJourDto
                {
                    Date = g.Key,
                    NombreCommandes = g.Count(),
                    RevenuTotal = g.Sum(c => c.Montant)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        // GET: api/statistiques/livreurs-actifs
        [HttpGet("livreurs-actifs")]
        public async Task<ActionResult<IEnumerable<UtilisateurDto>>> GetLivreursActifs()
        {
            return await _context.Utilisateurs
                .OfType<Livreur>()
                .Where(l => _context.Livraisons.Any(liv => liv.LivreurId == l.Id))
                .Select(l => new UtilisateurDto
                {
                    Id = l.Id,
                    Nom = l.Nom,
                    Email = l.Email,
                    Tel = l.Tel,
                    Adresse = l.Adresse,
                    Vehicule = l.Vehicule,
                    ImageUrl = l.ImageUrl,
                    Role = "Livreur"
                })
                .Take(5) // Top 5 livreurs actifs
                .ToListAsync();
        }
    }

    // DTOs spécifiques aux statistiques
    public class RepartitionCategorieDto
    {
        public int CategorieId { get; set; }
        public string CategorieNom { get; set; }
        public int NombreProduits { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class CommandesParJourDto
    {
        public DateTime Date { get; set; }
        public int NombreCommandes { get; set; }
        public decimal RevenuTotal { get; set; }
    }
}