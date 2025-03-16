using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Application_Livraison_Backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Méthode pour le hachage du mot de passe
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Méthode pour vérifier le mot de passe
        public bool VerifyPassword(string storedHash, string inputPassword)
        {
            var inputHash = HashPassword(inputPassword);
            return storedHash == inputHash;
        }

        // Générer le JWT pour un utilisateur
        public string GenerateJwtToken(Utilisateur user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Nom),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)  // Utilisez le rôle du modèle utilisateur
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Authentifier l'utilisateur en fonction de l'email et du mot de passe
        public Utilisateur? AuthenticateUser(string email, string mdp)
        {
            var user = _context.Utilisateurs
                .FirstOrDefault(u => u.Email == email);

            if (user != null && VerifyPassword(user.Mdp, mdp))
            {
                return user;
            }

            return null;  // Authentification échouée
        }
    }
}
