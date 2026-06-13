using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Config;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using NirfForm = ERPWebApp.Models.NirfForms.NirfForm;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml;
using ERPWebApp.Extensions;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class NirfFormsController : Controller
{
    private readonly IGraphAPIService _graphAPIService;
    private readonly INirfProductMappingService _nirfProductMappingService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserService _userService;
    private readonly ISkuCategoryService _skuCategoryService;
    private readonly ISkuColorService _skuColorService;
    private readonly ISkuUnitOfMeasureService _skuUnitOfMeasureService;
    private readonly IFontService _fontService;
    private readonly INirfFormService _nirfFormService;
    private readonly INirfForecastingService _nirfForecastingService;
    private readonly INirfInventoryService _nirfInventoryService;
    private readonly INirfPackagingService _nirfPackagingService;
    private readonly INirfShippingService _nirfShippingService;
    private readonly INirfParametersService _nirfParametersService;
    private readonly INirfVendorMappingService _nirfVendorMappingService;
    private readonly INirfImageMappingService _nirfImageMappingService;
    private readonly IShippingProviderService _shippingProviderService;
    private readonly ISiteService _siteService;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly IFilesService _filesService;
    private readonly IProductVendorMappingService _productVendorMappingService;
    private readonly IStocksService _stocksService;
    private readonly ILocationService _locationService;
    private readonly IProductFilesMappingsService _productFilesMappingsService;
    private readonly IProductContainerService _productContainerService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ExternalEndpoints _endpoints;
    //static model will need to monitor if it does need to change, but anytime it is called, it gets assigned before use, overlapping should nto occur
    private static NirfViewModel _nirfDbFull = new();
    public NirfFormsController(
        UserManager<IdentityUser> userManager,
        IGraphAPIService graphAPIService,
        INirfProductMappingService nirfProductMappingService,
        IDepartmentService departmentService,
        IUserService userService,
        ISkuCategoryService skuCategoryService,
        ISkuColorService skuColorService,
        ISkuUnitOfMeasureService skuUnitOfMeasureService,
        IFontService fontService,
        INirfFormService nirfFormService,
        INirfForecastingService nirfForecastingService,
        INirfInventoryService nirfInventoryService,
        INirfPackagingService nirfPackagingService,
        INirfShippingService nirfShippingService,
        INirfParametersService nirfParametersService,
        INirfVendorMappingService nirfVendorMappingService,
        INirfImageMappingService nirfImageMappingService,
        IShippingProviderService shippingProviderService,
        ISiteService siteService,
        IProductService productService,
        IVendorService vendorService,
        IFilesService filesService,
        IProductVendorMappingService productVendorMappingService,
        IStocksService stocksService,
        ILocationService locationService,
        IProductFilesMappingsService productFilesMappingsService,
        IProductContainerService productContainerService,
        IOptions<ExternalEndpoints> endpoints
    )
    {
        _userManager = userManager;
        _graphAPIService = graphAPIService;
        _nirfProductMappingService = nirfProductMappingService;
        _departmentService = departmentService;
        _userService = userService;
        _skuCategoryService = skuCategoryService;
        _skuColorService = skuColorService;
        _skuUnitOfMeasureService = skuUnitOfMeasureService;
        _fontService = fontService;
        _nirfFormService = nirfFormService;
        _nirfForecastingService = nirfForecastingService;
        _nirfInventoryService = nirfInventoryService;
        _nirfPackagingService = nirfPackagingService;
        _nirfShippingService = nirfShippingService;
        _nirfParametersService = nirfParametersService;
        _nirfVendorMappingService = nirfVendorMappingService;
        _nirfImageMappingService = nirfImageMappingService;
        _shippingProviderService = shippingProviderService;
        _siteService = siteService;
        _productService = productService;
        _vendorService = vendorService;
        _filesService = filesService;
        _productVendorMappingService = productVendorMappingService;
        _stocksService = stocksService;
        _locationService = locationService;
        _productFilesMappingsService = productFilesMappingsService;
        _productContainerService = productContainerService;
        _endpoints = endpoints.Value;
    }

    private static readonly DateTime _date = TimeZoneInfo.ConvertTime(
        DateTime.Now,
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
    );

    /// <summary>
    /// index of the nirf form page.  Grabs all nirfforms and displays them in a datatable.  The mapping links the nirfform to the product
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public IActionResult Index()
    {
        var query = (IQueryable<NirfProductMapping> nf) => nf
        .Include(x => x.NirfForm)
        .Include(x => x.Product)
        .GroupBy(x => x.NirfFormId)
        .Select(g => g.OrderBy(p => p.ProductId).FirstOrDefault());

        var model = _nirfProductMappingService.QueryFilter(query).ToList();

        return View(model);
    }

    /// <summary>
    /// Initiallizes the create page, this page doesnt get any data from index but will need to grab the department list
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> Create()
    {
        ViewData["DepartmentList"] = new SelectList(_departmentService.GetAll(),
            "DepartmentId",
            "DepartmentName"
        );
        var usersInRole = await _userService.GetUsersInRole();

        ViewData["InventoryList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.InventoryManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        ViewData["ParameterList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.ProductionManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        ViewData["PackagingList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.ProductionManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        ViewData["ForecastingList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.FinancialManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        ViewData["ShippingList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.ShippingManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        ViewData["VendorList"] = new SelectList(
            usersInRole.Where(
                x => x.RoleName == RoleList.ShippingManager ||
                    x.RoleName == RoleList.Administrator ||
                    x.UserName == this.User.Identity.Name
            ).DistinctBy(x => x.Id).OrderBy(x => x.UserName),
            "Id",
            "UserName"
        );

        _userManager.GetUserId(User);

        return View();
    }

    /// <summary>
    /// gets the sku catagories for the create page
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuCategory>))]
    public async Task<IEnumerable<SkuCategory>> GetSkuCategories()
    {
        return await _skuCategoryService.GetListAsync(x => x.IsActive);
    }

    /// <summary>
    /// grabs the sku colors for the create page
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuColor>))]
    public async Task<IEnumerable<SkuColor>> GetSkuColors()
    {
        return await _skuColorService.GetListAsync(x => x.IsActive);
    }

    /// <summary>
    /// grabs the unit of measure for the create page
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuUnitOfMeasure>))]
    public async Task<IEnumerable<SkuUnitOfMeasure>> GetSkuUnitOfMeasure()
    {
        return await _skuUnitOfMeasureService.GetListAsync(x => x.IsActive);
    }

    /// <summary>
    /// takes in a nirf form id and outputs the edit page for that nirf form
    /// </summary>
    /// <param name="id">id for the nirf forms</param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || !_nirfFormService.IsExists())
        {
            return NotFound();
        }
        //block the edit page if a form is canceled or in progress
        var nirfForm = await _nirfFormService.GetAsync(x => x.NirfFormId == id);
        if (nirfForm == null)
        {
            return NotFound();
        }

        ResetViewData();

        _nirfDbFull = new NirfViewModel
        {
            NirfForms = nirfForm,
            NirfProductMapping = await _nirfProductMappingService.GetAsync(
                x => x.NirfFormId == id,
                includes: [x => x.Product, x => x.Product.Departments]
            ),
            NirfForecastings = await _nirfForecastingService.GetAsync(x => x.NirfFormId == id),
            NirfInventories = (await _nirfInventoryService.GetAsync(x => x.NirfFormId == id)) ?? new NirfInventory(),
            NirfPackagings = await _nirfPackagingService.GetAsync(x => x.NirfFormId == id),
            NirfShippings = await _nirfShippingService.GetAsync(x => x.NirfFormId == id),
            NirfParameters = await _nirfParametersService.GetAsync(x => x.NirfFormId == id),
            NirfVendorMapping = await _nirfVendorMappingService.GetAsync(x => x.NirfFormId == id),
            NirfShippingProvider = new List<NirfShippingProdivder>(),
            NirfImageMapping = await _nirfImageMappingService.GetListAsync(
                x => x.NirfFormId == id,
                includes: [x => x.Files]
            ),
            NirfProducts = await _nirfProductMappingService.GetVariantProducts(id.Value)
        };

        var getProviders = await _shippingProviderService.GetAllAsync();

        foreach (var provider in getProviders)
        {
            var newShippingMapping = new NirfShippingProdivder
            {
                ShippingProvider = provider,
                ShippingProviderId = provider.ShippingProviderId,
                ShippingWeight = 0.01M,
                ShippingCost = 0.01M,
                ShippingSize = "0",
                NirfShippingProviderId = 0
            };
            _nirfDbFull.NirfShippingProvider.Add(newShippingMapping);
        }

        //grabs the departments
        var items = await _departmentService.GetAllAsync();
        //puts the departments that were selected from the <departments> field on the form in a list
        var selected = items
            .Where(_nirfDbFull.NirfProductMapping.Product.Departments.Contains)
            .Select(x => x.DepartmentId);

        // "items" are the whole department list, "selected" is a list of int's to select from the "selected" List
        ViewData["DepartmentList"] = new MultiSelectList(
            items,
            "DepartmentId",
            "DepartmentName",
            selected
        );

        var getMappings = await _nirfShippingService.GetAsync(
            x => x.NirfFormId == id,
            includes: [x => x.NirfShippingProvider]
        );

        var providerIds = "";
        if (getMappings != null)
        {
            foreach (var item in getMappings.NirfShippingProvider)
            {
                providerIds += "," + item.ShippingProviderId;

            }

            var ShippersList = await _shippingProviderService.GetListAsync(
                    x => !providerIds.Contains(x.ShippingProviderId.ToString())
                );

            ViewData["ShippersList"] = new SelectList(
                ShippersList,
                "ShippingProviderId",
                "ShippingProviderName"
            );
        }
        else
        {
            ViewData["ShippersList"] = new SelectList(
               await _shippingProviderService.GetAllAsync(),
                "ShippingProviderId",
                "ShippingProviderName"
                );
        }
        ViewData["SiteList"] = new SelectList(
          await _siteService.GetAllAsync(),
            "SiteId",
            "SiteName"

        );
        ViewData["FontList"] = new SelectList(await _fontService.GetAllAsync(), "FontId", "FontTitle");
        _nirfDbFull.NirfProductMapping.Product.FulfillmentCost = Math.Round(_nirfDbFull.NirfProductMapping.Product.FulfillmentCost, 2);

        return View(_nirfDbFull);
    }

    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(
            "NirfForms, NirfProductMapping, NirfProductMapping, NirfProductMapping.Product.DepartmentList"
        )]
        NirfViewModel nirfViewModel
    )
    {
        if (id != nirfViewModel.NirfForms.NirfFormId)
        {
            return NotFound();
        }
        try
        {
            // getting the form from the viewmodel
            NirfForm nirfForm = nirfViewModel.NirfForms;
            Product product = nirfViewModel.NirfProductMapping.Product;
            product.Departments = await _departmentService.GetListAsync(
                x => product.DepartmentList.Contains(x.DepartmentId)
            );
            await _departmentService.DeleteDepartmentProduct(product.ProductId);

            if (product.Departments.Any())
                foreach (var department in product.Departments)
                {
                    switch (department.DepartmentName)
                    {
                        case "UVP":
                            {
                                //UVP: White layers, color layers, printed/stained?, Sizing (x,y)
                                nirfForm.IsWhiteLayer = true;
                                nirfForm.IsColorLayer = true;
                                nirfForm.IsSizingX = true;
                                nirfForm.IsSizingY = true;
                                nirfForm.IsUVPType = true;
                                break;
                            }
                        case "Engraving":
                            {
                                //Engraving: Sizing (x,y), loop count, speed, current (A), Frequency KHz
                                nirfForm.IsLoopCount = true;
                                nirfForm.IsSpeed = true;
                                nirfForm.IsSizingX = true;
                                nirfForm.IsSizingY = true;
                                nirfForm.IsCurrent = true;
                                nirfForm.IsFrequency = true;
                                break;
                            }
                        case "Sublimation":
                            {
                                //Sublimation: Sizing (x,y), Time to Sublimate, Temperature
                                nirfForm.IsSizingX = true;
                                nirfForm.IsSizingY = true;
                                nirfForm.IsTemperature = true;
                                break;
                            }
                        case "Embroidery":
                            {
                                //Embroidery: Thread Colors, Font
                                nirfForm.IsFont = true;
                                nirfForm.IsThreadColor = true;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }

            product.ModifyDate = _date;
            product.ModifyByUser = User.Identity.Name;
            product.ModifySource = "WebApp";

            await _nirfFormService.UpdateAsync(nirfForm);
            await _productService.UpdateAsync(product);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }


    /// <summary>
    /// takes in the nirf form id and sends that information to the detail page
    /// </summary>
    /// <param name="id">id of the nirfform</param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public async Task<IActionResult> Details(int? id)
    {
        ResetViewData();

        var nirfViewData = new NirfViewModel
        {
            NirfProductMapping = await _nirfProductMappingService.GetAsync(
                x => x.NirfFormId == id,
                includes: [
                    x => x.Product,
                    x => x.NirfForm,
                    x => x.Product.Departments
                ]
            ),
            NirfForecastings = await _nirfForecastingService.GetAsync(x => x.NirfFormId == id),
            NirfInventories = await _nirfInventoryService.GetAsync(
                x => x.NirfFormId == id,
                includes: [
                    x => x.MainLocation,
                    x => x.AltMainLocation,
                    x => x.MembraneLocation,
                    x => x.AltMembraneLocation
                ]
            ),
            NirfPackagings = await _nirfPackagingService.GetAsync(x => x.NirfFormId == id),
            NirfShippings = await _nirfShippingService.GetAsync(
                x => x.NirfFormId == id,
                includes: [
                    x => x.NirfShippingProvider
                ]
            ),
            NirfParameters = await _nirfParametersService.GetAsync(
                x => x.NirfFormId == id,
                includes: [
                    x => x.Font
                ]
            ),
            NirfVendorMapping = await _nirfVendorMappingService.GetAsync(x => x.NirfFormId == id),
            NirfImageMapping = await _nirfImageMappingService.GetListAsync(
                x => x.NirfFormId == id,
                null,
                includes: [
                    x => x.Files,
                ]
            ),
            ShippingProviders = await _shippingProviderService.GetAllAsync(),
            NirfProducts = await _nirfProductMappingService.GetVariantProducts(id.Value)
        };

        //grabs the departments
        var items = await _departmentService.GetListAsync(x => nirfViewData.NirfProductMapping.Product.Departments.Contains(x));

        //puts the departments that were selected from the <departments> field on the form in a list
        var selected = items
        .Where(z => nirfViewData.NirfProductMapping.Product.Departments.Contains(z))
        .Select(x => x.DepartmentId);

        // "items" are the whole department list, "selected" is a list of int's to select from the "selected" List
        ViewData["DepartmentList"] = new MultiSelectList(
            items,
            "DepartmentId",
            "DepartmentName",
            selected
        );
        ViewData["Measure"] = new SelectList(
            _nirfFormService.GetList(x => x.NirfFormId == id),
            "UnitOfMeasure",
            "UnitOfMeasure"
        );

        return View(nirfViewData);
    }

    /// <summary>
    /// emails the creator of the nirf form, will only email when all forms are complete
    /// </summary>
    /// <param name="id">nirf form id</param>
    /// <param name="section">section that was completed</param>
    public async Task EmailNirfCreator(int id, string section)
    {
        var getNirfForm = _nirfProductMappingService.Get(
            x => x.NirfFormId == id,
            includes: [
                x => x.NirfForm,
                x => x.Product
            ]
        );

        if (getNirfForm == null)
        {
            return;
        }
        //Check to see if there are any departments not done yet.
        var query = _nirfFormService.GetAllNirfFormIdById(getNirfForm.NirfFormId);
        //Check to see if there are any departments not done yet.

        if (query != null && query.Any())
        {
            var getUser = _userService.Get(x => x.UserName == getNirfForm.NirfForm.CreatedBy);
            await _graphAPIService.SendEmailAlert(
                "ERP New NIRF Form : " + getNirfForm.Product.Sku,
                "Hello, "
                + " Please head to <a href='" + _endpoints.AppDomain + "/NirfForms/Edit/"
                + getNirfForm.NirfFormId
                + "'> HERE </a> to review the NIRF form."
                + "<br/>" + section + " was completed for the " + getNirfForm.Product.Sku
                + " NIRF form. <br/>" + "All sections are completed.",
                getUser.Email,
                null
            );
        }
    }

    /// <summary>
    /// gets the asp user id, function called at every portion signing
    /// </summary>
    /// <returns></returns>
    public string GetAspUser()
    {
        var getUser = "";
        var manusers = _userManager.Users.ToList();
        foreach (IdentityUser user in manusers)
        {
            var test = user.UserName;
            if (user.UserName == this.User.Identity.Name)
            {
                getUser = user.Id;
            }
        }
        return getUser;
    }

    /// <summary>
    /// creation save page, saves the entered information and starts the process of emailing
    /// </summary>
    /// <param name="nirfForm">nirf form information enetred</param>
    /// <param name="imagesInput">list of images to be saved</param>
    /// <param name="filterText">filter list of departments</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
    public async Task<IActionResult> Create(
        [Bind(
            "NirfForms, NirfProductMapping, NirfProductMapping.Products"
        )]
        NirfViewModel nirfForm,
        IFormFile[] imagesInput,
         string filterText,
         string InventoryList,
         string ParameterList,
         string PackagingList,
         string ForecastingList,
         string ShippingList,
         string VendorList
    )
    {

        if (nirfForm == null)
        {
            TempData["CreateError"] = "Nirf Form not Found";
            return RedirectToAction("Create");
        }

        if (!ModelState.IsValid)
        {
            return View(nirfForm);
        }

        try
        {

            // getting the form from the viewmodel
            _nirfDbFull.NirfProductMapping = nirfForm.NirfProductMapping;
            _nirfDbFull.NirfProductMapping.NirfForm = nirfForm.NirfForms;
            _nirfDbFull.NirfProductMapping.NirfForm.CreatedBy = this.User.Identity.Name;
            _nirfDbFull.NirfProductMapping.NirfForm.AspUserId = GetAspUser();

            var getProductSku = _productService.IsExists(x => x.Sku == _nirfDbFull.NirfProductMapping.Product.Sku);
            //if check product sku amd nirf sku
            if (getProductSku)
            {
                TempData["CreateError"] = "Sku cannot be the same as exsisting Sku";
                return RedirectToAction("Create");
            }

            var splitString = filterText.Split(",");
            foreach (var dept in splitString)
            {
                var getDept = _departmentService.Get(x => x.DepartmentId.ToString() == dept);

                if (getDept.DepartmentName == "UVP")
                {
                    //UVP: White layers, color layers, printed/stained?, Sizing (x,y)
                    _nirfDbFull.NirfProductMapping.NirfForm.IsWhiteLayer = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsColorLayer = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingX = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingY = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsUVPType = true;

                }
                if (getDept.DepartmentName == "Engraving")
                {
                    //Engraving: Sizing (x,y), loop count, speed, current (A), Frequency KHz
                    _nirfDbFull.NirfProductMapping.NirfForm.IsLoopCount = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSpeed = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingX = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingY = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsCurrent = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsFrequency = true;

                }
                if (getDept.DepartmentName == "Sublimation")
                {
                    //Sublimation: Sizing (x,y), Time to Sublimate, Temperature
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingX = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsSizingY = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsTemperature = true;
                }
                if (getDept.DepartmentName == "Embroidery")
                {
                    //Embroidery: Thread Colors, Font
                    _nirfDbFull.NirfProductMapping.NirfForm.IsFont = true;
                    _nirfDbFull.NirfProductMapping.NirfForm.IsThreadColor = true;
                }
            }
            _nirfDbFull.NirfProductMapping.Product = _departmentService.UpdateProductDepartments(splitString, _nirfDbFull.NirfProductMapping.Product);
            _nirfDbFull.NirfProductMapping.Product.ModifyDate = _date;
            _nirfDbFull.NirfProductMapping.Product.ModifyByUser = this.User.Identity.Name;
            _nirfDbFull.NirfProductMapping.Product.ModifySource = "WebApp";
            //setting the department list
            List<Department> departments = new List<Department>();
            foreach (var v in splitString)
            {
                var department = _departmentService.Get(x => x.DepartmentId.ToString() == v);
                departments.Add(department);
            }
            //setup the form to be submitted to the DB
            _nirfDbFull.NirfProductMapping.NirfForm.StartedDate = _date;
            _nirfDbFull.NirfProductMapping.Product.Departments = departments;
            _nirfDbFull.NirfProductMapping.NirfForm.CreatedBy = User.Identity.Name;
            _nirfDbFull.NirfProductMapping.NirfForm.AspUserId = GetAspUser();

            await _nirfFormService.AddAsync(_nirfDbFull.NirfProductMapping.NirfForm);

            await _productService.AddAsync(_nirfDbFull.NirfProductMapping.Product);

            await _nirfProductMappingService.AddAsync(_nirfDbFull.NirfProductMapping);

            //add images
            var files = new List<Files>();
            if (imagesInput != null)
            {
                //create File objects(yes multiple)
                foreach (var img in imagesInput)
                {
                    var file = new Files
                    {
                        FileName = Path.GetFileName(img.FileName),
                        ContentType = img.ContentType,
                        FileType = FileType.Image,
                        ProductId = null,
                        Product = null,
                        IsThumbnail = false,
                        IsDetailed = false
                    };

                    using var reader = new BinaryReader(img.OpenReadStream());
                    file.Content = reader.ReadBytes((int)img.Length);
                    //add the new file to the list of files
                    files.Add(file);
                }

                //add each file from the list to the DB then add that new ID of files to the mapping table
                foreach (var img in files)
                {
                    //get the file id
                    await _filesService.AddAsync(img);

                    //create a new mapping with all the info made above
                    var newMapping = new NirfImageMapping()
                    {
                        FileId = img.FileId,
                        NirfFormId = _nirfDbFull.NirfProductMapping.NirfFormId,
                        IsThumbnail = false
                    };

                    await _nirfImageMappingService.AddAsync(newMapping);
                }
            }

            //Email Production manager, shipping manager, customer support manager, financial manager, inventory manager
            var usersInRole = await _userService.GetList(
                x => x.Id == ParameterList ||
                    x.Id == ParameterList ||
                    x.Id == PackagingList ||
                    x.Id == ForecastingList ||
                    x.Id == ShippingList ||
                    x.Id == VendorList ||
                    x.UserName == this.User.Identity.Name
            );

            var ccList = new List<String>();
            var mainEmail = _userService.Get(x => x.Id == _nirfDbFull.NirfProductMapping.NirfForm.AspUserId).Email;

            foreach (var userEmail in usersInRole)
            {
                if (userEmail.Email != mainEmail && !this.User.IsInRole(RoleList.ExternalUser))
                {
                    ccList.Add(userEmail.Email);
                }
            }
            foreach (var emailList in ccList)
            {
                mainEmail += ";" + emailList;
            }
            foreach (var user in ccList)
            {
                await _graphAPIService.SendEmailAlert(
                    "ERP New NIRF Form : " + _nirfDbFull.NirfProductMapping.Product.Sku,
                    "Hello, "
                    + " Please head to <a href=' " + _endpoints.AppDomain + "/NirfForms/Edit/"
                    + _nirfDbFull.NirfProductMapping.NirfFormId
                    + "'> HERE </a> to complete your assigned section."
                    + "<br/> Sku :"
                    + _nirfDbFull.NirfProductMapping.Product.Sku
                    + "<br/> Description :"
                    + _nirfDbFull.NirfProductMapping.Product.Description
                    + "<br/> Disclaimer: If it sends you to the login page, Login and then navigate"
                    + "<br/> back to the email and click the link again to send you to the correct page.",
                    user,
                    null
                );
            }

            var key = _nirfDbFull.NirfProductMapping.NirfFormId;
            return RedirectToAction("Create", new { id = key });
        }
        catch
        {
            var key = _nirfDbFull.NirfProductMapping.NirfFormId;
            return RedirectToAction("Create", new { id = key });
        }
    }

    /// <summary>
    /// grabs locations by the site id
    /// </summary>
    /// <param name="SiteId">site Id used to grab all locations in that site</param>
    /// <returns></returns>
    public async Task<IActionResult> LocationBySiteId(int SiteId)
    {
        var locationsList = await _locationService.GetListAsync(
            x => x.SiteId == SiteId,
            orderSelectors: [x => x.LocationName]
        );

        return Json(locationsList);
    }

    /// <summary>
    /// once inventory portion is finished it will save that portion
    /// </summary>
    /// <param name="nirfForm">nirf form and nirf form inventories</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignInventory(
        [Bind(
            "NirfInventories" )]
        NirfViewModel nirfForm
    )
    {
        if (nirfForm == null || nirfForm.NirfInventories == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        var form = nirfForm.NirfInventories;

        try
        {
            bool getExternal = false;
            foreach (var department in _nirfDbFull.NirfProductMapping.Product.Departments)
            {
                if (department.DepartmentName == "External")
                {
                    getExternal = true;
                }
            }

            var getNirfInv = _nirfInventoryService.Get(x => x.NirfFormId == nirfForm.NirfInventories.NirfFormId);
            if (getNirfInv != null)
            {
                nirfForm.NirfProductMapping = _nirfProductMappingService.Get(x => x.NirfFormId == nirfForm.NirfInventories.NirfFormId);
                var getStock = _stocksService.GetList(x => x.ProductId == nirfForm.NirfProductMapping.ProductId);

                if (getStock != null)
                {
                    foreach (var stock in getStock)
                    {
                        try
                        {
                            await _stocksService.RemoveAsync(stock.StockId);
                        }
                        catch
                        {
                            await _stocksService.UpdateAsync(stock);
                        }
                    }

                    await _nirfInventoryService.RemoveAsync(getNirfInv.NirfInventoryId);
                }
            }

            //setting the location classes
            form.MainLocation = _locationService.Get(x => x.LocationId == form.MainLocationId);

            form.AltMainLocation = _locationService.Get(x => x.LocationId == form.AltMainLocationId);

            form.MembraneLocation = _locationService.Get(x => x.LocationId == form.MembraneLocationId);

            form.AltMembraneLocation = _locationService.Get(x => x.LocationId == form.AltMembraneLocationId);

            var createStock = new Stock
            {
                ProductId = (int)_nirfDbFull.NirfProductMapping.ProductId,
                LocationId = form.MainLocationId,
                Location = form.MainLocation,
                TotalAvailable = 0,
                RecentlyReadded = false,
                IsPrimary = true,
                ModifyByUser = this.User.Identity.Name,
                ModifyDate = _date,
                BeingCounted = false,
                LastCounted = _date,
                IsExternal = getExternal,
            };

            await _stocksService.AddAsync(createStock);

            if (form.AltMainLocationId != form.MainLocationId)
            {
                createStock.StockId = 0;
                createStock.LocationId = form.AltMainLocationId;
                createStock.Location = form.AltMainLocation;
                await _stocksService.AddAsync(createStock);
            }
            if (form.MembraneLocationId != form.MainLocationId && form.MembraneLocationId != form.AltMainLocationId)
            {
                createStock.StockId = 0;
                createStock.LocationId = form.MembraneLocationId;
                createStock.Location = form.MembraneLocation;
                await _stocksService.AddAsync(createStock);
            }
            if (form.AltMembraneLocationId != form.MainLocationId && form.AltMembraneLocationId != form.AltMainLocationId && form.AltMembraneLocationId != form.MembraneLocationId)
            {
                createStock.StockId = 0;
                createStock.LocationId = form.AltMembraneLocationId;
                createStock.Location = form.AltMembraneLocation;
                await _stocksService.AddAsync(createStock);
            }

            //setting the name and date
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();
            await _nirfInventoryService.AddAsync(form);

            await EmailNirfCreator(form.NirfFormId, "Inventory");
            var key = form.NirfFormId;

            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {
            var key = form.NirfFormId;
            TempData["CreateError"] = "Edit Could Not Save";
            return RedirectToAction("Edit", new { id = key });
        }
    }

    /// <summary>
    /// once the parameter portion is finished it gets saved here
    /// </summary>
    /// <param name="nirfForm">nirf form and parameters</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ProductionManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignParameters(
        [Bind(
            "NirfParameters"
        )]
        NirfViewModel nirfForm, string FontList, int TimeHour, int TimeMin, int TimeSec, string TempList
    )
    {
        if (nirfForm == null || nirfForm.NirfParameters == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        var modHour = TimeHour % 24;
        var day = (int)Math.Floor((decimal)TimeHour / 24);
        DateTime date = new DateTime();
        date = date.AddMinutes(TimeMin);
        date = date.AddHours(modHour);
        date = date.AddSeconds(TimeSec);
        date = date.AddDays(day);
        // getting the form from the viewmodel
        var form = nirfForm.NirfParameters;
        try
        {
            var getNirfPara = _nirfParametersService.Get(
                x => x.NirfFormId == nirfForm.NirfParameters.NirfFormId
            );

            if (getNirfPara != null)
            {
                await _nirfParametersService.RemoveAsync(getNirfPara.NirfParametersId);
            }
            //setting the name and date
            form.TimeToComplete = date;
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();
            if (TempList == "1")
            {
                form.IsFahrenheit = true;
            }
            else
            {
                form.IsFahrenheit = false;
            }
            var getfont = _fontService.Get(x => x.FontId.ToString() == FontList);

            if (getfont != null)
            {
                form.Font = getfont;
                form.FontId = getfont.FontId;
            }

            await _nirfParametersService.AddAsync(form);

            await EmailNirfCreator(form.NirfFormId, "Parameters");
            var key = form.NirfFormId;

            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {
            var key = form.NirfFormId;
            TempData["CreateError"] = "Parameters Could Not Save";
            return RedirectToAction("Edit", new { id = key });
        }
    }

    /// <summary>
    /// once the packaging portion is finished it gets saved here
    /// </summary>
    /// <param name="nirfForm">nirf form and packaging</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ProductionManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignPackaging(
        [Bind(
            "NirfPackagings"
        )]
        NirfViewModel nirfForm
    )
    {
        if (nirfForm == null || nirfForm.NirfPackagings == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        // getting the form from the viewmodel
        var form = nirfForm.NirfPackagings;
        try
        {
            var getNirfPack = _nirfPackagingService.Get(x => x.NirfFormId == nirfForm.NirfPackagings.NirfFormId);
            if (getNirfPack != null)
            {
                await _nirfPackagingService.RemoveAsync(getNirfPack.NirfPackagingId);
            }
            //setting the name and date
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();
            await _nirfPackagingService.AddAsync(form);

            var key = form.NirfFormId;
            await EmailNirfCreator(form.NirfFormId, "Packaging");
            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {
            var key = form.NirfFormId;
            TempData["CreateError"] = "Nirf Packaging Could Not Save";
            return RedirectToAction("Edit", new { id = key });
        }
    }

    /// <summary>
    /// once the forecasting portion is finished it gets saved here
    /// </summary>
    /// <param name="nirfForm">nirf form and forecasting</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignForecasting(
        [Bind("NirfForecastings")]
        NirfViewModel nirfForm
    )
    {
        if (nirfForm == null || nirfForm.NirfForecastings == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        // getting the form from the viewmodel
        var form = nirfForm.NirfForecastings;
        try
        {
            var getNirfFore = _nirfForecastingService.Get(x => x.NirfFormId == nirfForm.NirfForecastings.NirfFormId);
            if (getNirfFore != null)
            {
                await _nirfForecastingService.RemoveAsync(getNirfFore.NirfForecastingId);

            }
            //setting the name and date
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();

            await _nirfForecastingService.AddAsync(form);

            EmailNirfCreator(form.NirfFormId, "Forecasting");
            var key = form.NirfFormId;

            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {
            var key = form.NirfFormId;
            TempData["CreateError"] = "Nirf Forecasting Could Not Save";
            return RedirectToAction("Edit", new { id = key });
        }
    }

    /// <summary>
    /// once the shipping section is done it gets saved here
    /// </summary>
    /// <param name="nirfForm">nirf forms and shipping</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignShipping(
        [Bind(
            "NirfShippings, NirfShippingProvider"
        )]
        NirfViewModel nirfForm
    )
    {
        if (nirfForm == null || nirfForm.NirfShippings == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        // getting the form from the viewmodel
        var form = nirfForm.NirfShippings;
        try
        {
            var map = nirfForm.NirfShippingProvider.Where(x => x.ErrorMessage != null && x.ErrorMessage != "true").ToList();
            if (map.Count == 0)
            {
                TempData["Error"] = "No Shipping Provider Selected";
                return RedirectToAction("Edit", new { id = _nirfDbFull.NirfShippings.NirfFormId });
            }

            var getNirfShip = _nirfShippingService.Get(x => x.NirfFormId == nirfForm.NirfShippings.NirfFormId);
            if (getNirfShip != null)
            {
                await _nirfShippingService.RemoveAsync(getNirfShip.NirfShippingId);
            }
            //setting the name and date
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();
            form.NirfShippingProvider = map;
            await _nirfShippingService.AddAsync(form);

            EmailNirfCreator(form.NirfFormId, "Shipping");

            var key = form.NirfFormId;

            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {
            var key = form.NirfFormId;
            TempData["CreateError"] = "Nirf Shipping Could Not Save";
            return RedirectToAction("Edit", new { id = key });

        }
    }

    /// <summary>
    /// once the vendor mapping is finished it gets saved here
    /// </summary>
    /// <param name="nirfForm">nirf forms and vendor mapping</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SignVendorMapping(
        [Bind(
            "NirfVendorMapping"
        )]
        NirfViewModel nirfForm
    )
    {
        if (nirfForm == null || nirfForm.NirfVendorMapping == null)
        {
            return RedirectToAction("Index");//, new { id = key });
        }
        // getting the form from the viewmodel
        var form = nirfForm.NirfVendorMapping;
        try
        {
            var getVendMap = _nirfVendorMappingService.Get(x => x.NirfFormId == nirfForm.NirfVendorMapping.NirfFormId);
            var getProduct = _productService.Get(x => x.ProductId == _nirfDbFull.NirfProductMapping.ProductId);

            if (getVendMap != null)
            {
                var getVendor = _productVendorMappingService.Get(x => x.ProductId == getProduct.ProductId && x.VendorId == form.VendorId);

                await _productVendorMappingService.RemoveAsync(getVendor.ProductVendorMappingId);
                await _nirfVendorMappingService.RemoveAsync(getVendMap.NirfVendorMappingId);
            }

            var vendorMapping = new ProductVendorMapping()
            {
                ProductId = getProduct.ProductId,
                Product = getProduct,
                VendorId = form.VendorId,
                Cost = getProduct.Cost,
                isPrimaryVendor = false,
                VendorSku = _nirfDbFull.NirfProductMapping.NirfForm.SellersProductSku,
                IsActive = true
            };

            await _productVendorMappingService.AddAsync(vendorMapping);

            //setting the name and date
            form.SignedBy = User.Identity.Name;
            form.SignedOn = _date;
            form.AspUserId = GetAspUser();
            await _nirfVendorMappingService.AddAsync(form);

            EmailNirfCreator(form.NirfFormId, "Vendor Mapping");

            var key = form.NirfFormId;

            return RedirectToAction("Edit", new { id = key });
        }
        catch
        {

            var key = form.NirfFormId;
            TempData["CreateError"] = "Nirf Vendor Mapping Could Not Save";
            return RedirectToAction("Edit", new { id = key });
        }
    }

    /// <summary>
    /// adds product mapping to the nirf form and varient skus
    /// </summary>
    /// <param name="nirfForm">nirf forms and product mapping</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> AddNirfProductMapping([Bind("NirfVariations, NirfProductMapping, NirfProductMapping.Product,NirfProductMapping.Product.Sku, NirfProducts")] NirfViewModel nirfForm)
    {
        if (nirfForm?.NirfProductMapping == null)
        {
            return RedirectToAction("Index");
        }

        var nirfFormId = nirfForm.NirfProductMapping.NirfFormId;

        var nProdMap = await _nirfProductMappingService.GetAsync(
            x => x.NirfFormId == nirfFormId,
            includes: [x => x.Product, x => x.NirfForm]
            );

        try
        {
            // Check if Vendor Mapping exists
            if (!(await _nirfVendorMappingService.IsExistsAsync(x => x.NirfFormId == nirfFormId)))
            {
                TempData["Error"] = "Vendor Mapping must be completed before adding variations";
                return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
            }

            // Check for duplicate SKUs in the form
            var duplicateSkus = nirfForm.NirfProducts
                .GroupBy(p => p.Sku)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSkus.Any())
            {
                TempData["Error"] = "Duplicate Variant SKU(s): " + string.Join(", ", duplicateSkus);
                return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
            }

            var newVariants = nirfForm.NirfProducts.Where(x => x.ProductId <= 0).ToList();

            foreach (var product in newVariants)
            {
                // Check if variant SKU matches the original SKU
                if (product.Sku == nProdMap.Product.Sku)
                {
                    TempData["Error"] = "Variant SKU cannot be the same as original SKU";
                    return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
                }

                // Check if SKU already exists in the database
                if (await _productService.IsExistsAsync(x => x.Sku == product.Sku))
                {
                    TempData["Error"] = "Variant SKU cannot be the same as an existing SKU";
                    return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
                }
            }

            // Batch process new variants
            foreach (var product in newVariants)
            {
                // Add the new product
                var newProduct = new Product
                {
                    Sku = product.Sku,
                    Description = product.Description,
                    Length = nProdMap.Product.Length,
                    Width = nProdMap.Product.Width,
                    Height = nProdMap.Product.Height,
                    Cost = nProdMap.Product.Cost,
                    FulfillmentCost = nProdMap.Product.FulfillmentCost,
                    AltItemNumber = nProdMap.Product.AltItemNumber,
                    WeightAmount = nProdMap.Product.WeightAmount,
                    WeightUnit = nProdMap.Product.WeightUnit,
                    DimensionalUnit = nProdMap.Product.DimensionalUnit,
                    IsActive = true
                };

                await _productService.AddAsync(newProduct);
                var addedProduct = await _productService.GetAsync(x => x.Sku == newProduct.Sku);

                // Add product mapping
                await _nirfProductMappingService.AddAsync(new NirfProductMapping
                {
                    NirfFormId = nirfFormId,
                    Product = addedProduct,
                    ProductId = addedProduct.ProductId
                });

                // Add product vendor mapping
                var vendor = await _nirfVendorMappingService.GetAsync(x => x.NirfFormId == nirfFormId);

                await _productVendorMappingService.AddAsync(new ProductVendorMapping
                {
                    ProductId = addedProduct.ProductId,
                    Product = addedProduct,
                    VendorId = vendor.VendorId,
                    Cost = nProdMap.Product.Cost,
                    isPrimaryVendor = false,
                    VendorSku = nProdMap.NirfForm.SellersProductSku,
                    IsActive = true
                });
            }

            return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
        }
        catch
        {
            TempData["CreateError"] = "Nirf Product Mapping Could Not Save";
            return RedirectToAction("Edit", new { id = nProdMap.NirfFormId });
        }
    }

    /// <summary>
    /// once all sections are done user can click to finish the nirf form, it wraps up the form and finishes creating everything it needs to
    /// </summary>
    /// <param name="finishForm">nirf form id</param>
    /// <returns></returns>
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> FinishForm(
        [Bind("NirfForms, NirfFormId, NirfProductMapping")] NirfViewModel finishForm
    )
    {
        //grab the form
        var form = _nirfFormService.Get(x => x.NirfFormId == finishForm.NirfProductMapping.NirfForm.NirfFormId);

        var query = _nirfFormService.GetAllNirfFormIdById(form.NirfFormId);

        //Check to see if there are any departments not done yet.
        if (query == null || !query.Any())
        {
            TempData["Error"] = "Not all departments have completed the form!";
            return RedirectToAction("Edit", new { id = form.NirfFormId });
        }
        try
        {


            //set the status to completed
            form.CompletedDate = _date;
            form.NirfStatus = NirfForm.Status.Completed;
            await _nirfFormService.UpdateAsync(form);

            //add the product to the DB
            var getProduct = _productService.Get(x => x.ProductId == finishForm.NirfProductMapping.ProductId);
            var getForeCat = _nirfForecastingService.Get(x => x.NirfFormId == form.NirfFormId);
            getProduct.LeadTime = getForeCat.LeadTime;

            var getLocation = _nirfInventoryService.Get(
                x => x.NirfFormId == form.NirfFormId,
                [
                    x => x.MainLocation,
                    x => x.AltMainLocation,
                    x => x.MembraneLocation,
                    x => x.AltMembraneLocation
                ]
            );
            var getVendorMapping = _nirfVendorMappingService.Get(x => x.NirfFormId == form.NirfFormId);

            await _productService.UpdateAsync(getProduct);

            var _Height = _nirfDbFull.NirfPackagings.Height;
            var _Length = _nirfDbFull.NirfPackagings.Length;
            var _Width = _nirfDbFull.NirfPackagings.Width;
            var getVendormapping = _productVendorMappingService.Get(
                x => x.ProductId == finishForm.NirfProductMapping.ProductId
                    && x.VendorId == getVendorMapping.VendorId
                    && x.IsActive
            );

            if (getVendormapping != null)
            {
                await _productContainerService.UpdateAsync(new ProductContainer
                {
                    ContainerCost = _nirfDbFull.NirfProductMapping.Product.Cost,
                    ContainerDiminsions = ContainerDiminsions.Inches,
                    ContainerQuantity = (int)_nirfDbFull.NirfPackagings.UnitsPerContainer,
                    Height = _Height,
                    Length = _Length,
                    Width = _Width,
                    IsActive = true,
                    ModifyByUser = this.User.Identity.Name,
                    ModifyDate = _date,
                    ProductVendorMappingId = getVendormapping.ProductVendorMappingId,

                });
            }

            var getSkuVarients = _nirfProductMappingService.GetList(
                x => x.NirfFormId == finishForm.NirfProductMapping.NirfForm.NirfFormId &&
                    x.Product.ProductId != _nirfDbFull.NirfProductMapping.Product.ProductId,
                includes: [
                    x => x.Product,
                    x => x.NirfForm
                ]
            );

            foreach (var varient in getSkuVarients)
            {
                getVendormapping = _productVendorMappingService.Get(
                    x => x.ProductId == varient.Product.ProductId
                        && x.VendorId == getVendorMapping.VendorId
                        && x.IsActive
                );

                if (getVendormapping != null)
                {
                    getVendormapping.LeadTime = getProduct.LeadTime;
                    await _productVendorMappingService.UpdateAsync(getVendormapping);

                    var createContainerVarient = new ProductContainer()
                    {
                        ContainerCost = varient.Product.Cost,
                        ContainerDiminsions = ContainerDiminsions.Inches,
                        ContainerQuantity = 1,
                        Height = _Height,
                        Length = _Length,
                        Width = _Width,
                        IsActive = true,
                        ModifyByUser = this.User.Identity.Name,
                        ModifyDate = _date,
                        ProductVendorMappingId = getVendormapping.ProductVendorMappingId,
                    };
                    if (varient.Product.DimensionalUnit == DimensionalUnit.Inches)
                    {
                        createContainerVarient.ContainerDiminsions = ContainerDiminsions.Inches;
                    }
                    if (varient.Product.DimensionalUnit == DimensionalUnit.Centimeters)
                    {
                        createContainerVarient.ContainerDiminsions = ContainerDiminsions.Centimeters;
                    }
                    if (varient.Product.DimensionalUnit == DimensionalUnit.Feet)
                    {
                        createContainerVarient.ContainerDiminsions = ContainerDiminsions.Feet;
                    }
                    if (varient.Product.DimensionalUnit == DimensionalUnit.Meters)
                    {
                        createContainerVarient.ContainerDiminsions = ContainerDiminsions.Meters;
                    }

                    await _productContainerService.UpdateAsync(createContainerVarient);
                }

                varient.Product.LeadTime = getProduct.LeadTime;
                await _productService.UpdateAsync(varient.Product);

                if (getLocation != null)
                {
                    var MainLocation = _locationService.Get(x => x.LocationId == getLocation.MainLocation.LocationId);
                    var AltMainLocation = _locationService.Get(x => x.LocationId == getLocation.AltMainLocation.LocationId);
                    var MembraneLocation = _locationService.Get(x => x.LocationId == getLocation.MembraneLocation.LocationId);
                    var AltMembraneLocation = _locationService.Get(x => x.LocationId == getLocation.AltMembraneLocation.LocationId);

                    bool getExternal = false;
                    foreach (var department in _nirfDbFull.NirfProductMapping.Product.Departments)
                    {
                        if (department.DepartmentName == "External")
                        {
                            getExternal = true;
                        }
                    }
                    var createStock = new Stock
                    {
                        ProductId = (int)varient.ProductId,
                        LocationId = MainLocation.LocationId,
                        Location = MainLocation,
                        TotalAvailable = 0,
                        RecentlyReadded = false,
                        IsPrimary = false,
                        ModifyByUser = this.User.Identity.Name,
                        ModifyDate = _date,
                        BeingCounted = false,
                        LastCounted = _date,
                        IsExternal = getExternal,
                    };

                    await _stocksService.AddAsync(createStock);

                    if (AltMainLocation.LocationId != MainLocation.LocationId)
                    {
                        createStock.StockId = 0;
                        createStock.LocationId = AltMainLocation.LocationId;
                        createStock.Location = AltMainLocation;
                        await _stocksService.AddAsync(createStock);
                    }
                    if (MembraneLocation.LocationId != MainLocation.LocationId && MembraneLocation.LocationId != AltMainLocation.LocationId)
                    {
                        createStock.StockId = 0;
                        createStock.LocationId = MembraneLocation.LocationId;
                        createStock.Location = MembraneLocation;

                        await _stocksService.AddAsync(createStock);
                    }
                    if (AltMembraneLocation.LocationId != MainLocation.LocationId && AltMembraneLocation.LocationId != AltMainLocation.LocationId && AltMembraneLocation.LocationId != MembraneLocation.LocationId)
                    {
                        createStock.StockId = 0;
                        createStock.LocationId = AltMembraneLocation.LocationId;
                        createStock.Location = AltMembraneLocation;

                        await _stocksService.AddAsync(createStock);
                    }
                }

            }

            //Create product vendor mapping


            //check if there is any set thumbnails associated to this form

            if (!_nirfImageMappingService.IsExists(x => x.IsThumbnail && x.NirfFormId == form.NirfFormId))
                return RedirectToAction("Index");


            var map = _nirfImageMappingService.Get(x => x.IsThumbnail && x.NirfFormId == form.NirfFormId);

            var file = _filesService.Get(x => x.FileId == map.FileId);

            //resize image and create a map
            if (file != null && file.Content != null)
            {
                //resize the image from the files and create a new file for the thumbnail
                using MemoryStream ms = new(file.Content);
                using System.Drawing.Image fullSize = System.Drawing.Image.FromStream(ms);
                using System.Drawing.Image newSize = fullSize.GetThumbnailImage(100, 100, null, IntPtr.Zero);
                using MemoryStream Result = new();
                newSize.Save(Result, System.Drawing.Imaging.ImageFormat.Png);

                file.IsThumbnail = false;
                file.IsDetailed = true;

                var thumbnailFile = new Files()
                {
                    FileName = file.FileName,
                    FileType = file.FileType,
                    IsThumbnail = true,
                    Content = Result.ToArray(),
                    ContentType = file.ContentType,
                    ProductId = _nirfDbFull.NirfProductMapping.Product.ProductId,
                };

                await _filesService.AddAsync(thumbnailFile);
                await _filesService.UpdateAsync(file);

                //create thumbnail mapping from the original but now resized image
                var thumbnailImageProductMap = new ProductFilesMappings()
                {
                    FileId = thumbnailFile.FileId,
                    IsDetailedImage = false,
                    IsThumbnail = true,
                    ProductId = _nirfDbFull.NirfProductMapping.Product.ProductId
                };

                await _filesService.UpdateAsync(file);

                await _productFilesMappingsService.AddAsync(thumbnailImageProductMap);

                //create the detailed photo map from the original image from the create / edit page.
                var detailThumbnail = new ProductFilesMappings()
                {
                    FileId = file.FileId,
                    IsDetailedImage = true,
                    IsThumbnail = false,
                    ProductId = _nirfDbFull.NirfProductMapping.Product.ProductId
                };

                await _productFilesMappingsService.AddAsync(detailThumbnail);
            }
        }
        catch
        {
            return RedirectToAction("Edit", new { id = form.NirfFormId });
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// cancel form, if the nirf form needs to be cancelled, everything is deleted or marked inactive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> CancelForm(int? id)
    {
        //grab the form
        var form = _nirfFormService.Get(x => x.NirfFormId == id);

        if (form == null || form.NirfStatus == NirfForm.Status.Completed)
        {
            TempData["Error"] = "Nirf Form is cant be cancelled";
            return RedirectToAction("Details", new { id });
        }
        //set the status to canceled
        form.CompletedDate = DateTime.Now;
        form.NirfStatus = NirfForm.Status.Cancelled;

        // add the product to the DB
        await _nirfFormService.UpdateAsync(form);

        var getNirfProductsMapping = _nirfProductMappingService.GetList(
            x => x.NirfFormId == id,
            null,
            [
                x => x.Product
            ]
        );

        // Extract product IDs from mappings
        var productIds = getNirfProductsMapping.Select(x => x.ProductId).ToList();

        // Retrieve products related to this form (executed in-memory with AsEnumerable)
        var getProducts = _productService.GetList(x => productIds.Contains(x.ProductId)).AsEnumerable();

        foreach (var nirfproduct in getNirfProductsMapping)
        {
            nirfproduct.Product.IsActive = false;
            var getPVM = _productVendorMappingService.Get(x => x.ProductId == nirfproduct.ProductId && x.IsActive);

            if (getPVM != null)
            {
                try
                {
                    await _productVendorMappingService.RemoveAsync(getPVM.ProductVendorMappingId);
                }
                catch
                {

                }
            }
            var getStock = _stocksService.GetList(x => x.ProductId == nirfproduct.ProductId);

            if (getStock != null)
            {
                foreach (var stock in getStock)
                {
                    try
                    {
                        await _stocksService.RemoveAsync(stock.StockId);
                    }
                    catch
                    {
                        await _stocksService.UpdateAsync(stock);
                    }
                }
            }
        }
        foreach (var product in getProducts)
        {
            product.IsActive = false;
            await _productService.UpdateAsync(product);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    ///deletes the image from the files for the edit page.
    /// </summary>
    /// <param name="FileId"></param>
    /// <param name="NirfFormId"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> DeleteImage(int FileId, int NirfFormId)
    {
        var mapp = _nirfImageMappingService.Get(
            x => x.NirfFormId == NirfFormId && x.FileId == FileId
        );

        var file = _filesService.Get(x => x.FileId == FileId);

        if (mapp != null)
            await _nirfImageMappingService.RemoveAsync(mapp.NirfImageMappingId);
        if (file != null)
            await _filesService.RemoveAsync(file.FileId);

        return RedirectToAction("Edit", new { id = NirfFormId });
    }

    /// <summary>
    /// adds the ability for users to upload more images after the initial creation of the Nirf Form.
    /// </summary>
    /// <param name="nirfFormId"></param>
    /// <param name="newImages"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> UploadImages(int nirfFormId, IFormFile[] newImages)
    {
        var files = new List<Files>();
        if (newImages != null)
        {
            //create File objects(yes multiple)
            foreach (var img in newImages)
            {
                var file = new Files
                {
                    FileName = Path.GetFileName(img.FileName),
                    ContentType = img.ContentType,
                    FileType = FileType.Image,
                    ProductId = null,
                    Product = null,
                    IsThumbnail = false,
                    IsDetailed = false
                };

                using var reader = new BinaryReader(img.OpenReadStream());
                file.Content = reader.ReadBytes((int)img.Length);
                //add the new file to the list of files
                files.Add(file);
            }

            //add each file from the list to the DB then add that new ID of files to the mapping table
            foreach (var img in files)
            {
                //get the file id
                await _filesService.AddAsync(img);

                //create a new mapping with all the goodies made above
                var newMapping = new NirfImageMapping()
                {
                    FileId = img.FileId,
                    NirfFormId = nirfFormId,
                    IsThumbnail = false
                };
                await _nirfImageMappingService.AddAsync(newMapping);
            }
        }

        return RedirectToAction("Edit", new { id = nirfFormId });
    }

    /// <summary>
    /// sets the thumbnail image of the nirf form
    /// </summary>
    /// <param name="FileId"></param>
    /// <param name="NirfFormId"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> SetThumbnail(int FileId, int NirfFormId)
    {
        //Grab the image row in the mapping table
        var mappingRow = _nirfImageMappingService.Get(x => x.NirfFormId == NirfFormId && x.FileId == FileId);

        //Check if there are any others in the table that have been set as the thumbnail
        var thumbnails = _nirfImageMappingService.GetList(x => x.NirfFormId == NirfFormId && x.IsThumbnail);
        if (thumbnails.Any())
        {
            foreach (var thumbnail in thumbnails)
            {
                //set all the thumbnails to false
                thumbnail.IsThumbnail = false;
                await _nirfImageMappingService.UpdateAsync(thumbnail);
            }
        }

        //set the new image as the thumbnail.
        if (mappingRow != null)
        {
            mappingRow.IsThumbnail = true;
            await _nirfImageMappingService.UpdateAsync(mappingRow);
        }

        return RedirectToAction("Edit", new { id = NirfFormId });
    }

    /// <summary>
    /// resets the view data
    /// </summary>
    public void ResetViewData()
    {
        var vendors = _vendorService.GetAll();
        ViewData["VendorList"] = new SelectList(vendors, "VendorId", "VendorName");
    }

    /// <summary>
    /// gets the shipping provider list
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IActionResult GetShippingProviderlist(int? id)
    {
        var getMappings = _nirfShippingService.Get(
            x => x.NirfFormId == id,
            [x => x.NirfShippingProvider]
        );
        var providerIds = "";
        foreach (var item in getMappings.NirfShippingProvider)
        {
            providerIds += "," + item.ShippingProviderId;

        }
        var ShippingProviders = _shippingProviderService.GetList(
            x => !providerIds.Contains(x.ShippingProviderId.ToString())
        );
        var myList = new SelectList(
            ShippingProviders,
            "ShippingProviderId",
            "ShippingProviderName"
        );
        return Json(myList);
    }

    /// <summary>
    /// downloads the excel file of the nirf form information
    /// </summary>
    /// <param name="id"></param>
    /// <param name="nirfForm"></param>
    /// <returns></returns>
    public IActionResult DownloadExcel(
        int id,
        [Bind("NirfVariations, NirfForms, NirfFormId, Sku, Description")]
    NirfViewModel nirfForm
    )
    {
        // Fetching data similar to the original code
        _nirfDbFull = new NirfViewModel
        {
            NirfForms = _nirfFormService.Get(x => x.NirfFormId == id),
            NirfProductMapping = _nirfProductMappingService.Get(
                x => x.NirfFormId == id,
                [
                    x => x.Product,
                    x => x.NirfForm,
                    x => x.Product.Departments
                ]
            ),
            // Additional fetches for other related data
            NirfForecastings = _nirfForecastingService.Get(x => x.NirfFormId == id),
            NirfInventories = _nirfInventoryService.Get(x => x.NirfFormId == id),
            NirfPackagings = _nirfPackagingService.Get(x => x.NirfFormId == id),
            NirfShippings = _nirfShippingService.Get(
                x => x.NirfFormId == id,
                [x => x.NirfShippingProvider]
            ),
            NirfParameters = _nirfParametersService.Get(
                x => x.NirfFormId == id,
                [x => x.Font]
            ),
            NirfVendorMapping = _nirfVendorMappingService.Get(x => x.NirfFormId == id),
            NirfImageMapping = _nirfImageMappingService.GetList(
                x => x.NirfFormId == id,
                includes: [x => x.Files]
            )
        };

        if (_nirfDbFull.NirfInventories == null || _nirfDbFull.NirfParameters == null ||
            _nirfDbFull.NirfForms == null || _nirfDbFull.NirfPackagings == null ||
            _nirfDbFull.NirfShippings == null || _nirfDbFull.NirfProductMapping == null)
        {
            return RedirectToAction("Details", new { id });
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Create the Sheets collection in the Workbook
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Nirf Form" };
                sheets.Append(sheet);

                // Get the sheet data
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header and data rows (this replaces ClosedXML `Cell` operations)
                AddCell(sheetData, 1, 1, "Seller Sku", workbookPart, true);
                AddCell(sheetData, 2, 1, _nirfDbFull.NirfForms.SellersProductSku, workbookPart);
                AddCell(sheetData, 3, 1, "Description", workbookPart, true);
                AddCell(sheetData, 4, 1, _nirfDbFull.NirfProductMapping.Product.Description, workbookPart);

                AddCell(sheetData, 9, 1, "Fulfillment Cost", workbookPart, true);
                AddCell(sheetData, 10, 1, _nirfDbFull.NirfProductMapping.Product.FulfillmentCost.ToString(), workbookPart);

                AddCell(sheetData, 1, 2, "Sku", workbookPart, true);
                AddCell(sheetData, 2, 2, _nirfDbFull.NirfProductMapping.Product.Sku, workbookPart);

                // Add more rows as needed similarly...

                // Handle images (replacing ClosedXML's AddPicture)
                AddImagesToWorksheet(document, worksheetPart, _nirfDbFull.NirfImageMapping);

                workbookPart.Workbook.Save();
            }

            var content = memoryStream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NirfForm_" + _nirfDbFull.NirfProductMapping.Product.Sku + ".xlsx");
        }
    }

    private void AddCell(SheetData sheetData, uint rowIndex, uint colIndex, string text, WorkbookPart workbookPart, bool isHeader = false)
    {
        Row row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex) ?? new Row { RowIndex = rowIndex };
        Cell cell = new Cell { CellReference = ExcelFileExtensions.GetCellReference((int)colIndex, rowIndex), DataType = CellValues.String, CellValue = new CellValue(text) };

        if (isHeader)
        {
            ExcelFileExtensions.ApplyHeaderStyle(cell, workbookPart);
        }

        row.Append(cell);
        sheetData.Append(row);
    }

    private void AddImagesToWorksheet(SpreadsheetDocument document, WorksheetPart worksheetPart, IEnumerable<NirfImageMapping> images)
    {
        DrawingsPart drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
        worksheetPart.Worksheet.Append(new DocumentFormat.OpenXml.Spreadsheet.Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) });
        drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing();

        foreach (var imageMapping in images)
        {
            var image = _filesService.Get(x => x.FileId == imageMapping.FileId);
            using (MemoryStream ms = new MemoryStream(image.Content))
            {
                var imagePart = drawingsPart.AddImagePart(ImagePartType.Png);
                imagePart.FeedData(ms);

                // Add the image to the worksheet using the Open XML SDK drawing and positioning
                AddImageToDrawing(drawingsPart, imagePart);
            }
        }
    }

    private void AddImageToDrawing(DrawingsPart drawingsPart, ImagePart imagePart)
    {
        Xdr.NonVisualPictureProperties nvpp = new Xdr.NonVisualPictureProperties(
            new Xdr.NonVisualDrawingProperties { Id = (UInt32Value)1U, Name = "Picture" },
            new Xdr.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true }));

        Xdr.BlipFill blipFill = new Xdr.BlipFill(
            new A.Blip { Embed = drawingsPart.GetIdOfPart(imagePart) },
            new A.Stretch(new A.FillRectangle()));

        Xdr.ShapeProperties sp = new Xdr.ShapeProperties(
            new A.Transform2D(new A.Offset { X = 0L, Y = 0L }, new A.Extents { Cx = 990000L, Cy = 792000L }),
            new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle });

        Xdr.Picture picture = new Xdr.Picture(nvpp, blipFill, sp);
        Xdr.TwoCellAnchor anchor = new Xdr.TwoCellAnchor(
            new Xdr.FromMarker(new Xdr.ColumnId("1"), new Xdr.ColumnOffset("0"), new Xdr.RowId("1"), new Xdr.RowOffset("0")),
            new Xdr.ToMarker(new Xdr.ColumnId("2"), new Xdr.ColumnOffset("0"), new Xdr.RowId("2"), new Xdr.RowOffset("0")),
            picture, new Xdr.ClientData());

        drawingsPart.WorksheetDrawing.Append(anchor);
    }
}