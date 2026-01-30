using MVC___MSSQL_Classifieds_Portal.Models;
using System.Text.Json;

namespace MVC___MSSQL_Classifieds_Portal.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityName, int? entityId, int? userId, object? changes = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ClassifieldsContext _context;

        public AuditLogService(ClassifieldsContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string action, string entityName, int? entityId, int? userId, object? changes = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                UserId = userId,
                Changes = changes != null ? JsonSerializer.Serialize(changes) : null,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
