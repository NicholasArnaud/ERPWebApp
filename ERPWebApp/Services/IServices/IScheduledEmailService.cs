using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface IScheduledEmailService
    {
        Task<List<EmailAlert>> GetAllEmailAlertsAsync();
        Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId);
    }
}
