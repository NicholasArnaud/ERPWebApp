using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface IAuditLogService : IService<AuditLog>
    {
        Task<List<AuditLog>> GetAuditLogsAsync();
    }
}
