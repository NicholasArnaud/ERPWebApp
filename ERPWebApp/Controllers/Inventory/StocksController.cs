using BarcodeStandard;
using ERPWebApp.Extensions;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using Location = ERPWebApp.Models.Inventory.Location;

namespace ERPWebApp.Controllers.Inventory;

/// <summary>
///
/// </summary>
[Authorize(
    Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.InventoryBasic
            + ","
            + RoleList.ShippingBasic
            + ","
            + RoleList.ExternalUser
            + ","
            + RoleList.SellerBasic
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class StocksController : Controller
{
    private readonly IStocksService _stocksService;
    private readonly IProductService _productService;
    private readonly ISiteService _siteService;
    private readonly ISubCategoryService _subCategoryService;
    private readonly IDepartmentService _departmentService;
    private readonly ILocationService _locationService;
    private readonly IProductTagService _productTagService;
    private readonly IVendorService _vendorService;
    private readonly IShipStationStoreService _shipStationStoreService;
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    ///
    /// </summary>
    /// <param name="stocksService"></param>
    /// <param name="productService"></param>
    /// <param name="siteService"></param>
    /// <param name="departmentService"></param>
    /// <param name="subCategoryService"></param>
    /// <param name="locationService"></param>
    /// <param name="productTagService"></param>
    /// <param name="vendorService"></param>
    /// <param name="shipstationStoreService"></param>
    public StocksController(
        IStocksService stocksService,
        IProductService productService,
        ISiteService siteService,
        IDepartmentService departmentService,
        ISubCategoryService subCategoryService,
        ILocationService locationService,
        IProductTagService productTagService,
        IVendorService vendorService,
        IShipStationStoreService shipstationStoreService,
        UserManager<IdentityUser> userManager)
    {
        _locationService = locationService;
        _subCategoryService = subCategoryService;
        _departmentService = departmentService;
        _siteService = siteService;
        _productService = productService;
        _stocksService = stocksService;
        _productTagService = productTagService;
        _vendorService = vendorService;
        _shipStationStoreService = shipstationStoreService;
        _userManager = userManager;
    }

    /// <summary>
    ///
    /// </summary>
    public void OnGet()
    {
        _stocksService.GetList(
            s => s.Products.IsUv,
            orderSelectors:
            [s => s.Location.Sites.SiteName]
        );
        _productService.GetAll(
            orderSelectors:
            [s => s.Sku]
        );
    }

    // GET: Stocks
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        // show filtered ViewData based on internal/external user role, show both for admin/manager role
        var allSites = _siteService.GetAll();
        var allDepartments = _departmentService.GetAll();
        var allSubCategories = _subCategoryService.GetAll();
        var allProductTags = _productTagService.GetAll();
        var allVendors = _vendorService.GetList(v => v.IsActive);

        //since this is common in all scenarios, adding product tags as a common data set
        ViewData["ProductTagList"] = new SelectList(
            allProductTags,
            "TagId",
            "Description"
        );
        //add all available vendors list
        ViewData["VendorsList"] = new SelectList(
            allVendors,
            "VendorId",
            "VendorName"
        );

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            // show all
            ViewData["SiteName"] = new SelectList(allSites, "SiteId", "SiteName");
            ViewData["DepartmentName"] = new SelectList(
                allDepartments,
                "DepartmentId",
                "DepartmentName"
            );
            ViewData["SubCategoryList"] = new SelectList(
                allSubCategories,
                "SubCategoryId",
                "Description"
            );

            return View();
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            // Get the store ID for the external user  
            var storeId = await GetStoreIdForUserAsync();

            if (storeId != null)
            {
                // Trying to find stocks associated with the store  
                var stocks = await _stocksService.GetListAsync(stock => stock.ShipStationStoreId == storeId);

                // Grabbing locations & IDs associated with these stocks  
                var locationIds = stocks.Select(stock => stock.LocationId).Distinct();
                var locations = await _locationService.GetListAsync(location => locationIds.Contains(location.LocationId));

                // Filter sites based on these locations  
                var externalUserSites = allSites.Where(s => locations.Any(l => l.SiteId == s.SiteId));
                //var externalDept = allDepartments.Where(d => d.DepartmentName.Equals("External"));

                ViewData["SiteName"] = new SelectList(externalUserSites, "SiteId", "SiteName");
                ViewData["DepartmentName"] = new SelectList(
                    allDepartments,
                    "DepartmentId",
                    "DepartmentName"
                );
                ViewData["SubCategoryList"] = new SelectList(
                    allSubCategories,
                    "SubCategoryId",
                    "Description"
                );
                return View();
            }
            else
            {
                return View(); ;
            }
        }

        // else implied -> if neither admin/manager, nor external, then you are internal user, so show only internal
        var internalSites = allSites.Where(s => !s.IsExternal);
        var internalDepts = allDepartments.Where(d => !d.DepartmentName.Equals("External"));
        // currently don't filter subcategories as internal/external -> should we?

        ViewData["SiteName"] = new SelectList(internalSites, "SiteId", "SiteName");
        ViewData["DepartmentName"] = new SelectList(
            internalDepts,
            "DepartmentId",
            "DepartmentName"
        );
        ViewData["SubCategoryList"] = new SelectList(
            allSubCategories,
            "SubCategoryId",
            "Description"
        );

        return View();
    }

    private async Task<int?> GetStoreIdForUserAsync()
    {
        // Fetching the user's email using UserManager  
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        var userEmail = user?.Email;

        if (userEmail == null)
        {
            return null;
        }

        // Grabbing the store with the matching Id.
        var store = await _shipStationStoreService.GetAsync(s => s.Email == userEmail || s.PublicEmail == userEmail);

        return store?.ShipStationStoreId;
    }

    /// <summary>
    /// gets a list of products for the stock page, the user can filter the products and click on the products for more information
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetProducts")]
    public async Task<IActionResult> GetProducts(
        int? subCategoryId,
        int? siteId,
        int? departmentId,
        int? productTagId,
        int? vendorId,
        bool? zeroQtyStock
    )
    {
        var draw = Request.Query["draw"].FirstOrDefault();
        var start = Request.Query["start"].FirstOrDefault();
        var length = Request.Query["length"].FirstOrDefault();
        var sortColumn = Request.Query[
            "columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Query["search[value]"].FirstOrDefault();

        int pageSize = 0;

        int? storeId = null;
        if (User.IsInRole(RoleList.ExternalUser))
        {
            storeId = await GetStoreIdForUserAsync();
        }

        var applicationDbContextProduct = _stocksService.GetProducts(
            searchValue,
            zeroQtyStock,
            subCategoryId,
            siteId,
            departmentId,
            productTagId,
            vendorId,
            sortColumn,
            sortColumnDirection,
            storeId
        );

        if (length != null)
        {
            pageSize = length == "-1" ? applicationDbContextProduct.Count() : Convert.ToInt32(length);
        }

        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        int recordsTotal = applicationDbContextProduct.Count();

        applicationDbContextProduct = applicationDbContextProduct.Skip(skip).Take(pageSize);

        var data = applicationDbContextProduct.ToList();


        if (data != null)
        {
            return Ok(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            });
        }
        return BadRequest();
    }

    /// <summary>
    /// gets the products stocks available
    /// </summary>
    /// <param name="sku"></param>
    /// <returns></returns>
    [HttpPost("GetProductsStock")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetProductsStock(string sku)
    {
        int? storeId = null;
        if (User.IsInRole(RoleList.ExternalUser))
        {
            storeId = await GetStoreIdForUserAsync();
        }
        if (User.IsInRole(RoleList.Administrator))
        {
            ViewData["role"] = "Yes";
        }
        else if (
            User.IsInRole(RoleList.InventoryManager) && User.IsInRole(RoleList.ShippingManager)
        )
        {
            ViewData["role"] = "InvShip";
        }
        else if (User.IsInRole(RoleList.InventoryManager))
        {
            ViewData["role"] = "Inv";
        }
        else if (User.IsInRole(RoleList.ShippingManager))
        {
            ViewData["role"] = "Ship";
        }
        else
        {
            ViewData["role"] = "No";
        }

        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = 0;


        var role = ViewData["role"].ToString();

        IQueryable<Stock> myStockModel = _stocksService.GetProductsStock(
            searchValue,
            sku,
            sortColumn,
            sortColumnDirection,
            role,
            storeId
        );

        if (length != null)
        {
            pageSize = length == "-1" ? myStockModel.Count() : Convert.ToInt32(length);
        }

        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        var recordsTotal = myStockModel.Count();

        var data = myStockModel.Skip(skip).Take(pageSize).ToList();

        data.ForEach(x => x.Products.permission = role);

        return Ok(new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        });
    }

    // GET: Stocks/Details/5
    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var stock = await _stocksService.GetAsync(
            stock => stock.Where(x => x.StockId == id.Value)
                .Include(x => x.Products)
                .Include(x => x.Products.AlternateProduct)
                .Include(x => x.Products.ProductImages)
                .Include(x => x.Products.ProductVendorMappings)
                .ThenInclude(y => y.Vendor)
                .Include(x => x.Products.Departments)
                .Include(x => x.Location)
                .Include(x => x.Location.Sites)
                .Include(x => x.ShipStationStore)
        );

        if (stock == null)
        {
            return NotFound();
        }
        return View(stock);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]

    // GET: Stocks/Create
    public async Task<IActionResult> Create()
    { 
        await PopulateDropdownsAsync();
        var stockViewModel = new StockViewModel { Location = new Location() };
        return View(stockViewModel.Stock);
    }

    private async Task PopulateDropdownsAsync()
    {
        var allSites = await _siteService.GetAllAsync();
        var allLocations = await _locationService.GetAllAsync();
        var allShipStationStores = await _shipStationStoreService.GetAllAsync();
        var allActiveProducts = await _productService.GetListAsync(
            i => i.IsActive,
            orderSelectors: [s => s.Sku]
        );

        ViewData["Site"] = new SelectList(allSites, "SiteId", "SiteName");
        ViewData["Location"] = new SelectList(allLocations, "LocationId", "LocationName");
        ViewData["ShipStationStore"] = new SelectList(allShipStationStores, "ShipStationStoreId", "StoreName");

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            ViewData["SiteName"] = new SelectList(allSites.OrderBy(s => s.SiteName), "SiteId", "SiteName");
            ViewData["LocationName"] = new SelectList(allLocations.OrderBy(l => l.LocationName), "LocationId", "LocationName");
            ViewData["ProductId"] = new SelectList(allActiveProducts.OrderBy(ap => ap.Sku), "ProductId", "Sku");
            return;
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            var externalSites = allSites.Where(s => s.IsExternal).OrderBy(s => s.SiteName);
            var externalLocations = allLocations.Where(l => l.IsExternal).OrderBy(l => l.LocationName);
            var externalProducts = allActiveProducts.Where(ap => ap.IsExternalProduct).OrderBy(ap => ap.Sku);

            ViewData["SiteName"] = new SelectList(externalSites, "SiteId", "SiteName");
            ViewData["LocationName"] = new SelectList(externalLocations, "LocationId", "LocationName");
            ViewData["ProductId"] = new SelectList(externalProducts, "ProductId", "Sku");
            return;
        }

        var internalSites = allSites.Where(s => !s.IsExternal).OrderBy(s => s.SiteName);
        var internalLocations = allLocations.Where(l => !l.IsExternal).OrderBy(l => l.LocationName);
        var internalProducts = allActiveProducts.Where(ap => !ap.IsExternalProduct).OrderBy(ap => ap.Sku);

        ViewData["SiteName"] = new SelectList(internalSites, "SiteId", "SiteName");
        ViewData["LocationName"] = new SelectList(internalLocations, "LocationId", "LocationName");
        ViewData["ProductId"] = new SelectList(internalProducts, "ProductId", "Sku");
    }

    // POST: Stocks/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    ///
    /// </summary>
    /// <param name="stockViewModel"></param>
    /// <returns></returns>
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("StockId,LocationId,ProductId,IsPrimary,IsExternal,TotalAvailable, ShipStationStoreId")] Stock stock)
    {
        if (stock.ProductId == 0)
        {
            ModelState.AddModelError(nameof(Stock.ProductId), "The Product field is required");
        }
        if (stock.LocationId == 0)
        {
            ModelState.AddModelError(nameof(Stock.LocationId), "The Location field is required");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(stock);
        }
        var existingStock = await _stocksService.GetAsync(x => x.ProductId == stock.ProductId && x.LocationId == stock.LocationId);
        if (existingStock != null)
        {
            ModelState.AddModelError("", "A stock item with the same product and location already exists.");
            return View(stock);
        }

        if (stock.StockId == 0)
        {
            stock.RecentlyReadded = true;
            stock.ModifyByUser = User.Identity?.Name;
            stock.ModifyDate = DateTime.UtcNow;
            await _stocksService.AddAsync(stock);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator +"," + RoleList.InventoryManager)]
    // GET: Stocks/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var stock = await _stocksService.GetAsync(
            x => x.StockId == id.Value,
            includes:
                [x => x.Location]
        );

        if (stock == null)
        {
            return NotFound();
        }

        // show filtered ViewData based on internal/external user role, show both for admin/manager role
        var allSites = await _siteService.GetAllAsync();
        var allLocationsInSite = await _locationService.GetListAsync(x => x.SiteId == stock.Location.SiteId);
        var allShipStationStores = await _shipStationStoreService.GetAllAsync();
        var allProducts = await _productService.GetAllAsync(
            orderSelectors:
            [s => s.Sku]

        );

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            ViewData["ShipStationStore"] = new SelectList(allShipStationStores, "ShipStationStoreId", "StoreName");

            // show all (internal + external)
            ViewData["SiteName"] = new SelectList(
                allSites.OrderBy(s => s.SiteName),
                "SiteId",
                "SiteName",
                stock.Location.SiteId
            );
            ViewData["LocationName"] = new SelectList(
                allLocationsInSite
                    .Select(
                        e => new { e.LocationId, e.LocationName }
                    )
                    .OrderBy(l => l.LocationName),
                "LocationId",
                "LocationName",
                stock.LocationId
            );
            ViewData["ProductId"] = new SelectList(
                allProducts.OrderBy(p => p.Sku),
                "ProductId",
                "Sku",
                stock.ProductId
            );

            return View(stock);
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            // filter down to only external sites, locations, & products
            var externalSites = allSites.Where(s => s.IsExternal);
            var externalLocations = allLocationsInSite.Where(l => l.IsExternal);
            var externalProducts = allProducts.Where(p => p.IsExternalProduct);

            ViewData["SiteName"] = new SelectList(
                externalSites.OrderBy(s => s.SiteName),
                "SiteId",
                "SiteName",
                stock.Location.SiteId
            );
            ViewData["LocationName"] = new SelectList(
                externalLocations
                    .Select(
                        e => new { e.LocationId, e.LocationName }
                    )
                    .OrderBy(l => l.LocationName),
                "LocationId",
                "LocationName",
                stock.LocationId
            );
            ViewData["ProductId"] = new SelectList(
                externalProducts.OrderBy(p => p.Sku),
                "ProductId",
                "Sku",
                stock.ProductId
            );

            return View(stock);
        }

        // else implied -> if neither admin/manager, nor external, then you are internal user, so show only internal
        var internalSites = allSites.Where(s => !s.IsExternal);

        var internalLocations = allLocationsInSite.Where(l => !l.IsExternal);

        var internalProducts = allProducts.Where(p => !p.IsExternalProduct);

        ViewData["SiteName"] = new SelectList(
            internalSites.OrderBy(s => s.SiteName),
            "SiteId",
            "SiteName",
            stock.Location.SiteId
        );
        ViewData["LocationName"] = new SelectList(
            internalLocations
                .Select(e => new { e.LocationId, e.LocationName })
                .OrderBy(l => l.LocationName),
            "LocationId",
            "LocationName",
            stock.LocationId
        );
        ViewData["ProductId"] = new SelectList(
            internalProducts.OrderBy(p => p.Sku),
            "ProductId",
            "Sku",
            stock.ProductId
        );

        return View(stock);
    }

    /// <summary>
    /// Handles the POST request to edit a stock.
    /// </summary>
    /// <param name="id">The ID of the stock to edit.</param>
    /// <param name="stock">The updated stock object.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    [HttpPost]
    
    public async Task<IActionResult> Edit(
        int id,
        [Bind("StockId,ProductId,LocationId,ShipStationStoreId,TotalAvailable,IsPrimary,IsExternal")]
        Stock stock
    )
    {
        if (id != stock.StockId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                stock.Location = await _locationService.GetAsync(l => l.LocationId == stock.LocationId);
                stock.Products = await _productService.GetAsync(p => p.ProductId == stock.ProductId);
                stock.ModifyByUser = User.Identity?.Name;
                stock.ModifyDate = DateTime.UtcNow;

                await _stocksService.UpdateAsync(stock);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _stocksService.IsExistsAsync(x => x.StockId == stock.StockId))
                {
                    return NotFound();
                }

                throw;

            }

            return RedirectToAction(nameof(Index));
        }

        // show filtered ViewData based on internal/external user role, show both for admin/manager role
        var allSites = await _siteService.GetAllAsync();
        var allLocationsInSite = await _locationService.GetListAsync(x => x.SiteId == stock.Location.SiteId);
        var allProducts = await _productService.GetAllAsync(
            orderSelectors: [s => s.Sku]

        );

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            // show all (internal + external)
            ViewData["SiteName"] = new SelectList(
                allSites.OrderBy(s => s.SiteName),
                "SiteId",
                "SiteName",
                stock.Location.SiteId
            );
            ViewData["LocationName"] = new SelectList(
                allLocationsInSite
                    .Select(
                        e => new { e.LocationId, e.LocationName }
                    )
                    .OrderBy(l => l.LocationName),
                "LocationId",
                "LocationName",
                stock.LocationId
            );
            ViewData["ProductId"] = new SelectList(
                allProducts.OrderBy(p => p.Sku),
                "ProductId",
                "Sku",
                stock.ProductId
            );

            return View(stock);
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            // filter down to only external sites, locations, & products
            var externalSites = allSites.Where(s => s.IsExternal);
            var externalLocations = allLocationsInSite.Where(l => l.IsExternal);
            var externalProducts = allProducts.Where(p => p.IsExternalProduct);

            ViewData["SiteName"] = new SelectList(
                externalSites.OrderBy(s => s.SiteName),
                "SiteId",
                "SiteName",
                stock.Location.SiteId
            );
            ViewData["LocationName"] = new SelectList(
                externalLocations
                    .Select(
                        e => new { e.LocationId, e.LocationName }
                    )
                    .OrderBy(l => l.LocationName),
                "LocationId",
                "LocationName",
                stock.LocationId
            );
            ViewData["ProductId"] = new SelectList(
                externalProducts.OrderBy(p => p.Sku),
                "ProductId",
                "Sku",
                stock.ProductId
            );

            return View(stock);
        }

        // else implied -> if neither admin/manager, nor external, then you are internal user, so show only internal
        var internalSites = allSites.Where(s => !s.IsExternal);
        var internalLocations = allLocationsInSite.Where(l => !l.IsExternal);
        var internalProducts = allProducts.Where(p => !p.IsExternalProduct);
        ViewData["SiteName"] = new SelectList(
            internalSites.OrderBy(s => s.SiteName),
            "SiteId",
            "SiteName",
            stock.Location.SiteId
        );
        ViewData["LocationName"] = new SelectList(
            internalLocations
                .Select(e => new { e.LocationId, e.LocationName })
                .OrderBy(l => l.LocationName),
            "LocationId",
            "LocationName",
            stock.LocationId
        );
        ViewData["ProductId"] = new SelectList(
            internalProducts.OrderBy(p => p.Sku),
            "ProductId",
            "Sku",
            stock.ProductId
        );

        return View(stock);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]
    // GET: Stocks/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var stock = await _stocksService.GetAsync(
            x => x.StockId == id.Value,
            includes:
                [x => x.Location,
                x => x.Products]
        );

        if (stock == null)
        {
            return NotFound();
        }

        return View(stock);
    }

    // POST: Stocks/Delete/5
    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _stocksService.DeleteConfirmed(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    public async Task<IActionResult> LocationsBySiteId(int siteId)
    {
        var locationsList = await _locationService.GetListAsync(
            x => x.SiteId == siteId,
            orderSelectors:
                [x => x.LocationName]
        );
        return Json(locationsList);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IActionResult DownloadExcelTemplate()
    {
        try
        {
            List<string> fieldArray =
            [
                "Product_SKU",
                "Location_Name",
                "TotalAvailable",
                "IsPrimary",
                "IsExternal"
            ];

            List<List<string>> sampleArray = [["Test_Sku", "Test_Location_Name", "100", "TRUE", "TRUE"]];

            using MemoryStream ms = new();
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Prepare the template with field names and sample values
                ExcelFileExtensions.PrepareTemplate<Stock>(fieldArray, sampleArray, document);
            }

            ms.Seek(0, SeekOrigin.Begin);

            // Return the Excel file as a download
            return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "Stocks_Template.xlsx"
            };
        }
        catch (Exception ex)
        {
            BadRequest(ex);
        }
        return Ok();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userUpload"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    public async Task<IActionResult> BatchUserUpload(IFormFile userUpload)
    {
        try
        {
            if (userUpload == null || userUpload.Length == 0)
            {
                return BadRequest("No file is uploaded.");
            }

            string extension = Path.GetExtension(userUpload.FileName);
            if (extension != ".xlsx")
            {
                TempData["ErrorMessage"] = "Make sure the file is an Excel file with a .xlsx extension.";
                return RedirectToAction("Index");
            }

            string[] fieldArray = ["Product_SKU", "Location_Name", "TotalAvailable", "IsPrimary", "IsExternal"];

            var stream = new MemoryStream();
            await userUpload.CopyToAsync(stream);
            stream.Position = 0;

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, true))
            {
                var sheetData = ExcelFileExtensions.GetSheetData(document);  // Get the sheet data
                var properties = typeof(Stock).GetProperties();
                var failedRows = new List<int>();

                foreach (var row in sheetData.Elements<Row>().Skip(1)) // Skip header row
                {
                    var stock = new Stock();
                    try
                    {
                        // Get the row's data
                        var rowData = ExcelFileExtensions.ProcessRow(document, row, fieldArray, properties);

                        // Handle Product_SKU
                        var productSku = rowData["Product_SKU"].ToString();
                        var product = await _productService.GetAsync(x => x.Sku == productSku) ?? throw new Exception("Invalid Product");
                        stock.ProductId = product.ProductId;

                        // Handle Location_Name
                        var locationName = rowData["Location_Name"].ToString();
                        var location = await _locationService.GetAsync(x => x.LocationName == locationName) ?? throw new Exception("Invalid Location");
                        stock.LocationId = location.LocationId;

                        // Set additional properties dynamically
                        foreach (var property in properties)
                        {
                            if (rowData.TryGetValue(property.Name, out object value) && value != null)
                            {
                                property.SetValue(stock, value);
                            }
                        }

                        var isExists = await _stocksService.IsExistsAsync(x => x.ProductId == stock.ProductId && x.LocationId == stock.LocationId);
                        if (isExists) throw new Exception("A stock item with the same product and location already exists.");

                        stock.RecentlyReadded = true;
                        stock.ModifyByUser = User.Identity.Name;
                        stock.ModifyDate = DateTime.UtcNow;

                        await _stocksService.AddAsync(stock);
                    }
                    catch (Exception ex)
                    {
                        failedRows.Add((int)row.RowIndex.Value);
                        ExcelFileExtensions.AppendErrorMessage(row, ex.Message, document); // Add error message to row
                    }
                }

                // If there are failed rows, return the updated file with error messages using GenerateErrorFile
                if (failedRows.Count > 0)
                {
                    // Write the updated document back to the memory stream
                    stream.Position = 0;  // Reset stream position
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = "Stock_Upload_Errors.xlsx"
                    };
                }
            }

            return Ok("Data imported successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }



    private async Task<Location> GetOrCreateLocationAsync(StockViewModel stockViewModel)
    {
        Location location;
        if (stockViewModel.CreateNewLocation)
        {
            location = stockViewModel.Location;
            if (location != null)
            {
                location.LocationName = location.LocationName.Trim().ToUpperInvariant();
                location.LocationDescription ??= "";
                var existingLocation = await _locationService.GetAsync(l => l.LocationName == location.LocationName);
                if (existingLocation != null)
                {
                    return null;
                }

                await _locationService.AddAsync(location);
            }
        }
        else
        {
            location = stockViewModel.LocationId != 0 ? await _locationService.GetAsync(x => x.LocationId == stockViewModel.LocationId) : null;

            if (location == null)
            {
                return null;
            }
        }

        return location;
    }

    private async Task AddNewStockAsync(StockViewModel stockViewModel, Location location)
    {
        var newStock = stockViewModel.Stock;
        newStock.Location = location;
        newStock.LocationId = location.LocationId;

        newStock.RecentlyReadded = true;
        newStock.ModifyByUser = User.Identity?.Name;
        newStock.ModifyDate = DateTime.UtcNow;
        await _stocksService.AddAsync(newStock);
    }
    public IActionResult DownloadBarcode(int id)
    {
        using MemoryStream ms = new();
        var b = new Barcode
        {
            IncludeLabel = true,
            LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
        };
        using var bitmap = SKBitmap.FromImage(b.Encode(BarcodeStandard.Type.Code128B, $"{HttpContext.Request.Host.Value}/Stocks/Details/{id}"));
        bitmap.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(ms);
        string fileName = $"barcode{id}.jpg";
        return File(ms.ToArray(), "image/jpeg", fileName);
    }


    /// <summary>
    /// data model for the stock page
    /// </summary>
    public class StockPageDataModel
    {
        private PaginatedList<Product> ProductRowsExportAllHiddenFromUser { get; set; }
        private PaginatedList<Stock> StockRowsDetails { get; set; }
        private PaginatedList<Product> ProductRowsExportPage { get; set; }
    }

    // data model type to use as a type in one place, to avoid some Linq type inference errors
    private class QueryDataModel
    {
        private string Product { get; set; }
        private int Total { get; set; }
        public string Description { get; set; }
    }
}