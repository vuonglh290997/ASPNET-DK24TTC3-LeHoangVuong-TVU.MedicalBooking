using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(int userId, string action, string entityType, int? entityId, string description);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(int userId, int limit = 100);
        Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, int entityId);
        Task<List<AuditLog>> GetRecentLogsAsync(int limit = 100);
    }
}
