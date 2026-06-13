using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers.PurchaseOrders;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ProductionBasic + "," + RoleList.InventoryBasic)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ShippingMethodProviderController : Controller
{
    private readonly ApplicationDbContext _context;
    public ShippingMethodProviderController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var _shipMethodProvider = new MethodProviderView();
        _shipMethodProvider.Method = _context.ShippingMethod;
        _shipMethodProvider.Provider = _context.ShippingProvider;
        return View();
    }
    public class MethodProviderView
    {
        public IEnumerable<ShippingMethod> Method { get; set; }
        public IEnumerable<ShippingProvider> Provider { get; set; }
    }
}