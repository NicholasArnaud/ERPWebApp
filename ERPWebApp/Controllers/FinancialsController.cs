using ERPWebApp.Models;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using ERPWebApp.Models.Company;
using System.Security.Claims;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Controllers
{
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.Developer)]
    [CwaFeatureGate(CwaFeatures.ORDER)]
    [AutoValidateAntiforgeryToken]
    public class FinancialsController : Controller
    {
        private FinancialsViewModel _Financials;
        private readonly IFinancialsService _financialsService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserPreferencesService _userPreferencesService;
        private readonly IMyDashService _myDashService;

        public FinancialsController(
            IFinancialsService financialsService, IDepartmentService departmentService,
            IUserPreferencesService userPreferencesService,
             IMyDashService myDashService)
        {
            _financialsService = financialsService;
            _Financials = new FinancialsViewModel();
            _departmentService = departmentService;
            _userPreferencesService = userPreferencesService;
            _myDashService = myDashService;
        }

        public async Task<IActionResult> IndexAsync(int daysProductSales = -60)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (currentUserID is not null)
            {
                _Financials.DashboardLayouts = await _userPreferencesService.GetDashboardLayoutByDashboardAsync(currentUserID, DashboardNames.DashboardFinancials.ToString());

                var result = await _myDashService.GetUserDashboadData(currentUserID);
                if (result != null) {
                    _Financials.YearlyProfit = result.YearlyProfit;
                    _Financials.HistoricalTrends = result.HistoricalTrends;
                    _Financials.TotalFulfillmentSales = result.TotalFulfillmentSales;
                    _Financials.TopProductSales = result.TopProductSales;
                } 
            }

            var pieChartData = await TotalFulfillmentSales();
            _Financials.PieChartDataJson = JsonConvert.SerializeObject(pieChartData);

            var productSalesData = await ProductSales(daysProductSales);
            _Financials.ProductSalesDataJson = JsonConvert.SerializeObject(productSalesData);
            _Financials.ProductSalesInfo = productSalesData;

            DateTime endDateThisYear = DateTime.Today;
            DateTime startDateThisYear = new DateTime(endDateThisYear.Year, 1, 1);

            DateTime endDateLastYear = endDateThisYear.AddYears(-1);
            DateTime startDateLastYear = new DateTime(endDateLastYear.Year, 1, 1);

            var yearlyProfitsThisYear = await _financialsService.GetYearlyProfitsData(startDateThisYear, endDateThisYear);
            var yearlyProfitsLastYear = await _financialsService.GetYearlyProfitsData(startDateLastYear, endDateLastYear);
 
            _Financials.CurrentYearProfitsDataJson = JsonConvert.SerializeObject(yearlyProfitsThisYear);
            _Financials.LastYearProfitsDataJson = JsonConvert.SerializeObject(yearlyProfitsLastYear);

            return View(_Financials);
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

        public async Task<List<TrendsInfoDto>> HistoricalTrends(DateTime startDate, DateTime endDate)
        {
            var financialInformation = await _financialsService.TrendsTable(startDate, endDate);
            var trendsData = JsonConvert.SerializeObject(financialInformation);
            ViewBag.DaysTrends = (endDate - startDate).Days + 1;
            ViewBag.Trends = trendsData;
            return financialInformation;
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

        public async Task<IActionResult> ProductSalesView(int days)
        {
            var productSalesInfo = await _financialsService.ProductSalesTable(days);
            return PartialView("ProductSalesTablePartial", productSalesInfo);
        }

        public async Task<IActionResult> ExportFulfillmentToCSV()
        {
            var data = await TotalFulfillmentSales();
            var csv = GenerateFulfillmentCSV(data);

            var content = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var contentType = "text/csv";
            var fileName = "fulfillment_export.csv";

            return File(content, contentType, fileName);
        }

        private string GenerateFulfillmentCSV(IEnumerable<FulfillmentInfoDto> data)
        {
            var csv = new StringBuilder();

            var storeNames = data.SelectMany(item => item.StoreFulfillmentCost.Keys)
                                 .Distinct()
                                 .ToList();

            csv.AppendLine("Department Name," + string.Join(",", storeNames) + ",Total Department Sales,Total Cost"); 

            foreach (var item in data)
            {
                var departmentName = item.DepartmentName;
                var row = new List<string> { departmentName };
                decimal totalDepartmentCost = 0;

                foreach (var storeName in storeNames)
                {
                    var storeCost = item.StoreFulfillmentCost.TryGetValue(storeName, out decimal cost) ? cost : 0;
                    row.Add("$" + storeCost);
                    totalDepartmentCost += storeCost;
                }

                row.Add("$" + totalDepartmentCost);
                row.Add("$" + item.ProductCost);
                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString();
        }

        public async Task<IActionResult> ExportTrendsToCSV(DateTime startDate, DateTime endDate)
        {
            var data = await HistoricalTrends(startDate, endDate);
            var csv = GenerateTrendsCSV(data);

            var content = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var contentType = "text/csv";
            var fileName = "historical_trends_export.csv";

            return File(content, contentType, fileName);
        }

        private string GenerateTrendsCSV(IEnumerable<dynamic> data)
        {
            var csv = new StringBuilder();
            var departments = new List<string>();

            foreach (var item in data)
            {
                foreach (var key in ((IDictionary<string, int>)item.departmentsOrders).Keys)
                {
                    if (!departments.Contains(key))
                    {
                        departments.Add(key);
                    }
                }
            }

            csv.AppendLine("Date," + string.Join(",", departments) + ",Total Fulfillment Cost,Total Cost");

            foreach (var item in data)
            {
                var date = (DateTime)item.date;
                var formattedDate = $"{date.Month}/{date.Day}/{date.Year}";
                var row = new List<string> { formattedDate };
                decimal totalFulfillmentCost = 0;
                decimal totalCost = 0;

                foreach (var department in departments)
                {
                    var fulfillmentCost = ((IDictionary<string, decimal>)item.departmentsFulfillmentCost).TryGetValue(department, out decimal departmentCost) ? departmentCost : 0;
                    row.Add("$" + fulfillmentCost.ToString());
                    totalFulfillmentCost += fulfillmentCost;

                    var cost = ((IDictionary<string, decimal>)item.departmentsProductCost).TryGetValue(department, out decimal pc) ? pc : 0;
                    totalCost += cost;
                }

                row.Add("$" + totalFulfillmentCost.ToString());
                row.Add("$" + totalCost.ToString());
                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString();
        }

        public IActionResult OnGetDepartment()
        {
            var departmentsColors = _departmentService.GetList(
                    (IQueryable<Department>p)=> p.GroupBy(x=>x.DepartmentName)
                        .Select(x=> new Department(){
                           DepartmentName = x.Key,
                            DepartmentColor = x.First().DepartmentColor
                        })
            );
            return Json(departmentsColors);
        }

        [HttpGet]
        public async Task<IActionResult> GetYearlyProfitsData(DateTime startDate, DateTime endDate)
        {
            DateTime endDateThisYear = endDate.AddDays(1);
            DateTime startDateThisYear = startDate;

            DateTime endDateLastYear = new DateTime(endDateThisYear.Year - 1, endDateThisYear.Month, endDateThisYear.Day - 1);
            DateTime startDateLastYear = new DateTime(startDateThisYear.Year - 1, startDateThisYear.Month, startDateThisYear.Day);

            var yearlyProfitsThisYear = await _financialsService.GetYearlyProfitsData(startDateThisYear, endDateThisYear);
            var yearlyProfitsLastYear = await _financialsService.GetYearlyProfitsData(startDateLastYear, endDateLastYear);

            var result = new
            {
                currentYearProfitsData = yearlyProfitsThisYear,
                lastYearProfitsData = yearlyProfitsLastYear
            };

            return Json(result);
        }
    }
}
