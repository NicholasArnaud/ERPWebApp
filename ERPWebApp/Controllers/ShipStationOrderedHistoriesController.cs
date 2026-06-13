using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.ORDER)]
[CwaFeatureGate(CwaFeatures.SELLER)]
[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.SellerBasic)]
[AutoValidateAntiforgeryToken]
public class ShipStationOrderedHistoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private static DateTime _date = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    private static IShipStationOrderedHistoryService _shipStationOrderedHistoryService;
    private static IDepartmentService _departmentService;
    public ShipStationOrderedHistoriesController(
        IShipStationOrderedHistoryService shipStationOrderedHistoryService,
        IDepartmentService departmentService
    )
    {
        _shipStationOrderedHistoryService = shipStationOrderedHistoryService;
        _departmentService = departmentService;
    }

    // GET: InventoryShippedHistories
    public async Task<IActionResult> Index(int DepartmentId = 0)
    {
        var orderedHistories = _shipStationOrderedHistoryService.GetShipStationOrderedHistory(DepartmentId);

        //get detpartment list for the filtering
        var departmentList = _departmentService.GetList(d => d.IsActive,
            orderSelectors: [d => d.DepartmentName]);
        ViewData["DepartmentList"] = new SelectList(
            departmentList,
            "DepartmentId",
            "DepartmentName"
        );

        ViewData["SelectedDepartmentId"] = DepartmentId;

        foreach (var item in orderedHistories)
        {
            double salesDiff = 0.0;
            double avg3Days;
            double avg30Days;
            if (item.OrderedIn3Days != 0 && item.OrderedIn30Days != 0)
            {
                avg3Days = item.OrderedIn3Days / 3.0;
                avg30Days = item.OrderedIn30Days / 30.0;
                //divide by 0 check
                if (avg30Days != 0)
                {
                    salesDiff = (avg3Days / avg30Days) - 1;
                }
                else
                {
                    salesDiff = 0.0;
                }
            }
            else if (item.OrderedIn3Days == 0)
            {
                avg30Days = item.OrderedIn30Days / 30.0;
                //divide by 0 check
                if (avg30Days != 0)
                {
                    salesDiff = (0.01 / avg30Days) - 1;
                }
                else
                {
                    salesDiff = 0.0;
                }
            }

            item.SalesTrend = (decimal)Math.Round(salesDiff, 2) * 100;
        }
        return View(orderedHistories);
    }

}
