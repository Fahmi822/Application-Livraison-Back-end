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
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CategorieController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategorieController> _logger;

        public CategorieController(AppDbContext context, ILogger<CategorieController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Crée une nouvelle catégorie
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Categorie>> CreateCategorie([FromBody] CategorieDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var normalizedName = dto.Nom.Trim().ToLower();
                if (await _context.Categories.AnyAsync(c => c.Nom.Trim().ToLower() == normalizedName))
                    return Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = "Une catégorie avec ce nom existe déjà"
                    });

                var categorie = new Categorie
                {
                    Nom = dto.Nom.Trim(),
                    Description = dto.Description?.Trim()
                };

                await _context.Categories.AddAsync(categorie);
                await _context.SaveChangesAsync();

                LogAction($"a créé la catégorie {categorie.Id}");
                return CreatedAtAction(nameof(GetCategorieById), new { id = categorie.Id }, categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création catégorie");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la création de la catégorie"
                });
            }
        }

        /// <summary>
        /// Récupère toutes les catégories
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Categorie>>> GetAllCategories()
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur récupération catégories");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la récupération des catégories"
                });
            }
        }

        /// <summary>
        /// Récupère une catégorie par son ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Categorie>> GetCategorieById(int id)
        {
            try
            {
                var categorie = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                return categorie == null
                    ? NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Catégorie non trouvée"
                    })
                    : Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur récupération catégorie {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la récupération de la catégorie"
                });
            }
        }

        /// <summary>
        /// Met à jour une catégorie existante
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Categorie>> UpdateCategorie(int id, [FromBody] CategorieDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categorie = await _context.Categories.FindAsync(id);
                if (categorie == null)
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Catégorie non trouvée"
                    });

                var normalizedName = dto.Nom.Trim().ToLower();
                if (await _context.Categories.AnyAsync(c => c.Id != id && c.Nom.Trim().ToLower() == normalizedName))
                    return Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = "Une autre catégorie avec ce nom existe déjà"
                    });

                categorie.Nom = dto.Nom.Trim();
                categorie.Description = dto.Description?.Trim();

                await _context.SaveChangesAsync();

                LogAction($"a modifié la catégorie {categorie.Id}");
                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur modification catégorie {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la modification de la catégorie"
                });
            }
        }

        /// <summary>
        /// Supprime une catégorie existante
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategorie(int id)
        {
            try
            {
                var categorie = await _context.Categories
                    .Include(c => c.Produits)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (categorie == null)
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "Catégorie non trouvée"
                    });

                if (categorie.Produits?.Any() == true)
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = "Impossible de supprimer - catégorie associée à des produits"
                    });

                _context.Categories.Remove(categorie);
                await _context.SaveChangesAsync();

                LogAction($"a supprimé la catégorie {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur suppression catégorie {id}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "Une erreur est survenue lors de la suppression de la catégorie"
                });
            }
        }

        private void LogAction(string action)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin {AdminId} {Action}", adminId, action);
        }
    }
}