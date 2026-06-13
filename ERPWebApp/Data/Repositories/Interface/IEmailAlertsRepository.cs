using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Identity;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IEmailAlertsRepository : IRepository<EmailAlert>
    {
        List<EmailAlert> GetAllEmailAlerts();
        Task<EmailAlert> GetEmailAlertByIdAsync(int id);
        Task AddEmailAlert(EmailAlert emailAlert);
        Task AddEmailAlertMapping(UserEmailAlertMapping emailAlertMapping);
        Task UpdateEmailAlert(EmailAlert emailAlert);
        Task DeleteEmailAlertAsync(int emailAlertId);
        Task SaveChangesAsync();
        Task<List<string>> GetAllEmailsAsync();
        IdentityUser GetUserByEmail(string email);
        Task<IdentityUser> GetUserByIdAsync(string userId);
        Task<bool> ExecuteTransactionAsync(Func<Task> action);
        Task<List<ScheduledEmailViewModel>> GetAllEmailAlertsWithRecipients();
        Task<List<UserEmailAlertMapping>> GetMappingsForEmailAlert(int emailAlertId);
        Task DeleteUserEmailAlertMappingAsync(int emailAlertId, string userEmail);
        Task<List<EmailAlert>> GetSubscribedEmailAlertsAsync(string userEmail);
        Task<List<EmailAlert>> GetUnsubscribedEmailAlertsAsync(string userEmail);
        Task<EmailAlert> ChangeEmailAlertStatus(int emailAlertId, bool newStatus);
        Task<bool> GetEmailAlertStatus(int emailAlertId);
        List<AlertTriggerTemplateMappings> GetAlertTriggerTemplateMappings();
        Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId);
        Task<List<EmailAlert>> GetAllEmailAlertsAsync();
        Task<int?> GetAlertTemplateIdByEmailAlertId(int emailAlertId);

    }
}