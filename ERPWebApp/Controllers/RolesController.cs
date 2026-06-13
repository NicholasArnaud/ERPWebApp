using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[Authorize(Roles = "Administrator,Developer")]
[AutoValidateAntiforgeryToken]
public class RolesController : Controller
{
    RoleManager<IdentityRole> roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        this.roleManager = roleManager;
    }
    // [Authorize(Policy= "RequireManagerRole")]
    public IActionResult Index()
    {
        var roles = roleManager.Roles.ToList();
        return View(roles);
    }
    //[Authorize(Policy = "RequireAdministratorRole")]
    [Authorize(Roles = "Administrator,Developer")]
    public IActionResult Create()
    {
        return View(new IdentityRole());
    }
    [Authorize(Roles = "Administrator,Developer")]
    [HttpPost]
    public async Task<IActionResult> Create(IdentityRole role)
    {
        await roleManager.CreateAsync(role);
        return RedirectToAction("Index");
    }
}
