using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ScheduledEmailService : IScheduledEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailScheduler _emailScheduler;

        public ScheduledEmailService(IEmailScheduler emailScheduler, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _emailScheduler = emailScheduler;
        }

        public async Task<List<EmailAlert>> GetAllEmailAlertsAsync()
        {
            return await _unitOfWork.EmailAlerts.GetAllEmailAlertsAsync();
        }

        public async Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId)
        {
            return await _unitOfWork.EmailAlerts.GetRecipientsForEmailAlertAsync(emailAlertId);
        }
    }
}
