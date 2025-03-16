using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.Models;
using Application_Livraison_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Application_Livraison_Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // Connexion
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _authService.AuthenticateUser(request.Email, request.Mdp);

            if (user == null)
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });

            var token = _authService.GenerateJwtToken(user);

            // Retourne un objet avec le nom, le token et le rôle
            return Ok(new
            {
                nom = user.Nom,
                token = token,
                role = user.Role
            });
        }

        // Inscription - Réservée aux Clients
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignupRequest request)
        {
            // Vérifier si l'utilisateur existe déjà
            var existingUser = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return BadRequest(new { message = "L'utilisateur existe déjà avec cet email" });

            // Créer un nouveau Client
            var client = new Client
            {
                Nom = request.Nom,
                Email = request.Email,
                Tel = request.Tel,
                Mdp = _authService.HashPassword(request.Mdp),
                Role = "Client"  // Définir le rôle sur Client
            };

            _context.Utilisateurs.Add(client);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(client);
            return Ok(new { token });
        }

        // L'Admin crée un compte Livreur
        [HttpPost("admin/register-livreur")]
        public async Task<IActionResult> RegisterLivreur([FromBody] SignupRequest request)
        {
            // Seul l'Admin doit avoir accès à cette route
            var admin = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Role == "Admin");

            if (admin == null)
                return Unauthorized(new { message = "Accès non autorisé" });

            // Créer un nouveau Livreur
            var liv = new Livreur
            {
                Nom = request.Nom,
                Email = request.Email,
                Tel = request.Tel,
                Mdp = _authService.HashPassword(request.Mdp),
                Role = "Livreur"  // Définir le rôle sur Livreur
            };

            _context.Utilisateurs.Add(liv);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(liv);
            return Ok(new { token });
        }
    }
}
