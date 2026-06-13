using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Enum;
using ERPWebApp.Extensions;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace ERPWebApp.Controllers
{
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.SellerBasic)]
    public class ReportController : Controller
    {
        private readonly IProductService _productService;
        private readonly IStocksService _stocksService;
        private readonly IOrderShippingService _orderShippingService;
        private readonly IInventoryBalanceService _inventoryBalanceService;
        private readonly IOrderService _orderService;
        private readonly ISiteService _siteService;
        private readonly ICycleCountService _cycleCountService;
        private readonly ILocationService _locationService;
        private readonly ISubCategoryService _subCategoryService;
        private readonly IDepartmentService _departmentService;
        private readonly IFinancialsService _financialsService;
        private readonly IShipStationStoreService _shipStationStoreService;
        public ReportController(
            IProductService productService,
            IOrderShippingService orderShippingService,
            IOrderService orderService,
            ISiteService siteService,
            IStocksService stocksService,
            ICycleCountService cycleCountService,
            IInventoryBalanceService inventoryBalanceService,
            ILocationService locationService,
            ISubCategoryService subCategoryService,
            IDepartmentService departmentService,
            IFinancialsService financialsService,
            IShipStationStoreService shipStationStoreService)
        {
            _orderService = orderService;
            _productService = productService;
            _orderShippingService = orderShippingService;
            _siteService = siteService;
            _stocksService = stocksService;
            _cycleCountService = cycleCountService;
            _inventoryBalanceService = inventoryBalanceService;
            _locationService = locationService;
            _subCategoryService = subCategoryService;
            _departmentService = departmentService;
            _financialsService = financialsService;
            _shipStationStoreService = shipStationStoreService;
        }
        public IActionResult Index()
        {
            var products = _productService.GetList(
                (IQueryable<Product> p) => p
                    .Where(x=>x.IsActive)
                    .Select(
                    x => new SelectListItem
                    {
                        Value = x.ProductId.ToString(),
                        Text = x.Sku
                    })
            );

            ViewData["SkuData"] = new SelectList(products, "Value", "Text");

            var query = (IQueryable<Site> site) => site
                    .Where(x => x.IsActive)
                    .Select(x => new SelectListItem { Value = x.SiteId.ToString(), Text = x.SiteName });

            var sites = _siteService.GetList(query);

            ViewData["siteData"] = new SelectList(sites, "Value", "Text");

            var locationQuery = (IQueryable<Location> l) => l
                    .Where(x => x.IsActive)
                    .Select(x => new SelectListItem { Value = x.LocationId.ToString(), Text = x.LocationName });

            var location = _locationService.GetList(locationQuery);

            ViewData["locationList"] = new SelectList(location, "Value", "Text");

            var subcategories = _subCategoryService.GetList((IQueryable<SubCategory> s) => s
                    .Where(x => x.IsActive)
                    .Select(x => new SelectListItem { Value = x.SubCategoryId.ToString(), Text = x.Description })) ;

            ViewData["SubCategoryList"] = new SelectList(subcategories, "Value", "Text");

            var departments = _departmentService.GetList((IQueryable<Department> d) => d
                    .Where(x => x.IsActive)
                    .Select(x => new SelectListItem { Value = x.DepartmentId.ToString(), Text = x.DepartmentName }));

            ViewData["DepartmentList"] = new SelectList(departments, "Value", "Text");

            var reportTypesList = ReportTypes.GetOnHandBySites.ToList("Select Query");

            ViewData["QueryList"] = reportTypesList;

            var shipStationStores = _shipStationStoreService.GetList((IQueryable<ShipStationStore> s) => s
                   .Where(x => x.IsActive)
                   .Select(x => new SelectListItem { Value = x.ShipStationStoreId.ToString(), Text = x.StoreName }));

            ViewData["ShipStationStoreList"] = new SelectList(shipStationStores, "Value", "Text");

            return View();
        }

        [HttpGet("GetQueries")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetQueries(int Query, DateTime StartDate, DateTime EndDate, int? ProductId, int? SiteId, int? locationId)
        {
            List<Report> _report = new List<Report>();

            try
            {
                switch ((ReportTypes)Query)
                {
                    case ReportTypes.AverageShipmentCostBySkuAndDateRange:
                        _report = _orderShippingService.GetAvgShippingCostInDateRangeBySku(StartDate, EndDate);
                        break;

                    case ReportTypes.AverageShipmentCostByServiceAndDateRange:
                        _report = _orderShippingService.GetAvgShippingCostInDateRangeByService(StartDate, EndDate);
                        break;

                    case ReportTypes.AllItemAmountsShippedByDateRange:
                        _report = _orderShippingService.GetAmountItemsShippedByDateRange(StartDate, EndDate);
                        break;

                    case ReportTypes.AmountAnItemHasBeenShippedByDateRange:
                        _report = _orderShippingService.GetAmountShippedByDateRangeSkuFilter(ProductId, StartDate, EndDate);
                        break;

                    case ReportTypes.GetSumOfOrderSalesByDateRange:
                        return GetSumOfOrderSalesByDateRange(StartDate, EndDate);

                    case ReportTypes.GetOnHandBySites:
                        _report = SiteId.HasValue ? _stocksService.GetOnHandBySiteFilter(SiteId.Value) : new List<Report>();
                        break;

                    case ReportTypes.InventoryBalance:
                        _report = _inventoryBalanceService.GetReport(ProductId ?? 0);
                        break;

                    case ReportTypes.StockHistoryReport:
                        _report = _stocksService.StockHistoryReport_Old(locationId ?? 0, StartDate);
                        break;

                    case ReportTypes.ProductStockReport:
                        _report = _stocksService.GetStockHistoryReport(SiteId ?? 0, StartDate);
                        break;

                    case ReportTypes.CycleCountReport:
                        _report = _cycleCountService.GetCycleCountReport(StartDate, EndDate, locationId ?? 0);
                        break;

                    case ReportTypes.WeeklyGrossProfit:
                        var report = await _financialsService.GetWeeklyProfits(StartDate, EndDate);
                        return Ok(report);

                    default:
                        return Ok();
                }

                return Ok(new { recordsTotal = _report.Count, data = _report.ToList() });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An issue has arisen: " + ex.Message);
                return RedirectToAction("Index",new List<Report>());
            }
        }

        public IActionResult GetSumOfOrderSalesByDateRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var orderItems = _orderService.GetOrderItemsByDate(fromDate, toDate);
                var OrderItemsSum = _orderService.GetOrderItemsSumByDate(fromDate, toDate);

                Dictionary<string, Dictionary<string, string>> sheetColumns = new(){
                    {
                        "Sheet 1",
                        new Dictionary<string, string>{
                            {"Shipped Date", "DATE"},
                            {"Sum of shipped items", "NUMBER"},
                            {"Sum of shipped order totals", "DECIMAL"},
                            {"Sum of fulfillment from items", "DECIMAL"},
                            {"Sum of cost from items", "DECIMAL"},
                            {"Sum of labor from items", "DECIMAL"}
                        }
                    },
                    {
                        "Sheet 2",
                        new Dictionary<string, string>{
                            {"Shipped Date", "DATE"},
                            {"Order number", ""},
                            {"Product Sku", ""},
                            {"Order item sale cost", "DECIMAL"},
                            {"Product fulfillment", "DECIMAL"},
                            {"Product cost", "DECIMAL"},
                            {"Product labor cost", "DECIMAL"}
                        }
                    }
                };

                Dictionary<string, List<Dictionary<string, string>>> sheetData = new(){
                    {"Sheet 1", OrderItemsSum},
                    {"Sheet 2", orderItems}
                };


                // Prepare the workbook using the ExcelFileExtensions
                using (MemoryStream ms = new MemoryStream())
                {
                    // Open the SpreadsheetDocument in-memory and write to the MemoryStream
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
                    {
                        ExcelFileExtensions.PrepareWorkbook(sheetColumns, sheetData, document);
                    }

                    // Seek to the beginning of the stream before returning
                    ms.Seek(0, SeekOrigin.Begin);

                    // Return the file as a download response
                    return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"SumOfOrderSales_From_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.xlsx"
                    };
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        [HttpPost("GetStockHistoryReport")]
        [IgnoreAntiforgeryToken]
        public IActionResult GetStockHistoryReport(int Query, DateTime StartDate, DateTime EndDate, int? ProductId, int? SubCategoryId, int? DepartmentId, int? ShipStationStoreId, int? PageNo)
        {

            try
            {
                // gets form data for serverside processing
                var draw = Request.Form["draw"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form[
                    "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
                ].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int? pageSize = null;
                if (!string.IsNullOrEmpty(length) && length != "-1")
                {
                    pageSize = Convert.ToInt32(length);
                }

                //validate product id
                if (ProductId < 0) { ProductId = 0; }

                var _report = _stocksService.GetStockHistoryReport(StartDate, EndDate, ProductId ?? 0, SubCategoryId ?? 0, DepartmentId ?? 0, ShipStationStoreId ?? 0, PageNo ?? 1, pageSize ?? 100);


                return Ok(new
                {
                    draw = draw,
                    recordsFiltered = _report.TotalRecords,
                    recordsTotal = _report.TotalRecords,
                    data = _report.ReportItemsList
                });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("An issue has arisen:" + ex.Message);
                return BadRequest(ex.Message);
            }

        }


        public IActionResult GetProductsBy(int SubCategoryId = 0, int DepartmentId = 0)
        {
            try
            {
                var products = _productService.GetList((IQueryable<Product> s) => s
                    .Where(x => x.IsActive && (SubCategoryId == 0 || x.SubCategoryId == SubCategoryId) && (DepartmentId == 0 || x.Departments.Any(d => d.DepartmentId == DepartmentId)))
                    .Select(x => new { id = x.ProductId, text = x.Sku + " | " + x.Description }));

                return Ok(products);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("An issue has arisen:" + ex.Message);
                return BadRequest(ex.Message);
            }
        }

        public async Task<JsonResult> SearchProducts(string searchTerm)
        {
            var products = await _productService.GetListAsync(
                (IQueryable<Product> q) => q
                    .Where(x => x.IsActive && (
                          x.Sku.Contains(searchTerm)
                          || x.Description.Contains(searchTerm)
                    ))
                    .OrderBy(x => x.Sku)
                    .Take(10)
                    .Select(x => new
                    {
                        text = $"{x.Sku} : {x.Description}",
                        id = x.ProductId
                    })
            );

            return new JsonResult(products);
        }

        public IActionResult PowerBIReports(int? Query)
        {
            return View(Query);
        }

        [HttpGet("GetYearlyShippedProductReport")]
        public async Task<IActionResult> GetYearlyShippedProductReport()
        {
                var results = await _orderService.GetYearlyProductCountReport();
                return Ok(results);
        }
    }
}
