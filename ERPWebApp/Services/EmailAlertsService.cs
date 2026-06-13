using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Identity;

namespace ERPWebApp.Services
{
    public class EmailAlertsService : IEmailAlertsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailAlertsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<List<string>> GetAllUserEmailsAsync()
        {
            return await _unitOfWork.EmailAlerts.GetAllEmailsAsync();
        }
        public async Task AddEmailAlert(EmailAlert emailAlert)
        {
            await _unitOfWork.EmailAlerts.AddEmailAlert(emailAlert);
        }

        public async Task AddEmailAlertMapping(UserEmailAlertMapping emailAlertMapping)
        {
            await _unitOfWork.EmailAlerts.AddEmailAlertMapping(emailAlertMapping);
        }

        public async Task<EmailAlert> CreateEmailAlertAsync(EmailAlert emailAlert)
        {
            AddEmailAlert(emailAlert);
            await _unitOfWork.EmailAlerts.SaveChangesAsync();
            return emailAlert;
        }

        public async Task CreateUserEmailAlertMappingAsync(int emailAlertId, string userEmail)
        {
            try
            {
                var user = GetUserByEmail(userEmail);
                if (user != null)
                {
                    var mappingEntry = new UserEmailAlertMapping
                    {
                        EmailAlertId = emailAlertId,
                        UserId = user.Id,
                        UserEmail = userEmail
                    };
                    AddEmailAlertMapping(mappingEntry);
                    await _unitOfWork.EmailAlerts.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"Warning: User not found for email '{userEmail}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating UserEmailAlertMapping: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool success, string message)> CreateEmailAlertWithMappingsAsync(EmailAlert emailAlert, List<string> recipients)
        {
            try
            {
                await _unitOfWork.EmailAlerts.ExecuteTransactionAsync(async () =>
                {
                    var createdEmailAlert = await CreateEmailAlertAsync(emailAlert);

                    // Create UserEmailAlertMapping entries for each recipient  
                    if (recipients != null)
                    {
                        foreach (var recipientEmail in recipients)
                        {
                            await CreateUserEmailAlertMappingAsync(createdEmailAlert.EmailAlertId, recipientEmail);
                        }
                    }
                });

                return (true, "Email alert created successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error creating email alert: {ex.Message}");
            }
        }

        public IdentityUser GetUserByEmail(string email)
        {
            return _unitOfWork.EmailAlerts.GetUserByEmail(email);
        }
        public async Task<IdentityUser> GetUserByIdAsync(string userId)
        {
            return await _unitOfWork.EmailAlerts.GetUserByIdAsync(userId);
        }

        public async Task<List<ScheduledEmailViewModel>> GetAllEmailAlertsWithRecipients()
        {
            return await _unitOfWork.EmailAlerts.GetAllEmailAlertsWithRecipients();
        }
        public async Task<EmailAlert> GetEmailAlertByIdAsync(int emailAlertId)
        {
            return await _unitOfWork.EmailAlerts.GetEmailAlertByIdAsync(emailAlertId);
        }


        public async Task UpdateEmailAlertAsync(EmailAlert updatedEmailAlert, List<string> updatedRecipients)
        {
            await _unitOfWork.EmailAlerts.ExecuteTransactionAsync(async () =>
            {
                var originalEmailAlert = await _unitOfWork.EmailAlerts.GetEmailAlertByIdAsync(updatedEmailAlert.EmailAlertId);

                // Update the properties that should change  
                originalEmailAlert.Subject = updatedEmailAlert.Subject;
                originalEmailAlert.Body = updatedEmailAlert.Body;
                originalEmailAlert.ScheduledTime = updatedEmailAlert.ScheduledTime;
                originalEmailAlert.ModifyDate = DateTime.UtcNow;
                originalEmailAlert.AlertType = updatedEmailAlert.AlertType;
                originalEmailAlert.Frequency = updatedEmailAlert.Frequency;
                originalEmailAlert.AlertTemplateId = updatedEmailAlert.AlertTemplateId;

                // Update the email alert in the database        
                _unitOfWork.EmailAlerts.UpdateEmailAlert(originalEmailAlert);

                // Update the recipients        
                var existingMappings = await _unitOfWork.EmailAlerts.GetMappingsForEmailAlert(updatedEmailAlert.EmailAlertId);
                var newRecipients = updatedRecipients.Except(existingMappings.Select(mapping => mapping.UserEmail)).ToList();
                var removedRecipients = existingMappings.Select(mapping => mapping.UserEmail).Except(updatedRecipients).ToList();

                // Create new mappings for the new recipients        
                foreach (var recipient in newRecipients)
                {
                    await CreateUserEmailAlertMappingAsync(updatedEmailAlert.EmailAlertId, recipient);
                }

                // Remove the mappings for the removed recipients        
                foreach (var recipient in removedRecipients)
                {
                    await _unitOfWork.EmailAlerts.DeleteUserEmailAlertMappingAsync(updatedEmailAlert.EmailAlertId, recipient);
                }

                await _unitOfWork.SaveChangesAsync();
            });
        }
        public async Task DeleteUserEmailAlertMappingAsync(int emailAlertId, string userEmail)
        {
            await _unitOfWork.EmailAlerts.DeleteUserEmailAlertMappingAsync(emailAlertId, userEmail);
        }
        public async Task DeleteEmailAlertAsync(int emailAlertId)
        {
            await _unitOfWork.EmailAlerts.ExecuteTransactionAsync(async () =>
            {
                await _unitOfWork.EmailAlerts.DeleteEmailAlertAsync(emailAlertId);
            });
        }

        public async Task<List<string>> GetRecipientsForEmailAlert(int emailAlertId)
        {
            var mappings = await _unitOfWork.EmailAlerts.GetMappingsForEmailAlert(emailAlertId);
            return mappings.Select(mapping => mapping.UserEmail).ToList();
        }

        public async Task<List<EmailAlert>> GetSubscribedEmailAlertsAsync(string userEmail)
        {
            return await _unitOfWork.EmailAlerts.GetSubscribedEmailAlertsAsync(userEmail);
        }

        public async Task<List<EmailAlert>> GetUnsubscribedEmailAlertsAsync(string userEmail)
        {
            return await _unitOfWork.EmailAlerts.GetUnsubscribedEmailAlertsAsync(userEmail);
        }

        public async Task<EmailAlert> ChangeEmailAlertStatus(int emailAlertId, bool newStatus)
        {
            var emailAlert = await _unitOfWork.EmailAlerts.ChangeEmailAlertStatus(emailAlertId, newStatus);
            return emailAlert;
        }
        public async Task<bool> GetEmailAlertStatus(int emailAlertId)
        {
            return await _unitOfWork.EmailAlerts.GetEmailAlertStatus(emailAlertId);
        }

        public List<AlertTriggerTemplateMappings> GetAlertTriggerTemplateMappings()
        {
            return _unitOfWork.EmailAlerts.GetAlertTriggerTemplateMappings();
        }
        public async Task<int?> GetAlertTemplateIdByEmailAlertId(int emailAlertId)
        {
            return await _unitOfWork.EmailAlerts.GetAlertTemplateIdByEmailAlertId(emailAlertId);
        }

        private DateTime GetNextScheduledTime(DateTime scheduledTime, Frequency? frequency)
        {
            DateTime nextScheduledTime;

            switch (frequency)
            {
                case Frequency.Weekly:
                    nextScheduledTime = scheduledTime.AddDays(7);
                    break;
                case Frequency.Monthly:
                    nextScheduledTime = scheduledTime.AddMonths(1);
                    break;
                default: // Daily 
                    nextScheduledTime = scheduledTime.AddDays(1);
                    break;
            }

            return nextScheduledTime;
        }

        public async Task UpdateEmailAlertScheduledTimeAsync(int emailAlertId, DateTime newScheduledTime)
        {
            var emailAlert = await _unitOfWork.EmailAlerts.GetEmailAlertByIdAsync(emailAlertId);

            // Calculate the next scheduled time based on the frequency  
            var nextScheduledTime = GetNextScheduledTime(newScheduledTime, emailAlert.Frequency);

            // Convert nextScheduledTime from CST to UTC  
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var nextScheduledTimeUtc = TimeZoneInfo.ConvertTimeToUtc(nextScheduledTime, cstZone);

            emailAlert.ScheduledTime = nextScheduledTimeUtc;
            await _unitOfWork.EmailAlerts.UpdateEmailAlert(emailAlert);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}