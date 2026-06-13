using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.SELLER)]
[AutoValidateAntiforgeryToken]
public class WelcomeSeller : Controller
{
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.CustomerSupportBasic + "," + RoleList.SellerBasic + "," + RoleList.RestrictedUser)]

    public IActionResult Index()
    {
        return View();
    }
}
