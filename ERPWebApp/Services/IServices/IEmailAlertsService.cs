using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Identity;

namespace ERPWebApp.Services.IServices
{
    public interface IEmailAlertsService
    {
        Task<List<string>> GetAllUserEmailsAsync();
        Task AddEmailAlert(EmailAlert emailAlert);
        Task AddEmailAlertMapping(UserEmailAlertMapping emailAlertMapping);
        Task<EmailAlert> CreateEmailAlertAsync(EmailAlert emailAlert);
        Task CreateUserEmailAlertMappingAsync(int emailAlertId, string userEmail);
        IdentityUser GetUserByEmail(string email);
        Task<IdentityUser> GetUserByIdAsync(string userId);
        Task<(bool success, string message)> CreateEmailAlertWithMappingsAsync(EmailAlert emailAlert, List<string> recipients);
        Task<List<ScheduledEmailViewModel>> GetAllEmailAlertsWithRecipients();
        Task<EmailAlert> GetEmailAlertByIdAsync(int emailAlertId);
        Task UpdateEmailAlertAsync(EmailAlert updatedEmailAlert, List<string> updatedRecipients);
        Task DeleteUserEmailAlertMappingAsync(int emailAlertId, string userEmail);
        Task DeleteEmailAlertAsync(int emailAlertId);
        Task<List<string>> GetRecipientsForEmailAlert(int emailAlertId);
        Task<List<EmailAlert>> GetSubscribedEmailAlertsAsync(string userEmail);
        Task<List<EmailAlert>> GetUnsubscribedEmailAlertsAsync(string userEmail);
        Task<EmailAlert> ChangeEmailAlertStatus(int emailAlertId, bool newStatus);
        Task<bool> GetEmailAlertStatus(int emailAlertId);
        List<AlertTriggerTemplateMappings> GetAlertTriggerTemplateMappings();
        Task<int?> GetAlertTemplateIdByEmailAlertId(int emailAlertId);
        Task UpdateEmailAlertScheduledTimeAsync(int emailAlertId, DateTime newScheduledTime);


    }
}