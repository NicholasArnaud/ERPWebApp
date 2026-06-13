using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ProductionBasic)]
[AutoValidateAntiforgeryToken]
public class BarcodeScansController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IWebhooks _webhooks;
    private static BarcodeViewModel _barcodeViewModel = new();
    private readonly IShipStationAwaitingOrderServices _shipStationAwaitingOrderServices;
    private readonly IBarcodeScanService _barcodeScanService;
    private readonly IOrderTagService _orderTagService;
    public BarcodeScansController(IOrderService orderService,
        IWebhooks webhooks,
        IShipStationAwaitingOrderServices shipStationAwaitingOrderServices,
        IBarcodeScanService barcodeScanService,
        IOrderTagService orderTagService)
    {
        _orderService = orderService;
        _webhooks = webhooks;
        _shipStationAwaitingOrderServices = shipStationAwaitingOrderServices;
        _barcodeScanService = barcodeScanService;
        _orderTagService = orderTagService;
    }

    public async Task<IActionResult> Index(int[]? tagIds)
    {
        await LoadOrderTags(tagIds);
        _barcodeViewModel.TagIds = tagIds?.ToList() ?? new List<int>();
        return View(_barcodeViewModel);
    }

    [HttpPost]
    
    public async Task<ActionResult> Create(BarcodeViewModel barcode)
    {
        try
        {
            if (!barcode.TagIds.Any())
            {
                TempData["errorMessage"] = "No Tags Selected or Internal Error";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrEmpty(barcode.BarcodeScan.BarcodeScanCode))
            {
                TempData["errorMessage"] = "No Barcode Scanned or empty field";
                return RedirectToAction("Index", new { barcode.TagIds });
            }
            _barcodeViewModel.TagIds = barcode.TagIds;
            string OrderId = "";
            var shipStationAwaitingOrder = await _orderService.GetAsync(x => x.orderNumber == barcode.BarcodeScan.BarcodeScanCode);
            if (shipStationAwaitingOrder != null)
            {
                foreach (int tagId in barcode.TagIds)
                {
                    await _orderService.SetShipStationCompletedTag(shipStationAwaitingOrder.orderId.ToString(), tagId.ToString());
                }
                TempData["successMessage"] = "Barcode submitted";
            }
            else
            {
                Console.WriteLine("Order Not In Database");
                var foundOrders = await _orderService.GetShipStationOrderDetails(barcode.BarcodeScan.BarcodeScanCode,0);
                Order orderDetails = foundOrders.FirstOrDefault();
                foreach (int tagId in barcode.TagIds)
                {
                    var innerSet = await _orderService.SetShipStationCompletedTag(OrderId, tagId.ToString());
                }
                await _orderService.ProcessOrderNotify([orderDetails]);
                TempData["successMessage"] = "Barcode submitted";
            }

            DateTime now =
              TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            barcode.BarcodeScan.ModifyDate = now;
            barcode.BarcodeScan.ShipStationOrderId = shipStationAwaitingOrder?.orderId.ToString();
            await _barcodeScanService.AddAsync(barcode.BarcodeScan);
            return RedirectToAction("index", new { barcode.TagIds });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An issue has arisen:" + ex.Message);
            TempData["errorMessage"] = "An ERROR has occurred. Barcode not properly submitted. Barcode: " +
              barcode;
            return BadRequest(ex);
        }
    }

    private async Task LoadOrderTags(int[]? tagIds)
    {
        List<OrderTag> tagList = await _orderTagService.GetListAsync(
           query => query.Select(tag => new OrderTag { tagId = tag.tagId, name = tag.name })
           .OrderBy(tag => tag.name)
       );
        var selectedTags = tagList.Where(tagIds.Equals).ToArray();
        ViewData["TagIds"] = new MultiSelectList(
           tagList,
           "tagId",
           "name",
           tagIds
       );
    }
}
