using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.CustomerSupportBasic + "," + RoleList.ProductionBasic)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class ShipEngineController(IOrderService orderService) : Controller
{
    private readonly IOrderService _orderService = orderService;

    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> GetShipmentLabelAsync(string TrackingNumber)
    {
        try
        {
            var label = await _orderService.GetShipEngineOrderLabel(TrackingNumber);
            string[] shipLabelData = { label.LabelDownload.Href, label.LabelDownload.Pdf, label.LabelId, label.TrackingStatus, label.Status };
            return Ok(shipLabelData);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public async Task<IActionResult> VoidShipmentLabel(string LabelId)
    {
        try
        {
            var returnValue = await _orderService.VoidFulfillmentLabel(LabelId);
            var message = returnValue.Message;
            if (!returnValue.Approved)
            {
                return BadRequest(message.ToString());
            }
            return Ok(message.ToString());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }
}
