using Microsoft.EntityFrameworkCore;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(
            int userId,
            string action,
            string entityType,
            int? entityId,
            string description)
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    IpAddress = ipAddress ?? "Unknown"
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw - audit logging should not break the app
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(int userId, int limit = 100)
        {
            return await _context.AuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, int entityId)
        {
            return await _context.AuditLogs
                .Where(log => log.EntityType == entityType && log.EntityId == entityId)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentLogsAsync(int limit = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(log => log.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
