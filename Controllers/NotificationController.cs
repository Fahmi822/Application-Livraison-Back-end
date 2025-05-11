using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application_Livraison_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(AppDbContext context, ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int? GetAuthenticatedUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                _logger.LogWarning("Claim NameIdentifier manquant ou invalide");
                return null;
            }
            return userId;
        }

        private string GetAuthenticatedUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        [Authorize]
        [HttpGet("mes-notifications")]
        public async Task<IActionResult> GetMesNotifications()
        {
            var userId = User.FindFirst("ClientId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                return Unauthorized(new { message = "User role not found in token" });
            }

            var notifications = await _context.Notifications
                .Where(n => n.DestinataireId.ToString() == userId && n.DestinataireType == userRole)
                .OrderByDescending(n => n.Date)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("marquer-vue/{id}")]
        public async Task<IActionResult> MarquerCommeVue(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }

                var userId = GetAuthenticatedUserId();
                if (notification.DestinataireId != userId)
                {
                    return Forbid();
                }

                notification.EstVue = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Notification marquée comme lue" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du marquage de la notification {id} comme lue");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        [HttpPut("marquer-toutes-vues")]
        public async Task<IActionResult> MarquerToutesCommeVues([FromBody] List<int> notificationIds)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Utilisateur non authentifié" });
                }

                var notifications = await _context.Notifications
                    .Where(n => notificationIds.Contains(n.Id) && n.DestinataireId == userId)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.EstVue = true;
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = $"{notifications.Count} notifications marquées comme lues",
                    count = notifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage des notifications comme lues");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDto notificationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = new Notification
                {
                    Message = notificationDto.Message,
                    Type = notificationDto.Type,
                    CommandeId = notificationDto.CommandeId,
                    ExpediteurId = notificationDto.ExpediteurId ?? GetAuthenticatedUserId(),
                    ExpediteurType = notificationDto.ExpediteurType ?? "Admin",
                    DestinataireId = notificationDto.DestinataireId,
                    DestinataireType = notificationDto.DestinataireType,
                    Date = DateTime.UtcNow,
                    EstVue = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetNotification),
                    new { id = notification.Id },
                    new
                    {
                        notification.Id,
                        notification.Message,
                        notification.Date,
                        notification.Type
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de notification");
                return StatusCode(500, new { message = "Erreur lors de la création de notification" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.Commande)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notification == null)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }

                var userId = GetAuthenticatedUserId();
                var userRole = GetAuthenticatedUserRole();

                if (notification.DestinataireId != userId || notification.DestinataireType != userRole)
                {
                    return Forbid();
                }

                return Ok(new
                {
                    notification.Id,
                    notification.Message,
                    notification.Type,
                    notification.Date,
                    notification.EstVue,
                    CommandeId = notification.CommandeId,
                    CommandeStatut = notification.Commande?.Statut
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de la notification {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }

                var userId = GetAuthenticatedUserId();
                if (notification.DestinataireId != userId)
                {
                    return Forbid();
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Notification supprimée avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression de la notification {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        public class NotificationDto
        {
            [Required]
            [StringLength(500)]
            public string Message { get; set; }

            [Required]
            [StringLength(50)]
            public string Type { get; set; }

            public int? CommandeId { get; set; }
            public int? ExpediteurId { get; set; }
            public string ExpediteurType { get; set; }

            [Required]
            public int? DestinataireId { get; set; }

            [Required]
            [StringLength(20)]
            public string DestinataireType { get; set; }
        }
    }
}