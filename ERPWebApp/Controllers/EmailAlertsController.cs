using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Developer + "," + RoleList.Manager)]
[AutoValidateAntiforgeryToken]
public class EmailAlertsController : Controller
{
    private readonly IEmailAlertsService _emailAlertsService;
    private readonly EmailSchedulerHostedService _emailSchedulerService;
    private readonly ITriggerEmailAlertService _triggerEmailAlertService;

    public EmailAlertsController(IEmailAlertsService emailAlertsService,
                                 IEnumerable<IHostedService> hostedServices,
                                 ITriggerEmailAlertService triggerEmailAlertService)
    {
        _emailAlertsService = emailAlertsService;
        _emailSchedulerService = (EmailSchedulerHostedService)hostedServices.FirstOrDefault(hs => hs is EmailSchedulerHostedService);
        _triggerEmailAlertService = triggerEmailAlertService;
    }

    public async Task<IActionResult> Index()
    {
        var userEmails = await _emailAlertsService.GetAllUserEmailsAsync();
        var scheduledEmails = await _emailAlertsService.GetAllEmailAlertsWithRecipients();

        // Add the recipients to each ScheduledEmail object  
        foreach (var email in scheduledEmails)
        {
            Debug.WriteLine("Email: " + email.ScheduledTimeOnly);
            email.Recipients = await _emailAlertsService.GetRecipientsForEmailAlert(email.EmailAlertId);
            email.AlertTemplateId = await _emailAlertsService.GetAlertTemplateIdByEmailAlertId(email.EmailAlertId);
            email.ScheduledTime = ConvertDateTimeUtcToCst(email.ScheduledTime);
            email.ScheduledTimeOnly = ConvertTimeOnlyUtcToCst(email.ScheduledTimeOnly);
        }

        var alertTriggerTemplateMappings = _emailAlertsService.GetAlertTriggerTemplateMappings();

        var viewModel = new EmailAlertsViewModel
        {
            ScheduledEmails = scheduledEmails,
            UserEmails = userEmails,
            AlertTriggerTemplateMappings = alertTriggerTemplateMappings
        };

        return View(viewModel);
    }
    private DateTime ConvertDateTimeUtcToCst(DateTime utcDateTime)
    {
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        DateTime cstDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
        return cstDateTime;
    }
    private DateTime ConvertDateTimeCstToUtc(DateTime cstDateTime)
    {
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(cstDateTime, cstZone);
        return utcDateTime;
    }

    private TimeOnly ConvertTimeOnlyUtcToCst(TimeOnly utcTime)
    {
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        DateTime utcDateTime = DateTime.UtcNow.Date + utcTime.ToTimeSpan();
        DateTime cstDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
        TimeOnly cstTime = TimeOnly.FromDateTime(cstDateTime);
        return cstTime;
    }

    [HttpPost]
    
