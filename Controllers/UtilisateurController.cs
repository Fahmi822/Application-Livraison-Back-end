using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.Models;
using Application_Livraison_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application_Livraison_Backend.Controllers
{
    [Route("api/utilisateur")]
    [ApiController]
    public class UtilisateurController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public UtilisateurController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // ✅ Obtenir les infos de l'utilisateur connecté
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UtilisateurDto>> GetMonCompte()
        {
            var email = User.FindFirstValue(ClaimTypes.Email); // ✅ extrait l'email du token JWT

            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

            if (utilisateur == null) return NotFound();

            // ✅ Projection vers un DTO pour ne pas exposer le mot de passe
            return Ok(new UtilisateurDto
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                Tel = utilisateur.Tel,
                Adresse = utilisateur.Adresse,
                Vehicule = utilisateur.Vehicule,
                ImageUrl = utilisateur.ImageUrl,
                Role = utilisateur.Role
            });
        }


        // ✅ Modifier les infos de l'utilisateur connecté
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> ModifierMonCompte([FromBody] UtilisateurUpdateDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email); // ✅ récupéré du token

            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);
            if (utilisateur == null) return NotFound();

            // Mise à jour des champs
            utilisateur.Nom = dto.Nom;
            utilisateur.Email = dto.Email;
            utilisateur.Tel = dto.Tel;
            utilisateur.Adresse = dto.Adresse;
            utilisateur.Vehicule = dto.Vehicule;
            utilisateur.ImageUrl = dto.ImageUrl;

            // Si mot de passe fourni => on le hashe
            if (!string.IsNullOrWhiteSpace(dto.Mdp))
            {
                utilisateur.Mdp = _authService.HashPassword(dto.Mdp);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Informations mises à jour avec succès" });
        }

    }
}

