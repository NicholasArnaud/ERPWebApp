using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IScheduledEmailRepository : IRepository<EmailAlert>
    {
        Task<List<EmailAlert>> GetAllEmailAlertsAsync();
        Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId);
    }


}
