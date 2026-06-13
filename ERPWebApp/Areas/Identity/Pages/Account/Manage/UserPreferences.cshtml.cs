using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ERPWebApp.Areas.Identity.Pages.Account.Manage
{
    public class UserPreferencesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserPreferencesService _userPreferences;
        private readonly IDepartmentService _departmentService;

        public List<SelectListItem> DashBoards { get; set; }
        public List<SelectListItem> Theme { get; set; }
        public List<SelectListItem> Departments { get; set; }
        [BindProperty]
        public string SelectedDashBoard { get; set; }
        [BindProperty]
        public string SelectedTheme { get; set; }
        [BindProperty]
        public int? SelectedDepartment { get; set; }
        public UserPreferencesModel(UserManager<IdentityUser> userManager, IUserPreferencesService userPreferences, IDepartmentService departmentService)
        {
            _userManager = userManager;
            _userPreferences = userPreferences;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> OnGet()
        {
            await onLoad();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await onLoad();
                return Page();
            }
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var preferenceResult = await _userPreferences.GetAsync(x => x.UserId == currentUserID);
            if (preferenceResult == null)
            {
                var preference = new UserPreferences
                {
                    UserId = currentUserID,
                    PreferDashboard = SelectedDashBoard,
                    Theme = SelectedTheme,
                    PreferDepartment = SelectedDepartment

                };
                await _userPreferences.AddAsync(preference);
            }
            else
            {
                preferenceResult.PreferDashboard = SelectedDashBoard;
                preferenceResult.Theme = SelectedTheme;
                preferenceResult.PreferDepartment = SelectedDepartment;
                await _userPreferences.UpdateAsync(preferenceResult);
            }
            //TO DO: Reimplement preferences later.
            //HttpContext.Session.SetString("userPreferenceTheme", SelectedTheme);
            await onLoad();
            return RedirectToPage();
        }

        private async Task onLoad()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var preferenceResult = await _userPreferences.GetAsync(x => x.UserId == currentUserID);

            DashBoards = new List<SelectListItem>
            {
                new SelectListItem { Value = "default", Text = " Default" },
                new SelectListItem { Value = "Home", Text = " Operations" },
                new SelectListItem { Value = "Financials", Text = " Financials" },
                new SelectListItem { Value = "Inventory", Text = " Inventory" },
            };
            Theme = new List<SelectListItem>
            {
                new SelectListItem { Value = "dark", Text = " Dark" },
                new SelectListItem { Value = "light", Text = " Light" },
            };

            var departments = await _departmentService.GetListAsync(d => d.IsActive && d.IsProduction);
            Departments = departments.Select(x => new SelectListItem
            {
                Value = x.DepartmentId.ToString(),
                Text = x.DepartmentName
            }).ToList();

            if (preferenceResult != null)
            {
                SelectedDashBoard = preferenceResult.PreferDashboard;
                SelectedTheme = preferenceResult.Theme;
                SelectedDepartment = preferenceResult.PreferDepartment;
            }
        }
    }
}
