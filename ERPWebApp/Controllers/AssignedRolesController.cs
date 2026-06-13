using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[AutoValidateAntiforgeryToken]
public class AssignedRolesController : Controller
{
    UserManager<IdentityUser> userManager;
    RoleManager<IdentityRole> roleManager;

    public AssignedRolesController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
    }
    public IActionResult Index()
    {
        var roles = roleManager.Roles.ToList();
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create(IdentityUser user, IdentityRole role)
    {
        await userManager.AddToRoleAsync(user, role.Name);
        return RedirectToAction("Index");
    }
}
