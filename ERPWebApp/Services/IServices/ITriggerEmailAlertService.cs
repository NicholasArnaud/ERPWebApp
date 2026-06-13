using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface ITriggerEmailAlertService
    {
        Task<List<EmailAlert>> GetTriggeredEmailAlertsAsync();
        Task<List<EmailAlert>> GetUserTriggeredEmailAlertsAsync();
        Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId);
        void SendEmails(string subject, string body, List<string> recipients, byte[] attachment = null);
        Task NotifyOnStockUpdateAsync(Stock oldStock, Stock newStock);
        Task SendTriggerEmailAlertNow(EmailAlert emailAlert, List<string> recipients);
        Task SendFinishedCycleCountAlerts(CycleCountFinishedEmailAlertDTO cycleCount);
        Task SendUserCreateEmail(UserEmailAlertDTO userEmailData);

    }
}
