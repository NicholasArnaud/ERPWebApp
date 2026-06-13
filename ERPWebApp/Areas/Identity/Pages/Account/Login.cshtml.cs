using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
[AutoValidateAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IUserPreferencesService _userPreferences;
    //TO DO: Reimplement preferences later.
    //private readonly IHttpContextAccessor _httpContext;

    public LoginModel(SignInManager<IdentityUser> signInManager,
        ILogger<LoginModel> logger,
        UserManager<IdentityUser> userManager,
        IUserPreferencesService userPreferences)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _userPreferences = userPreferences;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
       
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }
        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {

        returnUrl ??= Url.Content("~/");

        var goodUrl = returnUrl.Split("/");

        returnUrl = "/" + goodUrl[1];

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(Input.Email.Split('@')[0], Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                //_logger.LogInformation("User logged in.");
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == Input.Email);
                var userPreference = await _userPreferences.GetAsync(x => x.UserId == user.Id);
                if (userPreference != null)
                {
                    //TO DO: Reimplement preferences later.
                    //if (!string.IsNullOrEmpty(userPreference.Theme))
                    //{
                    //    HttpContext.Session.SetString("userPreferenceTheme", userPreference.Theme);
                    //}

                    if (userPreference.PreferDashboard?.ToLower() != "default" & string.IsNullOrEmpty(goodUrl[1]))
                    {
                        return RedirectToAction("Index", userPreference.PreferDashboard);
                    }
                    else
                    {
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    return LocalRedirect(returnUrl);
                }
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                //_logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
