using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class MyDashController : Controller
{
    private readonly IMyDashService _myDashService;
    private InventoryViewModel _Inventory;
    private MyDashViewModel _MyDash;
    private readonly IInventoryService _inventoryService;
    private readonly IStocksService _stocksService;
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly IFinancialsService _financialsService;
    private readonly IDepartmentService _departmentService;
    private FinancialsViewModel _Financials;
    private readonly ISpeedOMeterGoalService _speedOMeterGoalService;
    private readonly IEmployeeService _employeeService;


    public MyDashController(IMyDashService myDashService,
        IInventoryService inventoryService,
        IStocksService stocksService,
        IUserPreferencesService userPreferencesService,
        IFinancialsService financialsService,
        IDepartmentService departmentService,
        ISpeedOMeterGoalService speedOMeterGoalService,
        IEmployeeService employeeService)
    {
        _myDashService = myDashService;
        _inventoryService = inventoryService;
        _stocksService = stocksService;
        _userPreferencesService = userPreferencesService;
        _Inventory = new InventoryViewModel();
        _financialsService = financialsService;
        _Financials = new FinancialsViewModel();
        _departmentService = departmentService;
        _MyDash = new MyDashViewModel();
        _speedOMeterGoalService = speedOMeterGoalService;
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _MyDash.DashboardLayouts = await _userPreferencesService.GetDashboardLayoutByDashboardAsync(currentUserID, DashboardNames.DashboardMyDash.ToString());
            _MyDash.MyDashData = await _myDashService.GetUserDashboadData(currentUserID);

            var pieChartData = await TotalFulfillmentSales();
            _MyDash.Financials.PieChartDataJson = JsonConvert.SerializeObject(pieChartData);
            int daysProductSales = -60;
            var productSalesData = await ProductSales(daysProductSales);
            _MyDash.Financials.ProductSalesDataJson = JsonConvert.SerializeObject(productSalesData);
            _MyDash.Financials.ProductSalesInfo = productSalesData;

            DateTime endDateThisYear = DateTime.Today;
            DateTime startDateThisYear = new DateTime(endDateThisYear.Year, 1, 1);

            DateTime endDateLastYear = endDateThisYear.AddYears(-1);
            DateTime startDateLastYear = new DateTime(endDateLastYear.Year, 1, 1);

            var yearlyProfitsThisYear = await _financialsService.GetYearlyProfitsData(startDateThisYear, endDateThisYear);
            var yearlyProfitsLastYear = await _financialsService.GetYearlyProfitsData(startDateLastYear, endDateLastYear);

            _MyDash.Financials.CurrentYearProfitsDataJson = JsonConvert.SerializeObject(yearlyProfitsThisYear);
            _MyDash.Financials.LastYearProfitsDataJson = JsonConvert.SerializeObject(yearlyProfitsLastYear);

            //home dashboard 
            _MyDash.Home.SpeedOMeterGoal = await _speedOMeterGoalService.GetLastSpeedOMeterGoalAsync();
            _MyDash.Home.ProductionVsLaborCostPrice = await _employeeService.GetLastProductionVsLaborCostPrice();
            _MyDash.Home.Department = await _departmentService.GetListAsync(
                x => x.IsProduction == true,
                orderSelectors: new Expression<Func<Department, string>>[]{
            x=> x.DepartmentName
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        ViewData["Department"] = new SelectList(_MyDash.Home.Department, "DepartmentId", "DepartmentName");

        return View(_MyDash);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFavouriteStatus(string propertyName, bool value)
    {

        var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var result = await _myDashService.UpdateFavouriteStatus(propertyName, value, currentUserID);

        return View();
    }

    [HttpGet]
    
    public async Task<List<FulfillmentInfoDto>> TotalFulfillmentSales()
    {
        var financialInformationPie = await _financialsService.FulfillmentTable();
        Dictionary<string, decimal> departmentCostsPie = new Dictionary<string, decimal>();

        foreach (var financial in financialInformationPie)
        {
            if (financial.DepartmentName != null && financial.ProductProfit.HasValue)
            {
                if (departmentCostsPie.ContainsKey(financial.DepartmentName))
                {
                    departmentCostsPie[financial.DepartmentName] += financial.ProductProfit.Value;
                }
                else
                {
                    departmentCostsPie[financial.DepartmentName] = financial.ProductProfit.Value;
                }
            }
        }

        return financialInformationPie;
    }
    
    public async Task<List<ProductSalesInfoDto>> ProductSales(int days)
    {
        var financialInformation = await _financialsService.ProductSalesTable(days);

        var jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
        };

        return financialInformation;
    }
}
