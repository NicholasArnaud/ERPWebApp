using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace ERPWebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
[AutoValidateAntiforgeryToken]
public class RegisterModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IGraphAPIService _graphAPIService;

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IGraphAPIService graphAPIService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _roleManager = roleManager;
        _context = context;
        _graphAPIService = graphAPIService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(
            100,
            ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(
            "Password",
            ErrorMessage = "The password and confirmation password do not match."
        )]
        public string ConfirmPassword { get; set; }

        public string Name { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ViewData["roles"] = _roleManager.Roles.ToList();
        ReturnUrl = returnUrl;
        ExternalLogins = (
            await _signInManager.GetExternalAuthenticationSchemesAsync()
        ).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var role = _roleManager.FindByIdAsync(Input.Name).Result;
        ExternalLogins = (
            await _signInManager.GetExternalAuthenticationSchemesAsync()
        ).ToList();
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = Input.Email.Split('@')[0],
                Email = Input.Email
            };
            IdentityResult result = null;
            //check if given exists in ShipStationStore table
            bool SellerEmailExists = _context.ShipStationStore.Any(
                store => store.Email == user.Email
            );
            //create regular expression to check format of the given email
            Regex re = new Regex(@"^[a-zA-Z0-9._%+-]+(@completeful.com|@ERP.com)$");
            //in case of given email not existing in ShipStationStore table, check if it follows the format of regular Expression
            bool IsERPEmail = re.IsMatch(user.Email);
            //if store and email match, make sure they are registering under Restricted User role. if email is @completeful.com, make sure they are NOT in seller role, and register
            if (
                (SellerEmailExists && Input.Name == "7acbde4f-3ce8-4d94-8323-f7e0608c06a9")
                || (IsERPEmail && Input.Name != "6ee08639-b482-4713-9d4a-a1619f89f6d9")
            )
            {
                if (SellerEmailExists)
                {
                    result = await CreateUser(user, nameof(RoleList.SellerBasic), RoleList.SellerBasic, returnUrl);
                    Console.WriteLine("sellerEmailExists");
                }
                else if (IsERPEmail)
                {
                    result = await CreateUser(user, nameof(RoleList.BasicUser), RoleList.BasicUser, returnUrl);
                    Console.WriteLine("isERPEmail");
                }

            }
            else
            {
                result = await CreateUser(user, nameof(RoleList.RestrictedUser), RoleList.RestrictedUser, returnUrl);
            }

            if (result.Succeeded)
            {

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage(
                        "RegisterConfirmation",
                        new { email = Input.Email, returnUrl = returnUrl }
                    );
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        ViewData["roles"] = _roleManager.Roles.ToList();
        // If we got this far, something failed, redisplay form
        return Page();
    }

    private async Task<IdentityResult> CreateUser(IdentityUser user, string roleName, string role, string returnUrl)
    {
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            //_logger.LogInformation("User created a new account with password.");

            bool isExistsRole = _context.Roles.Any(x => x.Id == roleName);
            if (!isExistsRole)
            {
                var newRole = new IdentityRole
                {
                    ConcurrencyStamp = role,
                    Name = role,
                    NormalizedName = role.ToUpper(),
                    Id = roleName
                };
                _context.Roles.Add(newRole);
                _context.SaveChanges();
            }

            await _userManager.AddToRoleAsync(user, role);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new
                {
                    area = "Identity",
                    userId = user.Id,
                    code = code,
                    returnUrl = returnUrl
                },
                protocol: Request.Scheme
            );

            await _graphAPIService.SendEmailAlert(
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                Input.Email,
                null
            );
        }

        return result;
    }
}