using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class HomeController : Controller
{
    private readonly DateTime _now;
    private Home _Home;
    private HomeViewModel _HomeViewModel;
    private readonly IOrderShippingService _orderShippingService;
    private readonly IDepartmentService _departmentService;
    private readonly ISpeedOMeterGoalService _speedOMeterGoalService;
    private readonly IEmployeeService _employeeService;
    private readonly IStocksService _stocksService;
    private readonly ISiteService _siteService;
    private readonly IShipStationStoreService _shipStationStoreService;
    private readonly IProductionVsLaborCostPriceService _productionVsLaborCostPriceService;
    private readonly IHomeService _homeService;
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly IMyDashService _myDashService;


    public HomeController(
        IOrderShippingService orderShippingService,
        IDepartmentService departmentService,
        ISpeedOMeterGoalService speedOMeterGoalService,
        IEmployeeService employeeService,
        IStocksService stocksService,
        IShipStationStoreService shipStationStoreService,
        ISiteService siteService,
        IProductionVsLaborCostPriceService productionVsLaborCostPriceService,
        IHomeService homeService,
        IUserPreferencesService userPreferencesService,
        IMyDashService myDashService
    )
    {
        _productionVsLaborCostPriceService = productionVsLaborCostPriceService;
        _siteService = siteService;
        _orderShippingService = orderShippingService;
        _departmentService = departmentService;
        _speedOMeterGoalService = speedOMeterGoalService;
        _employeeService = employeeService;
        _stocksService = stocksService;
        _shipStationStoreService = shipStationStoreService;
        _homeService = homeService;
        _userPreferencesService = userPreferencesService;
        _HomeViewModel = new HomeViewModel();
        _Home = new Home();
        _myDashService = myDashService;

        _now = TimeZoneInfo.ConvertTime(
            DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );
    }

    //public async Task<IActionResult> Index()
    public async Task<IActionResult> IndexAsync()
    {

        if (this.User.IsInRole(RoleList.SellerBasic) || this.User.IsInRole(RoleList.RestrictedUser))
        {
            return RedirectToAction("Index", "WelcomeSeller");
        }
        try
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (currentUserID is not null)
            {
                _Home.DashboardLayouts = await _userPreferencesService.GetDashboardLayoutByDashboardAsync(currentUserID, DashboardNames.DashboardOperations.ToString());

                var result = await _myDashService.GetUserDashboadData(currentUserID);
                if (result != null) {
                    _Home.SpeedOMeter = result.SpeedOMeter;
                    _Home.DepartmentOrderHistory = result.DepartmentOrderHistory;
                    _Home.TopDepartment = result.TopDepartment;
                }  
            }

            _Home.SpeedOMeterGoal = await _speedOMeterGoalService.GetLastSpeedOMeterGoalAsync();
            _Home.ProductionVsLaborCostPrice = await _employeeService.GetLastProductionVsLaborCostPrice();
            
            var userPreferences = await _userPreferencesService.GetPreferencesByUserIdAsync(currentUserID);
            var preferDepartment = userPreferences?.PreferDepartment;

            _Home.Department = await _departmentService.GetListAsync(
                x => x.IsProduction == true,
                orderSelectors: new Expression<Func<Department, string>>[]{
                    x=> x.DepartmentName
                }
            );

            ViewData["Department"] = new SelectList(
                _Home.Department,
                "DepartmentId",
                "DepartmentName",
                preferDepartment 
            );    

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return View(_Home);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult EULA()
    {
        return View();
    }

    public Task<List<TopDepartment>> TopDepartment(DateTime startDate, DateTime endDate)
        => _homeService.TopDepartment(startDate, endDate);

    [CwaFeatureGate(CwaFeatures.ORDER)]
    public Task<List<TallyDto>> GetDailyOrderCompletionCount()
        => _homeService.GetDailyOrderCompletionCount();

    [CwaFeatureGate(CwaFeatures.ORDER)]
    [CwaFeatureGate(CwaFeatures.SELLER)]

    public async Task<string> GetDailyShipstationOrdersAll(DateTime startDate, DateTime endDate, int? departmentId = null)
    {
        var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var userPreferences = await _userPreferencesService.GetPreferencesByUserIdAsync(currentUserID);
        var preferDepartment = userPreferences?.PreferDepartment;

        if (!departmentId.HasValue && preferDepartment.HasValue)
        {
            departmentId = preferDepartment;
        }
        else if(departmentId == 0)
        {
            departmentId = null;
        }

        var homeInformation = await _homeService.GetDailyShipstationOrdersAll(startDate, endDate, departmentId);

        var orderDateOrdersIn = homeInformation
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new Tuple<DateTime, int>(g.Key, g.Sum(o => o.OrdersIn)))
            .OrderBy(o => o.Item1)
            .ToList();

        var shipDateOrdersOut = homeInformation
            .Where(o => o.ShipDate != default(DateTime) && o.ShipDate >= startDate && o.ShipDate <= endDate)
            .GroupBy(o => o.ShipDate.Date)
            .Select(g => new Tuple<DateTime, int>(g.Key, g.Sum(o => o.OrdersOut)))
            .OrderBy(o => o.Item1)
            .ToList();

        // Grabbing unique dates from both sections.
        var uniqueDates = orderDateOrdersIn.Select(x => x.Item1)
            .Union(shipDateOrdersOut.Select(x => x.Item1))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        // Joining unique dates with OrdersIn and OrdersOut  
        var ordersInWithZeros = uniqueDates
            .GroupJoin(orderDateOrdersIn,
                date => date,
                orderIn => orderIn.Item1,
                (date, orderIns) => new { Date = date, OrderIns = orderIns })
            .SelectMany(x => x.OrderIns.DefaultIfEmpty(),
                (x, y) => new Tuple<DateTime, int>(x.Date, y != null ? y.Item2 : 0))
            .OrderBy(x => x.Item1)
            .ToList();

        var ordersOutWithZeros = uniqueDates
            .GroupJoin(shipDateOrdersOut,
                date => date,
                orderOut => orderOut.Item1,
                (date, orderOuts) => new { Date = date, OrderOuts = orderOuts })
            .SelectMany(x => x.OrderOuts.DefaultIfEmpty(),
                (x, y) => new Tuple<DateTime, int>(x.Date, y != null ? y.Item2 : 0))
            .OrderBy(x => x.Item1)
            .ToList();

        //Essentially what this does is, if there's an entry for a date in OrdersIn, but not OrdersOut, it will add an entry for that date with a count of 0 for OrdersOut.
        //This is for OrdersOut graph wonkiness, and also to ensure that the x values of each data set matches, that way the tooltip on the graph displays both instead of one.
        var result = new
        {
            OrderDateOrdersIn = ordersInWithZeros,
            ShipDateOrdersOut = ordersOutWithZeros
        };

        return JsonConvert.SerializeObject(result);
    }

    //An attempt at sending an error log. Not working
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //[AllowAnonymous]
    [Route("Error")]
    public IActionResult Error(string requestUrl)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ViewBag.ErrorCode = Response.StatusCode;

        if (exceptionHandlerPathFeature != null)
        {
            ViewBag.ErrorPath = exceptionHandlerPathFeature.Path;
            ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
            ViewBag.ErrorSource = exceptionHandlerPathFeature.Error.Source;
            ViewBag.ErrorStackTrace = exceptionHandlerPathFeature.Error.StackTrace;
        }

        return View(new ErrorViewModel { RequestId = requestId });
    }

    public async Task<SpeedOMeterGoal> GetSpeedOMeterGoal()
    {
        return await _speedOMeterGoalService.GetLastSpeedOMeterGoalAsync();
    }
    public async Task<string> GetOrderShipmentsByService()
    {
        var getTodaysShipments = await _homeService.GetDaysShipmentsByServiceCode();
        var jsonResult = System.Text.Json.JsonSerializer.Serialize(getTodaysShipments);
        return jsonResult;
    }

    // POST: SpeedOMeterGoals/CreateSOMG
    [HttpPost]
    
    public async Task<IActionResult> CreateSOMG([Bind("SpeedOMeterGoalId,ElectroplatingGoal,EmbroideryGoal,EngravingGoal,MetalGoal,UVGoal, SublimationGoal, PlantGoal, WoodGoal")] SpeedOMeterGoal speedOMeterGoal)
    {
        if (ModelState.IsValid)
        {
            var lastSpeedOMeterGoal = await _speedOMeterGoalService.GetLastSpeedOMeterGoalAsync();

            speedOMeterGoal.ElectroplatingGoal = speedOMeterGoal.ElectroplatingGoal == 0 ? lastSpeedOMeterGoal.ElectroplatingGoal : speedOMeterGoal.ElectroplatingGoal;
            speedOMeterGoal.EmbroideryGoal = speedOMeterGoal.EmbroideryGoal == 0 ? lastSpeedOMeterGoal.EmbroideryGoal : speedOMeterGoal.EmbroideryGoal;
            speedOMeterGoal.EngravingGoal = speedOMeterGoal.EngravingGoal == 0 ? lastSpeedOMeterGoal.EngravingGoal : speedOMeterGoal.EngravingGoal;
            speedOMeterGoal.MetalGoal = speedOMeterGoal.MetalGoal == 0 ? lastSpeedOMeterGoal.MetalGoal : speedOMeterGoal.MetalGoal;
            speedOMeterGoal.UVGoal = speedOMeterGoal.UVGoal == 0 ? lastSpeedOMeterGoal.UVGoal : speedOMeterGoal.UVGoal;
            speedOMeterGoal.SublimationGoal = speedOMeterGoal.SublimationGoal == 0 ? lastSpeedOMeterGoal.SublimationGoal : speedOMeterGoal.SublimationGoal;
            speedOMeterGoal.PlantGoal = speedOMeterGoal.PlantGoal == 0 ? lastSpeedOMeterGoal.PlantGoal : speedOMeterGoal.PlantGoal;
            speedOMeterGoal.WoodGoal = speedOMeterGoal.WoodGoal == 0 ? lastSpeedOMeterGoal.WoodGoal : speedOMeterGoal.WoodGoal;

            speedOMeterGoal.ModifyByUser = User.Identity.Name;
            speedOMeterGoal.ModifyDate = _now;
            await _speedOMeterGoalService.AddAsync(speedOMeterGoal);
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    // POST: ProductionVsLaborCostPrices/CreatePVLCPrices
    [HttpPost]
    
    public async Task<IActionResult> CreatePVLCPrices([Bind("ProductionVsLaborCostPriceId,ElectroplatingItemCost,EmbroideryItemCost,EngravingItemCost,MetalItemCost,UVItemCost")] ProductionVsLaborCostPrice productionVsLaborCostPrice)
    {
        if (ModelState.IsValid)
        {
            productionVsLaborCostPrice.ModifyByUser = User.Identity.Name;
            productionVsLaborCostPrice.ModifyDate = _now;
            await _productionVsLaborCostPriceService.AddAsync(productionVsLaborCostPrice);
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    //AUDITING PAGE
    public async Task SetupDepartmentalBreakdown()
    {
        try
        {
            JObject data = await _employeeService.GetEmployeeErrors();

            List<string> departments = ((JArray)data["departments"]).ToObject<List<string>>();
            List<string> employeeReferences = ((JArray)data["employeeReferences"]).ToObject<List<string>>();
            List<int> errorCounts = ((JArray)data["errorCounts"]).ToObject<List<int>>();

            ViewData["EmployeeErrorDepartments"] = string.Join(",", departments.Cast<string>().Distinct().ToArray());
            ViewData["EmployeeErrorReferenceIds"] = string.Join(",", employeeReferences.Cast<string>().ToArray());
            ViewData["ErrorCounts"] = string.Join(",", errorCounts.Cast<int>().ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public JsonResult SetupPieCharts()
    {
        try
        {
            JObject data = _shipStationStoreService.GetShipStationStorePieChartsData();

            List<int> electroplatingPlot = ((JArray)data["electroplatingPlot"]).ToObject<List<int>>();
            List<String> electroplatingSkus = ((JArray)data["electroplatingSkus"]).ToObject<List<string>>();
            List<int> embroideryPlot = ((JArray)data["embroideryPlot"]).ToObject<List<int>>();
            List<String> embroiderySkus = ((JArray)data["embroiderySkus"]).ToObject<List<String>>();
            List<int> engravingPlot = ((JArray)data["engravingPlot"]).ToObject<List<int>>();
            List<String> engravingSkus = ((JArray)data["engravingSkus"]).ToObject<List<String>>();
            List<int> metalPlot = ((JArray)data["metalPlot"]).ToObject<List<int>>();
            List<String> metalSkus = ((JArray)data["metalSkus"]).ToObject<List<String>>();
            List<int> uvpPlot = ((JArray)data["uvpPlot"]).ToObject<List<int>>();
            List<String> uvpSkus = ((JArray)data["uvpSkus"]).ToObject<List<String>>();
            List<int> unknownPlot = ((JArray)data["unknownPlot"]).ToObject<List<int>>();
            List<String> unknownSkus = ((JArray)data["unknownSkus"]).ToObject<List<String>>();

            return Json(new
            {
                electroplatingCountArray = electroplatingPlot.Take(10),
                electroplatingSKUArray = electroplatingSkus.Take(10),

                embroideryCountArray = embroideryPlot.Take(10),
                embroiderySKUArray = embroiderySkus.Take(10),

                engravingCountArray = engravingPlot.Take(10),
                engravingSKUarray = engravingSkus.Take(10),

                metalCountArray = metalPlot.Take(10),
                metalSKUArray = metalSkus.Take(10),

                uvpCountArray = uvpPlot.Take(10),
                uvpSKUArray = uvpSkus.Take(10),

                unknownCountArray = unknownPlot.Take(10),
                unknownSKUArray = unknownSkus.Take(10)
            });
        }
        catch
        {
            //This should only be reached on new database creation since the table "_ShipstationOrders" does not exist within the project
            return Json(new
            {
                electroplatingCountArray = 0,
                electroplatingSkuArray = 0,

                embroideryCountArray = 0,
                embroiderySKUArray = 0,

                engravingCountArray = 0,
                engravingSKUarray = 0,

                metalCountArray = 0,
                metalSKUArray = 0,

                uvpCountArray = 0,
                uvpSKUArray = 0,

                unknownCountArray = 0,
                unknownSKUArray = 0
            });
        }
    }
}
