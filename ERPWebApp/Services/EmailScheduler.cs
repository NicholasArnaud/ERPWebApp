using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

public class EmailSchedulerHostedService : IHostedService, IEmailScheduler
{
    private Timer _timer;
    private readonly IGraphAPIService _graphAPIService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public List<ScheduledEmailViewModel> ScheduledEmails { get; private set; }

    public EmailSchedulerHostedService(IGraphAPIService graphAPIService, IServiceScopeFactory serviceScopeFactory)
    {
        _graphAPIService = graphAPIService;
        _serviceScopeFactory = serviceScopeFactory;
        ScheduledEmails = new List<ScheduledEmailViewModel>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var scheduledEmailService = scope.ServiceProvider.GetRequiredService<IScheduledEmailService>();

            var emailAlerts = await scheduledEmailService.GetAllEmailAlertsAsync();

            foreach (var emailAlert in emailAlerts)
            {
                if (emailAlert.AlertType == 0 && emailAlert.IsActive == true)
                {
                    var recipients = await scheduledEmailService.GetRecipientsForEmailAlertAsync(emailAlert.EmailAlertId);

                    foreach (var recipient in recipients)
                    {
                        var scheduledEmail = new ScheduledEmailViewModel
                        {
                            EmailAlertId = emailAlert.EmailAlertId,
                            Subject = emailAlert.Subject,
                            Body = emailAlert.Body,
                            RecipientEmail = recipient,
                            ScheduledTime = emailAlert.ScheduledTime,
                            IsActive = emailAlert.IsActive,
                            AlertType = emailAlert.AlertType,
                            Frequency = emailAlert.Frequency
                        };

                        _ = ScheduleEmailAlertAsync(scheduledEmail, cancellationToken);
                    }
                }
            }
        }

