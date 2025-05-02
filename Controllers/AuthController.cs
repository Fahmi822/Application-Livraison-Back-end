using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Dtos;
using Application_Livraison_Backend.Models;
using Application_Livraison_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Application_Livraison_Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public AuthController(AuthService authService,EmailService emailService, AppDbContext context)
        {
            _authService = authService;
            _emailService = emailService;
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

            // Retourne un objet avec le nom, le token, le rôle et le ClientId
            return Ok(new
            {
                nom = user.Nom,
                token = token,
                role = user.Role,
                clientId = user.Id  // Ajout du clientId dans la réponse
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
        [HttpPost("admin/register-livreur")]
        public async Task<IActionResult> RegisterLivreur([FromBody] SignupRequest request)
        {
            // Récupérer le token JWT depuis l'en-tête de la requête
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token manquant" });
            }

            // Valider le token et récupérer les claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Vérifier si l'utilisateur est un Admin
            if (roleClaim != "Admin")
            {
                return Unauthorized(new { message = "Accès non autorisé. Seul un Admin peut ajouter un livreur." });
            }

            // Vérifier si un utilisateur avec cet email existe déjà
            var existingUser = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "L'utilisateur existe déjà avec cet email" });
            }

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

            // Envoyer un e-mail au livreur avec ses informations de connexion
            var emailSubject = "Vos informations de connexion";
            var emailBody = $@"
        <h1>Bonjour {liv.Nom},</h1>
        <p>Votre compte livreur a été créé avec succès.</p>
        <p>Voici vos informations de connexion :</p>
        <ul>
            <li><strong>Email :</strong> {liv.Email}</li>
            <li><strong>Mot de passe :</strong> {request.Mdp}</li>
        </ul>
        <p>Connectez-vous dès maintenant pour commencer à travailler.</p>
    ";

            await _emailService.SendEmailAsync(liv.Email, emailSubject, emailBody);

            var tokenResponse = _authService.GenerateJwtToken(liv);
            return Ok(new { token = tokenResponse });
        }
    }
}