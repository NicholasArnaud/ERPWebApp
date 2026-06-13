using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class AuditLogService : Service<AuditLog>, IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuditLogService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<AuditLog>> GetAuditLogsAsync()
        {
            return _unitOfWork.AuditLogs.GetAllAsync();
        }
    }
}
