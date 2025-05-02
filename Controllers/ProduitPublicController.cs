using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application_Livraison_Backend.Controllers
{
    [ApiController]
    [Route("api/public/produits")]
    public class ProduitPublicController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProduitPublicController(AppDbContext context)
        {
            _context = context;
        }

        // Obtenir tous les produits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produit>>> GetAllProduits()
        {
            var produits = await _context.Produits
                .Include(p => p.Categorie)
                .AsNoTracking()
                .ToListAsync();

            return Ok(produits);
        }

        // Obtenir toutes les catégories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Categorie>>> GetAllCategories()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }
        // GET: api/public/produits/by-categorie/{categorieId}
        [HttpGet("by-categorie/{categorieId}")]
        public async Task<ActionResult<IEnumerable<Produit>>> GetProduitsParCategorie(int categorieId)
        {
            var produits = await _context.Produits
                .Where(p => p.CategorieId == categorieId)
                .Include(p => p.Categorie)
                .AsNoTracking()
                .ToListAsync();

            if (produits == null || produits.Count == 0)
            {
                return NotFound($"Aucun produit trouvé pour la catégorie ID {categorieId}");
            }

            return Ok(produits);
        }

    }
}
