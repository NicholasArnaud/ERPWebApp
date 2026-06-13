using ERPWebApp.Middleware;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class UserPreferenceController : Controller
{
    private readonly IUserPreferencesService _userPreferencesService;

    public UserPreferenceController(IUserPreferencesService userPreferencesService)
    {
        _userPreferencesService = userPreferencesService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult<DashboardConfig>> SaveLayout([FromBody] DashboardConfig dashboardConfig)
    {
        //validate user input
        if (dashboardConfig is null || string.IsNullOrEmpty(dashboardConfig.Name) || dashboardConfig.Name == DashboardNames.Invalid.ToString()
            || dashboardConfig.Layouts is null || !dashboardConfig.Layouts.Any())
        {
            return NotFound();
        }

        var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (currentUserID is null)
        {
            return NotFound();
        }

        var preference = await _userPreferencesService.UpdateDashboardConfigAsync(currentUserID, dashboardConfig);

        return Ok(preference);
    }
}