        return;
    }

    private DateTime GetCurrentCstTime()
    {
        var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        var currentCstTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, cstZone);
        return currentCstTime;
    }

    private DateTime GetNextAlertTime(DateTime scheduledTime, Frequency? frequency)
    {
        var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        DateTimeOffset inputDateTimeOffset = new DateTimeOffset(scheduledTime, TimeSpan.Zero);
        DateTimeOffset inputDateTimeOffsetInCst = TimeZoneInfo.ConvertTime(inputDateTimeOffset, cstZone);

        DateTime scheduledCstTime;

        if (inputDateTimeOffset.Offset == inputDateTimeOffsetInCst.Offset)
        {
            scheduledCstTime = scheduledTime;
        }
        else
        {
            scheduledCstTime = TimeZoneInfo.ConvertTimeFromUtc(scheduledTime, cstZone);
        }

        var now = GetCurrentCstTime();

        while (now > scheduledCstTime)
        {
            switch (frequency)
            {
                case Frequency.Weekly:
                    scheduledCstTime = scheduledCstTime.AddDays(7);
                    break;
                case Frequency.Monthly:
                    scheduledCstTime = scheduledCstTime.AddMonths(1);
                    break;
                default: // Frequency.Daily  
                    scheduledCstTime = scheduledCstTime.AddDays(1);
                    break;
            }
        }

        return scheduledCstTime;
    }

    private async Task ScheduleEmailAlertAsync(ScheduledEmailViewModel scheduledEmail, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var scheduledTime = GetNextAlertTime(scheduledEmail.ScheduledTime, scheduledEmail.Frequency);
            scheduledEmail.ScheduledTime = scheduledTime;

            var delay = scheduledTime - GetCurrentCstTime();
            if (delay.Ticks < 0)
            {
                delay = TimeSpan.Zero;
            }

            await Task.Delay(delay, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                string userId = await _graphAPIService.GetUserIdByEmail(scheduledEmail.RecipientEmail);
                await _graphAPIService.SendEmailAlert(scheduledEmail.Subject, scheduledEmail.Body, scheduledEmail.RecipientEmail, userId);

                await UpdateScheduledTimeForEmailAlert(scheduledEmail.EmailAlertId, scheduledTime);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void AddScheduledEmail(ScheduledEmailViewModel email)
    {
        ScheduledEmails.Add(email);
    }

    public async Task<bool> UpdateScheduledEmailAlert(EmailAlert emailAlert, List<string> recipients)
    {
        bool success = false;
        try
        {
            // Cancel the previous scheduled email alerts  
            var oldScheduledEmails = ScheduledEmails.Where(s => s.EmailAlertId == emailAlert.EmailAlertId).ToList();
            foreach (var oldScheduledEmail in oldScheduledEmails)
            {
                oldScheduledEmail.CancellationTokenSource.Cancel();
            }

            // Remove the old scheduled email alerts associated with the emailAlert.EmailAlertId  
            ScheduledEmails.RemoveAll(s => s.EmailAlertId == emailAlert.EmailAlertId);

            // Add the updated scheduled email alerts  
            foreach (var recipient in recipients)
            {
                var scheduledEmail = new ScheduledEmailViewModel
                {
                    EmailAlertId = emailAlert.EmailAlertId,
                    Subject = emailAlert.Subject,
                    Body = emailAlert.Body,
                    RecipientEmail = recipient,
                    ScheduledTime = emailAlert.ScheduledTime,
                    IsActive = emailAlert.IsActive,
                    AlertType = emailAlert.AlertType,
                    Frequency = emailAlert.Frequency,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                if (emailAlert.IsActive && emailAlert.AlertType == 0)
                {
                    ScheduledEmails.Add(scheduledEmail);
                    _ = ScheduleEmailAlertAsync(scheduledEmail, scheduledEmail.CancellationTokenSource.Token);
                }

                success = true;
            }
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success;
    }

    public async Task<bool> AddNewScheduledEmailAlert(EmailAlert emailAlert, List<string> recipients)
    {
        bool success = false;
        try
        {
            foreach (var recipient in recipients)
            {
                var scheduledEmail = new ScheduledEmailViewModel
                {
                    EmailAlertId = emailAlert.EmailAlertId,
                    Subject = emailAlert.Subject,
                    Body = emailAlert.Body,
                    RecipientEmail = recipient,
                    ScheduledTime = emailAlert.ScheduledTime,
                    IsActive = emailAlert.IsActive,
                    AlertType = emailAlert.AlertType,
                    Frequency = emailAlert.Frequency,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                if (emailAlert.IsActive && emailAlert.AlertType == 0)
                {
                    ScheduledEmails.Add(scheduledEmail);
                    _ = ScheduleEmailAlertAsync(scheduledEmail, scheduledEmail.CancellationTokenSource.Token);
                }

                success = true;
            }
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    public async Task<bool> RemoveScheduledEmailAlert(int emailAlertId)
    {
        bool success = false;
        try
        {
            var scheduledEmailsToRemove = ScheduledEmails.Where(s => s.EmailAlertId == emailAlertId).ToList();
            foreach (var scheduledEmail in scheduledEmailsToRemove)
            {
                scheduledEmail.CancellationTokenSource.Cancel();
            }
            ScheduledEmails.RemoveAll(s => s.EmailAlertId == emailAlertId);
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    public async Task<bool> SubscribeToEmailAlert(EmailAlert emailAlert, string recipientEmail)
    {
        bool success = false;
        try
        {
            var scheduledEmail = ScheduledEmails.FirstOrDefault(se => se.EmailAlertId == emailAlert.EmailAlertId && se.RecipientEmail == recipientEmail);

            if (scheduledEmail == null)
            {
                var newScheduledEmail = new ScheduledEmailViewModel
                {
                    EmailAlertId = emailAlert.EmailAlertId,
                    Subject = emailAlert.Subject,
                    Body = emailAlert.Body,
                    RecipientEmail = recipientEmail,
                    ScheduledTime = emailAlert.ScheduledTime,
                    IsActive = emailAlert.IsActive,
                    AlertType = emailAlert.AlertType,
                    Frequency = emailAlert.Frequency,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                ScheduledEmails.Add(newScheduledEmail);
                _ = ScheduleEmailAlertAsync(newScheduledEmail, newScheduledEmail.CancellationTokenSource.Token);
            }
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    public async Task<bool> UnsubscribeFromEmailAlert(int emailAlertId, string recipientEmail)
    {
        bool success = false;
        try
        {
            var scheduledEmail = ScheduledEmails.FirstOrDefault(se => se.EmailAlertId == emailAlertId && se.RecipientEmail == recipientEmail);

            if (scheduledEmail != null)
            {
                scheduledEmail.CancellationTokenSource.Cancel();
                ScheduledEmails.Remove(scheduledEmail);
            }
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    public async Task<bool> EnableEmailAlert(EmailAlert emailAlert, string recipientEmail)
    {
        bool success = false;
        try
        {
            var scheduledEmail = new ScheduledEmailViewModel
            {
                EmailAlertId = emailAlert.EmailAlertId,
                Subject = emailAlert.Subject,
                Body = emailAlert.Body,
                RecipientEmail = recipientEmail,
                ScheduledTime = emailAlert.ScheduledTime,
                IsActive = emailAlert.IsActive,
                AlertType = emailAlert.AlertType,
                Frequency = emailAlert.Frequency,
                CancellationTokenSource = new CancellationTokenSource()
            };

            ScheduledEmails.Add(scheduledEmail);
            _ = ScheduleEmailAlertAsync(scheduledEmail, scheduledEmail.CancellationTokenSource.Token);

            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    public async Task<bool> DisableEmailAlert(int emailAlertId, string recipientEmail)
    {
        bool success = false;
        try
        {
            var scheduledEmail = ScheduledEmails.FirstOrDefault(se => se.EmailAlertId == emailAlertId && se.RecipientEmail == recipientEmail);

            if (scheduledEmail != null)
            {
                scheduledEmail.CancellationTokenSource.Cancel();
                ScheduledEmails.Remove(scheduledEmail);
            }
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }
    private async Task UpdateScheduledTimeForEmailAlert(int emailAlertId, DateTime newScheduledTime)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var emailAlertService = scope.ServiceProvider.GetRequiredService<IEmailAlertsService>();
            await emailAlertService.UpdateEmailAlertScheduledTimeAsync(emailAlertId, newScheduledTime);
        }
    }
    public async Task SendScheduledEmailAlertNow(EmailAlert emailAlert, List<string> recipients)
    {
        foreach (var recipient in recipients)
        {
            string userId = await _graphAPIService.GetUserIdByEmail(recipient);
            await _graphAPIService.SendEmailAlert(emailAlert.Subject, emailAlert.Body, recipient, userId);
        }
    }
}
