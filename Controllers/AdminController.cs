using Application_Livraison_Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Application_Livraison_Backend.Controllers
{

    

    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // Seul un Admin peut accéder à ce contrôleur
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Lister tous les clients
        [HttpGet("list-clients")]
        public async Task<IActionResult> ListClients()
        {
            var clients = await _context.Utilisateurs
                .Where(u => u.Role == "Client") // Filtre pour ne récupérer que les clients
                .ToListAsync();

            return Ok(clients);
        }

        // Lister tous les livreurs
        [HttpGet("list-livreurs")]
        public async Task<IActionResult> ListLivreurs()
        {
            var livreurs = await _context.Utilisateurs
                .Where(u => u.Role == "Livreur") // Filtre pour ne récupérer que les livreurs
                .ToListAsync();

            return Ok(livreurs);
        }
    }
}
