using System.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories
{
    public class EmailAlertsRepository : Repository <EmailAlert>, IEmailAlertsRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailAlertsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<EmailAlert> GetAllEmailAlerts()
        {
            return _context.EmailAlerts.ToList();
        }

        public async Task<EmailAlert> GetEmailAlertByIdAsync(int id)
        {
            return await _context.EmailAlerts.FirstOrDefaultAsync(e => e.EmailAlertId == id);
        }

        public async Task AddEmailAlert(EmailAlert emailAlert)
        {
            _context.EmailAlerts.Add(emailAlert);
        }
        public async Task AddEmailAlertMapping(UserEmailAlertMapping emailAlertMapping)
        {
            _context.UserEmailAlertMapping.Add(emailAlertMapping);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetAllEmailsAsync()
        {
            var userList = await _context.Users.Select(user => user.Email).ToListAsync();
            return userList;
        }
        
        public IdentityUser GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
        public async Task<IdentityUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> ExecuteTransactionAsync(Func<Task> action)
        {
            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
            try
            {
                await action();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ScheduledEmailViewModel>> GetAllEmailAlertsWithRecipients()
        {
            var emailAlerts = await _context.EmailAlerts.ToListAsync();
            var scheduledEmails = new List<ScheduledEmailViewModel>();

            foreach (var emailAlert in emailAlerts)
            {
                var recipientMappings = await _context.UserEmailAlertMapping
                    .Where(mapping => mapping.EmailAlertId == emailAlert.EmailAlertId)
                    .ToListAsync();

                var recipients = recipientMappings.Select(mapping => mapping.UserEmail).ToList();

                scheduledEmails.Add(new ScheduledEmailViewModel
                {
                    EmailAlertId = emailAlert.EmailAlertId,
                    Subject = emailAlert.Subject,
                    Body = emailAlert.Body,
                    RecipientEmail = string.Join(", ", recipients),
                    ScheduledTime = emailAlert.ScheduledTime,
                    ScheduledTimeOnly = TimeOnly.FromDateTime(emailAlert.ScheduledTime),
                    IsActive = emailAlert.IsActive,
                    AlertType = emailAlert.AlertType,
                    Frequency = emailAlert.Frequency,
                    AlertTemplateId = emailAlert.AlertTemplateId
                });
            }

            return scheduledEmails;
        }

        public async Task UpdateEmailAlert(EmailAlert emailAlert)
        {
            _context.EmailAlerts.Update(emailAlert);
        }

        public async Task<List<UserEmailAlertMapping>> GetMappingsForEmailAlert(int emailAlertId)
        {
            var mappings = await _context.UserEmailAlertMapping.Where(mapping => mapping.EmailAlertId == emailAlertId).ToListAsync();
            return mappings;
        }

        public async Task DeleteUserEmailAlertMappingAsync(int emailAlertId, string userEmail)
        {
            var mapping = await _context.UserEmailAlertMapping.FirstOrDefaultAsync(m => m.EmailAlertId == emailAlertId && m.UserEmail == userEmail);
            if (mapping != null)
            {
                _context.UserEmailAlertMapping.RemoveRange(mapping);
                await _context.SaveChangesAsync(); // Add this line to save changes to the database  
            }
        }
        public async Task DeleteEmailAlertAsync(int emailAlertId)
        {
            var emailAlert = await _context.EmailAlerts.FirstOrDefaultAsync(e => e.EmailAlertId == emailAlertId);
            if (emailAlert != null)
            {
                _context.EmailAlerts.Remove(emailAlert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<EmailAlert>> GetSubscribedEmailAlertsAsync(string userEmail)
        {
            var userMappings = await _context.UserEmailAlertMapping
                .Where(mapping => mapping.UserEmail == userEmail)
                .ToListAsync();

            var subscribedAlertIds = userMappings.Select(mapping => mapping.EmailAlertId);
            var subscribedAlerts = await _context.EmailAlerts
                .Where(alert => subscribedAlertIds.Contains(alert.EmailAlertId) && alert.IsActive)
                .ToListAsync();

            return subscribedAlerts;
        }

        public async Task<List<EmailAlert>> GetUnsubscribedEmailAlertsAsync(string userEmail)
        {
            var userMappings = await _context.UserEmailAlertMapping
                .Where(mapping => mapping.UserEmail == userEmail)
                .ToListAsync();

            var subscribedAlertIds = userMappings.Select(mapping => mapping.EmailAlertId);
            var unsubscribedAlerts = await _context.EmailAlerts
                .Where(alert => !subscribedAlertIds.Contains(alert.EmailAlertId) && alert.IsActive)
                .ToListAsync();

            return unsubscribedAlerts;
        }
        public async Task<EmailAlert> ChangeEmailAlertStatus(int emailAlertId, bool newStatus)
        {
            var emailAlert = await _context.EmailAlerts.FindAsync(emailAlertId);
            if (emailAlert != null)
            {
                emailAlert.IsActive = newStatus;
                await _context.SaveChangesAsync();
            }
            return emailAlert;
        }
        public async Task<bool> GetEmailAlertStatus(int emailAlertId)
        {
            var emailAlert = await _context.EmailAlerts.FindAsync(emailAlertId);
            return emailAlert?.IsActive ?? false;
        }

        public List<AlertTriggerTemplateMappings> GetAlertTriggerTemplateMappings()
        {
            return _context.AlertTriggerTemplateMappings.ToList();
        }

        public async Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId)
        {
            return await _context.UserEmailAlertMapping
                .Where(mapping => mapping.EmailAlertId == emailAlertId)
                .Select(mapping => mapping.UserEmail)
                .ToListAsync();
        }
        public async Task<List<EmailAlert>> GetAllEmailAlertsAsync()
        {
            return await _context.EmailAlerts.ToListAsync();
        }
        public async Task<int?> GetAlertTemplateIdByEmailAlertId(int emailAlertId)
        {
            var emailAlert = await _context.EmailAlerts.FindAsync(emailAlertId);
            return emailAlert?.AlertTemplateId;
        }
    }

    public class AspNetUsers
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}