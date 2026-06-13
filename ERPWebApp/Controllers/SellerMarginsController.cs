using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Sellers;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.SELLER)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.SellerBasic)]
[AutoValidateAntiforgeryToken]
public class SellerMarginsController : Controller
{
    UserManager<IdentityUser> _userManager;
    private readonly ISellerMarginService _sellerMarginService;
    private readonly IShipStationStoreService _shipStationStoreService;

    public SellerMarginsController(
        UserManager<IdentityUser> userManager,
        ISellerMarginService sellerMarginService,
        IShipStationStoreService shipStationStoreService)
    {
        _userManager = userManager;
        _sellerMarginService = sellerMarginService;
        _shipStationStoreService = shipStationStoreService;
    }

    // GET: SellerMargins index  
    public async Task<IActionResult> IndexAsync()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account"); // Redirect the user to the login page if not authenticated  
        }
        var user = await _userManager.GetUserAsync(User);
        var email = user.Email;

        var userStore = await _shipStationStoreService.GetShipStationStoreByEmailAsync(email);
        var sellerMarginsWithStoreIdList = new SellerMarginsWithStoreId();
        ViewData["StoreList"] = new SelectList(await _shipStationStoreService.GetAllOrderedByNameAsync(), "StoreId", "StoreName");

        if (userStore == null)
        {
            if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.FinancialManager) || User.IsInRole(RoleList.CustomerSupportManager))
            {
                sellerMarginsWithStoreIdList.shipstationstore = await _shipStationStoreService.GetFirstOrderedByNameAsync();
            }
            else
            {
                sellerMarginsWithStoreIdList.shipstationstore = new ShipStationStore();
                sellerMarginsWithStoreIdList.shipstationstore.StoreName = "No Store Found";
            }
        }
        else
        {
            sellerMarginsWithStoreIdList.shipstationstore = userStore;
        }

        sellerMarginsWithStoreIdList.sellerMargins = await _sellerMarginService.GetSellerMarginsAsync();
        return View(sellerMarginsWithStoreIdList);
    }

    public class SellerMarginsWithStoreId
    {
        public List<SellerMargins> sellerMargins = new();
        public ShipStationStore shipstationstore;
    }

    [HttpPost]
    public async Task<IActionResult> PullSellerMarginsByDateRange(int? StoreId, DateTime StartDate, DateTime EndDate)
    {
        var sellerMargins = await _sellerMarginService.GetSellerMarginsByDateRangeAsync(StoreId, StartDate, EndDate);
        var result = sellerMargins.Select(m => new {
            m.OrderNumber,
            ShipDate = m.ShipDate.ToString("MM/dd/yyyy"),
            m.StoreName,
            m.ServiceCode,
            m.TrackingNumber,
            m.StoreItemsCost,
            m.CustomerItemsCost,
            m.ShippingCost,
            m.ShipmentCost,
            m.StoreCostWithEtsy,
            m.StoreCostDiffSubfulfillmentAndShipping
        }).ToList();
        return Json(result);
    }


    //calls the partiallist to reload partialview  
    [HttpGet]
    public IActionResult PartialViewIndex(List<SellerMargins> sellerMargins)
    {
        return PartialView("PartialIndex", sellerMargins);
    }

}
