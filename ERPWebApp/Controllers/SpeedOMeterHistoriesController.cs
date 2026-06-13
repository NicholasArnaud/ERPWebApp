using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class SpeedOMeterHistoriesController : Controller
{
    private readonly IOrderShippingService _orderShippingService;
    public SpeedOMeterHistoriesController(IOrderShippingService orderShippingService)
    {
        _orderShippingService = orderShippingService;
    }

    // GET: SpeedOMeterHistories
    public async Task<IActionResult> Index(DateTime? startDate,DateTime? endDate)
    {
        if(!startDate.HasValue || !endDate.HasValue)
        {
            return View(await _orderShippingService.GetDepartmentShippedTotalsByDateList());
        }
        return View(await _orderShippingService.GetAllDepartmentShippedTotalsInRangeAsync(startDate.Value,endDate.Value));
    }
}
