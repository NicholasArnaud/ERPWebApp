using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using ERPWebApp.Models.Common;
using ERPWebApp.Middleware;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class CycleCountController : Controller
{
    private readonly ICycleCountFrequencyService _cycleCoutFrequencyService;
    private readonly ISiteService _siteService;
    private readonly IStocksService _stockService;
    private readonly IProductService _productService;
    private readonly ICycleCountService _cycleCountService;
    private readonly IEmployeeService _employeeService;
    private readonly CycleCount _cycleCount = new CycleCount();
    private readonly ITriggerEmailAlertService _triggerEmailAlertService;

    public CycleCountController(
        ICycleCountFrequencyService cycleCoutFrequencyService,
        ISiteService siteService,
        IStocksService stockService,
        IProductService productService,
        ICycleCountService cycleCountService,
        IEmployeeService employeeService,
        ITriggerEmailAlertService triggerEmailAlertService
    )
    {
        _cycleCoutFrequencyService = cycleCoutFrequencyService;
        _siteService = siteService;
        _stockService = stockService;
        _productService = productService;
        _cycleCountService = cycleCountService;
        _employeeService = employeeService;
        _triggerEmailAlertService = triggerEmailAlertService;
    }
    public async Task<IActionResult> Index()
    {
        await _cycleCoutFrequencyService.GenerateFrequenciesAsync();
        var SiteFilter = (await _siteService.GetListAsync(x => x.IsActive)).Select(x => new { x.SiteId, x.SiteName });
        ViewData["SiteFilter"] = new SelectList(SiteFilter, "SiteId", "SiteName");
        return View();
    }

    [HttpGet("GetCycleCountList")]
    public async Task<IActionResult> GetCycleCountList(int siteId, bool? isStarted)
    {

        var draw = Request.Query["draw"].FirstOrDefault();
        var sortColumn = Request.Query[
                "columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][data]"
            ].FirstOrDefault();
        var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";
        var searchValue = Request.Query["search[value]"].FirstOrDefault();
        var start = Request.Query["start"].FirstOrDefault();
        var length = Request.Query["length"].FirstOrDefault();

        var search = new SearchParameters
        {
            Start = string.IsNullOrEmpty(start) ? 0 : int.Parse(start),
            PageSize = string.IsNullOrEmpty(length) ? 10 : int.Parse(length),
            SortBy = sortColumn,
            SearchValue = searchValue,
            IsDescending = sortColumnDirection != "asc",
            SearchColumns = ["Products.Sku", "Products.Description", "Location.LocationName"]
        };

        var (data, count) = await _cycleCountService.GetStockToCountAsync(siteId, search, isStarted);

        return Ok(new
        {
            draw,
            recordsTotal = count,
            recordsFiltered = count,
            data
        });
    }

    public async Task<IActionResult> PopulateFrequency(int SiteId)
    {
        var freq = await _cycleCoutFrequencyService.GetLatestFrequencyAsync(SiteId);
        var baseDays = freq.BaseDays;
        var thousand = freq.Over1000;
        var cost10 = freq.Cost10;

        return Json(new { basefreq = baseDays, thousandfreq = thousand, cost10freq = cost10 });
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.InventoryManager)]
    public async Task<IActionResult> StartCount(int id)
    {
        await _cycleCountService.StartCycleCountAsync(id);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.InventoryManager)]
    public async Task<IActionResult> StartBulk([FromBody] List<int> ids)
    {
        await _cycleCountService.StartCycleCountAsync(ids);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.InventoryManager)]
    public async Task<IActionResult> StartCountBySite(int siteId)
    {
        await _cycleCountService.StartCycleCountForSiteAsync(siteId);
        return Ok();
    }

    // GET: CycleCount/Edit/5
    [HttpGet]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.InventoryManager)]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }


        //send the page the list
        var cycleCount = _cycleCountService.Get(x => x.StockId == id, includes: new Expression<Func<CycleCount, object>>[]
        {
            x => x.Stock.Products,
            x => x.Stock.Products.Departments,
            x => x.Stock.Location,
            x => x.Stock.Location.Sites
        });
        if (cycleCount == null)
        {
            return NotFound();
        }
        ViewBag.BeforeCount = cycleCount.Stock.TotalAvailable;
        ViewData["EnteredById"] = new SelectList(_employeeService.GetAll(), "EmployeeId", "FullName");
        return PartialView(cycleCount);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit([Bind("CycleCountId, EnteredSku, StockId, EnteredQuantity, EnteredById")] CycleCount cycleCount)
    {
        int previousQty = 0;
        try
        {

            previousQty = await _stockService.GetAsync(
                                      query: stocks => stocks
                                      .Where(stock => stock.StockId == cycleCount.StockId)
                                      .Select(stock => stock.TotalAvailable));
            //mark the stock as counted by today
            var stock = await _cycleCountService.EditCycleCount(cycleCount, this.User.Identity.Name);
            var sites = (await _siteService.GetListAsync(x => x.SiteId != stock.Location.SiteId))?.Select(x => new { SiteId = x.SiteId, SiteName = x.SiteName });

            List<SelectListItem> vList = new SelectList(sites, "SiteId", "SiteName").ToList();

            vList.Insert(0, (new SelectListItem { Text = stock.Location.Sites.SiteName, Value = stock.Location.SiteId.ToString() }));

            if (previousQty != cycleCount.EnteredQuantity)
            {
                var cycleCountEmailData = new CycleCountFinishedEmailAlertDTO
                {
                    Location = stock.Location.LocationName,
                    Sku = stock.Products.Sku,
                    PreviousQuantity = previousQty,
                    NewQuantity = cycleCount.EnteredQuantity,
                };

                await Task.Run(() => _triggerEmailAlertService.SendFinishedCycleCountAlerts(cycleCountEmailData));
            }

            ViewData["SiteFilter"] = vList;

        }
        catch
        {
            throw;
        }

        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.InventoryManager)]
    public async Task<IActionResult> ModifyCycle(int BaseDays, int Over100, int Cost10, int SiteId)
    {
        var modifyCycle = new CycleCountFrequency();

        var IsExists = _siteService.IsExists(x => x.SiteId == SiteId && x.IsActive);
        if (!IsExists) return BadRequest();

        modifyCycle.SiteId = SiteId;
        modifyCycle.BaseDays = BaseDays;
        modifyCycle.Over1000 = Over100;
        modifyCycle.Cost10 = Cost10;
        modifyCycle.ModifyDate = DateTime.Now;
        modifyCycle.ModifyByUser = this.User.Identity.Name;

        await _cycleCoutFrequencyService.AddAsync(modifyCycle);
        return Ok();
    }

}
