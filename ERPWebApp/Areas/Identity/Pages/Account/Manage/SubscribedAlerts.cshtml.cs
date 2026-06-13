using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using System.Security.Claims;


namespace ERPWebApp.Areas.Identity.Pages.Account.Manage
{
    public class SubscribedAlertsModel : PageModel
    {
        private readonly IEmailAlertsService _emailAlertsService;
        private readonly EmailSchedulerHostedService _emailSchedulerService;

        public SubscribedAlertsModel(IEmailAlertsService emailAlertsService, IEnumerable<IHostedService> hostedServices)
        {
            _emailAlertsService = emailAlertsService;
            _emailSchedulerService = (EmailSchedulerHostedService)hostedServices.FirstOrDefault(hs => hs is EmailSchedulerHostedService);
        }

        public List<EmailAlert> SubscribedAlerts { get; set; }
        public List<EmailAlert> UnsubscribedAlerts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _emailAlertsService.GetUserByIdAsync(userId);
            string userEmail = user.Email;

            SubscribedAlerts = await _emailAlertsService.GetSubscribedEmailAlertsAsync(userEmail);
            UnsubscribedAlerts = await _emailAlertsService.GetUnsubscribedEmailAlertsAsync(userEmail);

            return Page();
        }

        public async Task<IActionResult> OnPostSubscribeAsync(int emailAlertId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _emailAlertsService.GetUserByIdAsync(userId);
            string userEmail = user.Email;

            var emailAlert = await _emailAlertsService.GetEmailAlertByIdAsync(emailAlertId);

            await _emailAlertsService.CreateUserEmailAlertMappingAsync(emailAlertId, userEmail);
            _emailSchedulerService.SubscribeToEmailAlert(emailAlert, userEmail);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnsubscribeAsync(int emailAlertId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _emailAlertsService.GetUserByIdAsync(userId);
            string userEmail = user.Email;

            await _emailAlertsService.DeleteUserEmailAlertMappingAsync(emailAlertId, userEmail);
            _emailSchedulerService.UnsubscribeFromEmailAlert(emailAlertId, userEmail);

            return RedirectToPage();
        }
    }
}