    public async Task<IActionResult> CreateEmailAlert([FromForm] CreateEmailAlertViewModel createEmailAlertViewModel)
    {
        var emailAlert = createEmailAlertViewModel.EmailAlert;

        try
        {
            if (emailAlert == null)
            {
                return BadRequest("Email alert data is missing.");
            }

            // Set the IsActive, CreateDate, ModifyDate, and CreatedBy fields  
            emailAlert.IsActive = true;
            emailAlert.AlertTemplateId = createEmailAlertViewModel.AlertTemplateId;
            emailAlert.CreateDate = DateTime.UtcNow;
            emailAlert.ModifyDate = DateTime.UtcNow;
            emailAlert.CreatedBy = User.Identity.Name;
            emailAlert.ScheduledTime = ConvertDateTimeCstToUtc(emailAlert.ScheduledTime);

            // Create the EmailAlert and UserEmailAlertMappings using the service method  
            var (success, message) = await _emailAlertsService.CreateEmailAlertWithMappingsAsync(emailAlert, createEmailAlertViewModel.Recipients);

            if (success)
            {
                _emailSchedulerService.AddNewScheduledEmailAlert(emailAlert, createEmailAlertViewModel.Recipients);
                return RedirectToAction("Index", "EmailAlerts");
            }
            else
            {
                return Json(new { success = false, error = message });
            }
        }
        catch (Exception ex)
        {
            // Log or display the error details  
            string errorMessage = $"Error: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
            }
            Console.WriteLine(errorMessage);
            return Json(new { success = false, error = errorMessage });
        }
    }

    [HttpPost]
    
    public async Task<IActionResult> UpdateEmailAlert([FromForm] CreateEmailAlertViewModel updateEmailAlertViewModel)
    {
        try
        {
            // Fetch the email alert's status from the database  
            bool emailAlertIsActive = await _emailAlertsService.GetEmailAlertStatus(updateEmailAlertViewModel.EmailAlert.EmailAlertId);
            updateEmailAlertViewModel.EmailAlert.IsActive = emailAlertIsActive;
            updateEmailAlertViewModel.EmailAlert.AlertTemplateId = updateEmailAlertViewModel.AlertTemplateId;
            updateEmailAlertViewModel.EmailAlert.ScheduledTime = ConvertDateTimeCstToUtc(updateEmailAlertViewModel.EmailAlert.ScheduledTime);

            await _emailAlertsService.UpdateEmailAlertAsync(updateEmailAlertViewModel.EmailAlert, updateEmailAlertViewModel.Recipients);
            await _emailSchedulerService.UpdateScheduledEmailAlert(updateEmailAlertViewModel.EmailAlert, updateEmailAlertViewModel.Recipients);
            return RedirectToAction("Index", "EmailAlerts");
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRecipientsForEmailAlert(int emailAlertId)
    {
        try
        {
            var recipients = await _emailAlertsService.GetRecipientsForEmailAlert(emailAlertId);
            return Json(recipients);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
            }
            Console.WriteLine(errorMessage);
            return Json(new { success = false, error = errorMessage });
        }
    }

    [HttpPost]
    
    public async Task<IActionResult> DeleteEmailAlert(int emailAlertId)
    {
        try
        {
            await _emailAlertsService.DeleteEmailAlertAsync(emailAlertId);
            await _emailSchedulerService.RemoveScheduledEmailAlert(emailAlertId);
            return RedirectToAction("Index", "EmailAlerts");
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    
    public async Task<IActionResult> ChangeEmailAlertStatus(int emailAlertId, bool newStatus)
    {
        var emailAlert = await _emailAlertsService.ChangeEmailAlertStatus(emailAlertId, newStatus);

        var userEmails = await _emailAlertsService.GetRecipientsForEmailAlert(emailAlertId);
        foreach (var userEmail in userEmails)
        {
            if (newStatus)
            {
                await _emailSchedulerService.EnableEmailAlert(emailAlert, userEmail);
            }
            else
            {
                await _emailSchedulerService.DisableEmailAlert(emailAlertId, userEmail);
            }
        }

        return RedirectToAction("Index", "EmailAlerts");
    }

    public IActionResult EditEmailAlertModal(int emailAlertId, List<string> userEmails)
    {
        ViewBag.EmailAlertId = emailAlertId;
        ViewBag.UserEmails = userEmails;
        return PartialView("Edit");
    }

    [HttpPost]
    
    public async Task<IActionResult> SendEmailAlertNow(int emailAlertId)
    {
        try
        {
            var emailAlert = await _emailAlertsService.GetEmailAlertByIdAsync(emailAlertId);
            var recipients = await _emailAlertsService.GetRecipientsForEmailAlert(emailAlertId);

            if (emailAlert.AlertType == AlertType.TimeBased)
            {
                await _emailSchedulerService.SendScheduledEmailAlertNow(emailAlert, recipients);
            }
            else if (emailAlert.AlertType == AlertType.TriggerBased)
            {
                await _triggerEmailAlertService.SendTriggerEmailAlertNow(emailAlert, recipients);
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }

        return RedirectToAction("Index", "EmailAlerts");
    }
}