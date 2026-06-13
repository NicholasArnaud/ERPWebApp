using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.SELLER)]
[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.CustomerSupportBasic + "," + RoleList.SellerBasic)]
[AutoValidateAntiforgeryToken]
public class SellerOrdersController : Controller
{
    UserManager<IdentityUser> _userManager;
    private readonly IWebhooks _webhooks;
    private readonly IOrderService _orderService;
    private readonly IOrderShippingService _orderShippingService;
    private readonly IShipStationStoreService _shipStationStoreService;

    public SellerOrdersController(UserManager<IdentityUser> userManager, IWebhooks webhooks, IOrderService orderService,
        IOrderShippingService orderShippingService, IShipStationStoreService shipStationStoreService)
    {
        _userManager = userManager;
        _webhooks = webhooks;
        _orderService = orderService;
        _orderShippingService = orderShippingService;
        _shipStationStoreService = shipStationStoreService;
    }

    public IActionResult Index(Order order)
    {
        ViewData["trackingStatus"] = "null";
        if (order == null)
            return View(new Order());
        return View(order);
    }

    [HttpPost, ActionName("GetOrderDetails")]
    
    public async Task<IActionResult> GetOrderDetailsAsync([Bind("orderNumber")] string OrderNumber)
    {
        try
        {
            OrderNumber = OrderNumber.Trim();
            var order = await _orderService.GetOrderByOrderNumberAsync(OrderNumber);
            if (order == null)
            {
                var user = await _userManager.GetUserAsync(User);
                var email = user.Email;
                var userStore = await _shipStationStoreService.GetShipStationStoreByEmailAsync(email);
                if (userStore != null)
                {
                    order = await _orderService.FindMissingOrder(OrderNumber, userStore.StoreId);
                }
            }
            ViewData["trackingStatus"] = "";
            ViewData["trackingNumber"] = "";
            if (order == null)
            {
                ViewData["trackingStatus"] = null;
                ViewData["trackingNumber"] = null;
                ModelState.AddModelError("orderNumber", "No order number was found");
                return View(nameof(Index));
            }
            if (order.orderStatus == OrderStatus.shipped)
            {
                var trackingNum = order.orderShipments.Any(x => x.voided == false) ? order.orderShipments.FirstOrDefault(x => x.voided == false).trackingNumber :
                    order.orderFulfillments.Any(x => x.voided == false) ? order.orderFulfillments.FirstOrDefault(x => x.voided == false).trackingNumber :
                    string.Empty;
                ViewData["trackingNumber"] = trackingNum;
                if (trackingNum != string.Empty)
                {
                    try
                    {
                        var labelDeserialized = await _orderService.GetShipEngineOrderLabel(trackingNum);
                        string trackingStatusShip = labelDeserialized.TrackingStatus;
                        if (trackingStatusShip == "delivered")
                        {
                            ViewData["trackingStatus"] = "DE";
                        }
                        else if (trackingStatusShip == "N/A")
                        {
                            ViewData["trackingStatus"] = "AC";
                        }
                        if (trackingStatusShip == "in_transit")
                        {
                            ViewData["trackingStatus"] = "IT";
                        }
                    }
                    catch
                    {
                        TempData["ErrorMessage"] =
                            "Not Shipped Through ShipEngine";
                    }
                }

            }
            return View(nameof(Index), order);
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    [HttpPost, ActionName("ForceGetOrderDetails")]
    
    public async Task<IActionResult> ForceGetOrderDetails(string OrderNumber)
    {

        if (!ModelState.IsValid)
        {
            return View(nameof(Index));
        }

        try
        {
            OrderNumber = OrderNumber.Trim().ToUpper();
            Order order = await _orderService.GetOrderByOrderNumberAsync(OrderNumber);
            order = (await _orderService.GetOrderUpdates([order.ERPOrderId])).SingleOrDefault();
            ViewData["trackingStatus"] = "";
            ViewData["trackingNumber"] = "";
            if (order.orderStatus == OrderStatus.shipped)
            {
                var trackingNum = order.orderShipments.Any(x => x.voided == false) ? order.orderShipments.FirstOrDefault(x => x.voided == false).trackingNumber :
                    order.orderFulfillments.Any(x => x.voided == false) ? order.orderFulfillments.FirstOrDefault(x => x.voided == false).trackingNumber :
                    string.Empty;
                ViewData["trackingNumber"] = trackingNum;
                if (trackingNum != string.Empty)
                {
                    try
                    {
                        var labelInfo = await _orderService.GetShipEngineOrderLabel(trackingNum);
                        string trackingStatusShip = labelInfo.TrackingStatus;
                        if (trackingStatusShip == "delivered")
                        {
                            ViewData["trackingStatus"] = "DE";
                        }
                        else if (trackingStatusShip == "N/A")
                        {
                            ViewData["trackingStatus"] = "AC";
                        }
                        if (trackingStatusShip == "in_transit")
                        {
                            ViewData["trackingStatus"] = "IT";
                        }
                    }
                    catch
                    {
                        TempData["ErrorMessage"] =
                            "Not Shipped Through ShipEngine";
                    }
                }

            }
            return View(nameof(Index), order);

        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }
}
