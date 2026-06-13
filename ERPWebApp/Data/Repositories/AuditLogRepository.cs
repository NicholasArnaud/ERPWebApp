using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
