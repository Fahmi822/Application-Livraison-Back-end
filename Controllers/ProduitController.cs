using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.DTOs;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application_Livraison_Backend.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProduitController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProduitController> _logger;
        private readonly IWebHostEnvironment _environment;
        private const string ImageUploadFolder = "images/produits";

        public ProduitController(AppDbContext context, ILogger<ProduitController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Crée un nouveau produit
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Produit>> CreateProduit([FromForm] ProduitDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ValidationProblemDetails(ModelState));

                if (dto.ImgUp == null || dto.ImgUp.Length == 0)
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = "Une image est requise"
                    });

                // Vérification de la catégorie
                var categorieExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategorieId);
                if (!categorieExists)
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = "Catégorie invalide"
                    });

                // Vérification du nom du produit
                var normalizedName = dto.Nom.Trim().ToLower();
                if (await _context.Produits.AnyAsync(p => p.Nom.Trim().ToLower() == normalizedName))
                    return Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = "Un produit avec ce nom existe déjà"
                    });

                // Gestion de l'image
                var imagePath = await SaveImage(dto.ImgUp);

                var produit = new Produit
                {
                    Nom = dto.Nom.Trim(),
                    Prix = dto.Prix,
                    Description = dto.Description?.Trim(),
                    Quantite = dto.Quantite,
                    Img = imagePath,
                    CategorieId = dto.CategorieId
                };

                _context.Produits.Add(produit);
                await _context.SaveChangesAsync();

                LogAction($"a créé le produit {produit.Id}");
                return CreatedAtAction(nameof(GetProduitById), new { id = produit.Id }, produit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création produit");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la création du produit"
                });
            }
        }

        /// <summary>
        /// Récupère tous les produits
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Produit>>> GetAllProduits()
        {
            try
            {
                return await _context.Produits
                    .AsNoTracking()
                    .Include(p => p.Categorie)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur récupération produits");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la récupération des produits"
                });
            }
        }

        /// <summary>
        /// Récupère un produit par son ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Produit>> GetProduitById(int id)
        {
            try
            {
                var produit = await _context.Produits
                    .AsNoTracking()
                    .Include(p => p.Categorie)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return produit == null
                    ? NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Produit non trouvé"
                    })
                    : Ok(produit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur récupération produit {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la récupération du produit"
                });
            }
        }

        /// <summary>
        /// Met à jour un produit existant
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Produit>> UpdateProduit(int id, [FromForm] ProduitDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ValidationProblemDetails(ModelState));

                var produit = await _context.Produits.FindAsync(id);
                if (produit == null)
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Produit non trouvé"
                    });

                // Vérification de la catégorie
                var categorieExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategorieId);
                if (!categorieExists)
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = "Catégorie invalide"
                    });

                // Vérification du nom du produit
                var normalizedName = dto.Nom.Trim().ToLower();
                if (await _context.Produits.AnyAsync(p => p.Id != id && p.Nom.Trim().ToLower() == normalizedName))
                    return Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = "Un autre produit avec ce nom existe déjà"
                    });

                // Mise à jour des propriétés
                produit.Nom = dto.Nom.Trim();
                produit.Prix = dto.Prix;
                produit.Description = dto.Description?.Trim();
                produit.Quantite = dto.Quantite;
                produit.CategorieId = dto.CategorieId;

                // Gestion de l'image si fournie
                if (dto.ImgUp != null && dto.ImgUp.Length > 0)
                {
                    // Supprimer l'ancienne image si elle existe
                    if (!string.IsNullOrEmpty(produit.Img))
                    {
                        DeleteImage(produit.Img);
                    }

                    produit.Img = await SaveImage(dto.ImgUp);
                }

                await _context.SaveChangesAsync();
                await _context.Entry(produit).Reference(p => p.Categorie).LoadAsync();

                LogAction($"a modifié le produit {produit.Id}");
                return Ok(produit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur modification produit {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la modification du produit"
                });
            }
        }

        /// <summary>
        /// Supprime un produit
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduit(int id)
        {
            try
            {
                var produit = await _context.Produits
                    .Include(p => p.Commandes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produit == null)
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Produit non trouvé"
                    });

                if (produit.Commandes?.Any() == true)
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = "Impossible de supprimer - produit associé à des commandes"
                    });

                // Supprimer l'image associée
                if (!string.IsNullOrEmpty(produit.Img))
                {
                    DeleteImage(produit.Img);
                }

                _context.Produits.Remove(produit);
                await _context.SaveChangesAsync();

                LogAction($"a supprimé le produit {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur suppression produit {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la suppression du produit"
                });
            }
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, ImageUploadFolder);
            Directory.CreateDirectory(uploadsFolder);  // Crée le dossier s'il n'existe pas

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";  // Nom unique pour éviter les conflits
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);  // Chemin complet de l'image

            // Sauvegarde de l'image dans le dossier spécifié
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;  // Retourne le nom de fichier pour le stockage en base de données
        }

        private void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_environment.WebRootPath, ImageUploadFolder, imageName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        private void LogAction(string action)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin {AdminId} {Action}", adminId, action);
        }
        // ✅ Obtenir tous les produits avec leur catégorie
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produit>>> GetAllProduitsPublic()
        {
            return await _context.Produits
                .AsNoTracking()
                .Include(p => p.Categorie)
                .ToListAsync();
        }

        // ✅ Obtenir les produits par catégorie
        [HttpGet("public/categorie/{categorieId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produit>>> GetProduitsByCategoriePublic(int categorieId)
        {
            return await _context.Produits
                .Where(p => p.CategorieId == categorieId)
                .Include(p => p.Categorie)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Obtenir un produit par ID
        [HttpGet("public/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Produit>> GetProduitByIdPublic(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
                return NotFound(new { message = "Produit non trouvé" });

            return Ok(produit);
        }

    }

}