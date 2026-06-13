using BarcodeStandard;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Extensions;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using SkiaSharp;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace ERPWebApp.Controllers.Inventory;

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
    + RoleList.CustomViewOnly
    )]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ProductsController(
    IProductService productService,
    IDepartmentService departmentService,
    ISubCategoryService subCategoryService,
    IProductImageService productImageService,
    IFilesService filesService,
    IProductVendorMappingService productVendorMappingService,
    IStocksService stockService,
    ISkuCategoryService skuCategoryService,
    ISkuColorService skuColorService,
    ISkuUnitOfMeasureService skuUnitOfMeasureService,
    IProductFilesMappingsService productFilesMappingsService,
    IPurchaseOrderService purchaseOrderService,
    IProductTagService productTagService
) : Controller
{
    private readonly DateTime _now = TimeZoneInfo.ConvertTime(
        DateTime.Now,
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );

    private static readonly List<char> _letters = [.. "ABCDEFGHIJKLMNOPQRSTUVWXYZ ".ToCharArray()];

    IProductService _productService = productService;
    IDepartmentService _departmentService = departmentService;
    ISubCategoryService _subCategoryService = subCategoryService;
    IProductImageService _productImageService = productImageService;
    IFilesService _filesService = filesService;
    IProductVendorMappingService _productVendorMappingService = productVendorMappingService;
    IStocksService _stockService = stockService;
    ISkuCategoryService _skuCategoryService = skuCategoryService;
    ISkuColorService _skuColorService = skuColorService;
    ISkuUnitOfMeasureService _skuUnitOfMeasureService = skuUnitOfMeasureService;
    IProductFilesMappingsService _productFilesMappingsService = productFilesMappingsService;
    IPurchaseOrderService _purchaseOrderService = purchaseOrderService;

    private readonly IProductTagService _productTagService = productTagService;

    // GET: Products
    public async Task<IActionResult> Index(int? id)
    {
        var _productDbFull = new ProductIndexData
        {
            Products = await _productService.GetAllAsync(includes: [s => s.Departments])
        };

        if (id != null)
        {
            ViewBag.ProductId = id.Value;
            _productDbFull.Departments = _productDbFull.Products
                .Single(i => i.ProductId == id.Value)
                .Departments.ToList();
        }

        #region to retain filters
        //Commented out due to removing sessions for lack of data protection.
        //var filter = HttpContext.Session.GetObjectFromJson<Dictionary<string, string>>("ProductFilters") ?? [];

        //var department = filter.TryGetValue("department", out var dep) && !string.IsNullOrEmpty(dep)
        //    ? JsonConvert.DeserializeObject<int[]>(dep) : [];
        //var subcat = filter.TryGetValue("subcat", out var sub) && !string.IsNullOrEmpty(sub)
        //    ? JsonConvert.DeserializeObject<int[]>(sub) : [];
        //var producttag = filter.TryGetValue("producttag", out var tag) && !string.IsNullOrEmpty(tag)
        //    ? JsonConvert.DeserializeObject<string[]>(tag) : [];
        //bool isProduction = filter.TryGetValue("isProduction", out var isProd) && Convert.ToBoolean(isProd);
        #endregion

        var subCategories = await _subCategoryService.GetListAsync(
            (IQueryable<SubCategory> query) => query
                .Select(s => new SubCategory { SubCategoryId = s.SubCategoryId, Description = s.Description })
                .OrderBy(s => s.Description)
        );

        var productTagList = await _productTagService.GetListAsync(
            (IQueryable<ProductTagsRegistry> query) => query
                .Select(t => new ProductTagsRegistry { TagId = t.TagId, Description = t.Description })
                .OrderBy(t => t.Description)
        );


        List<Department> departmentList = [];

        //Commented out due to removing sessions for lack of data protection.
        //if (isProduction)
        //{
        //    departmentList = await _departmentService.GetListAsync(
        //        (IQueryable<Department> query) => query.Where(d => d.IsProduction)
        //            .Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
        //            .OrderBy(d => d.DepartmentName)
        //    );
        //}
        //else
        {
            departmentList = await _departmentService.GetListAsync(
                (IQueryable<Department> query) => query
                    .Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .OrderBy(d => d.DepartmentName)
                );
            //}

            //ViewData["SubCatList"] = new MultiSelectList(subCategories, "SubCategoryId", "Description", subcat);
            //ViewData["ProductTagList"] = new MultiSelectList(productTagList, "TagId", "Description", producttag);
            //ViewData["DepartmentList"] = new MultiSelectList(departmentList, "DepartmentId", "DepartmentName", department);
            //ViewData["isActive"] = !filter.TryGetValue("active", out var active) || Convert.ToBoolean(active);
            //ViewData["isProduction"] = isProduction;


            return View(_productDbFull);
        }
    }

    public IActionResult DeptList(string id)
    {
        var departments = new List<Department>();

        if (id == "true")
        {
            departments = _departmentService.GetList(
                (IQueryable<Department> query) => query.Where(d => d.IsProduction).Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .OrderBy(d => d.DepartmentName)
                );
        }
        else
        {
            departments = _departmentService.GetList(
                (IQueryable<Department> query) => query.Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .OrderBy(d => d.DepartmentName)
                );
        }

        return Json(new SelectList(departments, "DepartmentId", "DepartmentName"));
    }

    /// <summary>
    /// gets the products for the product datatable. it uses a combination of queries to join them into 1 product. this uses server side processing to allow
    /// the product page to load faster.  it returns data with a given page size through a json call to allow the front end to parse through
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetMyProducts")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetMyProducts(string department, string subcat, string producttag, bool active, bool isProduction)
    {
        department ??= "[]";
        subcat ??= "[]";
        if (string.IsNullOrEmpty(producttag))
        {
            producttag = "[]";
        }

        #region to retain filters
        //Commented out due to removing sessions for lack of data protection.
        //var filter = new Dictionary<string, string>
        //{
        //    {"department", department},
        //    {"subcat", subcat},
        //    {"producttag", producttag},
        //    {"active", active.ToString()},
        //    {"isProduction", isProduction.ToString()}
        //};

        //HttpContext.Session.SetObjectAsJson("ProductFilters", filter);
        #endregion

        var departmentFilterList = JsonConvert.DeserializeObject<List<int>>(department);
        var subcatFilterList = JsonConvert.DeserializeObject<List<string>>(subcat);
        var productTagFilterList = JsonConvert.DeserializeObject<List<string>>(producttag);

        if (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.ProductionManager)
            || User.IsInRole(RoleList.ShippingManager)
            || User.IsInRole(RoleList.InventoryManager)
            || User.IsInRole(RoleList.FinancialManager)
            )
        {
            ViewData["role"] = "yes";
        }
        else
        {
            ViewData["role"] = "no";
        }

        if (
            User.IsInRole(RoleList.Administrator) ||
            (User.IsInRole(RoleList.Manager) && User.IsInRole(RoleList.FinancialManager))
            )
        {
            ViewData["costP"] = "yes";
        }
        else
        {
            ViewData["costP"] = "no";
        }

        // gets form data for serverside processing
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
                "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
            ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = 0;

        // takes in the possible null departmentId. if it is null then it goes with all products regardless of department. if it is not null it hits the if check
        // and only sets the "model" full of products within the given department.
        var query = (IQueryable<Product> product) => product
            .Where(x => x.IsActive || !x.IsActive)
            .Include(x => x.Departments)
            .Include(x => x.SubCategory)
            .Include(x => x.AlternateProduct)
            .Include(i => i.ProductTags)
            .ThenInclude(t => t.Tag)
            .Include(x => x.ProductImages.Where(i => i.IsDefault));

        var myproductDB = _productService.QueryFilter(query);

        if (department != "[]")
        {
            if (subcat != "[]")
            {
                myproductDB = myproductDB.Where(
                    x => x.Departments.Any(
                        z => departmentFilterList.Any(f => f == z.DepartmentId)
                            && subcatFilterList.Any(f => f == x.SubCategory.SubCategoryId.ToString())
                        )
                    );
            }
            else
            {
                myproductDB = myproductDB.Where(
                    x => x.Departments.Any(z => departmentFilterList.Any(f => f == z.DepartmentId))
                    );
            }
        }
        else
        {
            if (subcat != "[]")
            {
                myproductDB = myproductDB.Where(
                    x => subcatFilterList.Any(f => f == x.SubCategory.SubCategoryId.ToString())
                    );
            }
            else
            {

            }
        }

        //apply product tags fileter
        if (productTagFilterList != null && productTagFilterList.Any())
        {
            myproductDB = myproductDB.Where(p => p.ProductTags != null && p.ProductTags.Any() && p.ProductTags.Any(t => productTagFilterList.Any(f => f == t.TagId.ToString())));
        }

        // Filter results based on role, with regard to external/internal user
        // Skip this filtering if Admin/Manager, to see all products
        if (!User.IsInRole(RoleList.Administrator) && !User.IsInRole(RoleList.ExternalViewer))
        {
            if (User.IsInRole(RoleList.ExternalUser))
            {
                myproductDB = myproductDB.Where(e => e.IsExternalProduct);
            }
            // handle all other roles already authorized to see this page
            else
            {
                myproductDB = myproductDB.Where(e => !e.IsExternalProduct);
            }
        }

        //grabs a list of all products with stock at the locations that are available and creates a new product with productid and totalavailable
        var siteIds = new int[] { 1, 2, 48, 49 };
        var stockQuery = (IQueryable<Stock> Query) => Query
            .Where(z => siteIds.Contains(z.Location.SiteId) && z.Location.Type != LocationType.ReceiveOnly)
            .Include(i => i.Location)
            .Include(i => i.Products)
            .Include(i => i.Products.ProductTags)
            .ThenInclude(t => t.Tag)
            .GroupBy(y => y.Products.ProductId)
            .Select(
            x => new Product
            {
                ProductId = x.Key,
                StockTotalAvailable = x.Sum(i => i.TotalAvailable)
            }
            );
        var productsWithStock = _stockService.QueryFilter(stockQuery);

        //filters through a user search string (massive for amount of columns to search through normally done behind the scenes)
        if (!String.IsNullOrEmpty(searchValue))
        {
            myproductDB = myproductDB.Where(
                p =>
                    p.Sku.Contains(searchValue)
                    || p.Description.Contains(searchValue)
                    || p.Cost.ToString().ToLower().Contains(searchValue)
                    || p.FulfillmentCost.ToString().ToLower().Contains(searchValue)
                    || p.LaborCost.ToString().ToLower().Contains(searchValue)
                    || p.OnOrder.ToString().ToLower().Contains(searchValue)
                    || p.LeadTime.ToString().ToLower().Contains(searchValue)
                    || p.Length.ToString().ToLower().Contains(searchValue)
                    || p.Width.ToString().ToLower().Contains(searchValue)
                    || p.Height.ToString().ToLower().Contains(searchValue)
                    || p.Departments.Any(x => x.DepartmentName.Contains(searchValue))
                    || (p.AltItemNumber != null && p.AltItemNumber.ToLower().Contains(searchValue))
                    || p.SubCategory.Description.Contains(searchValue)
                    || p.ProductTags.Any(x => x.Tag.Description.ToLower().Contains(searchValue))
                );
        }
        if (active)
        {
            myproductDB = myproductDB.Where(x => x.IsActive);
        }
        // sets page size for the user
        if (length != null)
        {
            if (length == "-1")
            {
                pageSize = myproductDB.Count();
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }

        if (ViewData["excel"] != null)
        {
            var teststr = ViewData["excel"];
        }

        //sets the start point to the skip point if your on a page
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        int recordsTotal = 0;
        var columndir = 0;

        //column sort direction
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        myproductDB = myproductDB.OrderBy(x => x.Sku);
                        break;
                    case "SubCategory.Description":
                        myproductDB = myproductDB.OrderBy(x => x.SubCategory.Description);
                        break;
                    case "Description":
                        myproductDB = myproductDB
                            .OrderBy(x => x.Description)
                            .ThenBy(x => x.Sku);
                        break;
                    case "FulfillmentCost":
                        myproductDB = myproductDB
                            .OrderBy(x => x.FulfillmentCost)
                            .ThenBy(x => x.Sku);
                        break;
                    case "Cost":
                        myproductDB = myproductDB.OrderBy(x => x.Cost).ThenBy(x => x.Sku);
                        break;
                    case "LaborCost":
                        myproductDB = myproductDB.OrderBy(x => x.LaborCost).ThenBy(x => x.Sku);
                        break;
                    case "StockTotalAvailable":
                        columndir = 1;
                        break;
                    case "AltItemNumber":
                        myproductDB = myproductDB
                            .OrderBy(x => x.AltItemNumber)
                            .ThenBy(x => x.Sku);
                        break;
                    case "AlternateProduct":
                        myproductDB = myproductDB.OrderBy(x => x.AlternateProductId).ThenBy(x => x.Sku);
                        break;
                    case "OnOrder":
                        myproductDB = myproductDB.OrderBy(x => x.OnOrder).ThenBy(x => x.Sku);
                        break;
                    case "LeadTime":
                        myproductDB = myproductDB.OrderBy(x => x.LeadTime).ThenBy(x => x.Sku);
                        break;
                    case "IsActive":
                        myproductDB = myproductDB.OrderBy(x => x.IsActive).ThenBy(x => x.Sku);
                        break;
                }
            }
            else if (sortColumnDirection == "desc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        myproductDB = myproductDB.OrderByDescending(x => x.Sku);
                        break;
                    case "SubCategory.Description":
                        myproductDB = myproductDB.OrderByDescending(
                            x => x.SubCategory.Description
                            );
                        break;
                    case "Description":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.Description)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "FulfillmentCost":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.FulfillmentCost)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "Cost":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.Cost)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "LaborCost":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.LaborCost)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "StockTotalAvailable":
                        columndir = 2;
                        break;
                    case "AltItemNumber":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.AltItemNumber)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "AlternateProduct":
                        myproductDB = myproductDB.OrderByDescending(x => x.AlternateProductId).ThenByDescending(x => x.Sku);
                        break;
                    case "OnOrder":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.OnOrder)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "LeadTime":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.LeadTime)
                            .ThenByDescending(x => x.Sku);
                        break;
                    case "IsActive":
                        myproductDB = myproductDB
                            .OrderByDescending(x => x.IsActive)
                            .ThenByDescending(x => x.Sku);
                        break;
                }
            }
        }

        //sets number of rows for the datatable
        recordsTotal = myproductDB.Count();
        var products = new List<ProductDTO>();

        if (columndir == 1)
        {
            products = (
                from fullProducts in myproductDB.ToList()
                join productsWithAmounts in productsWithStock
                on fullProducts.ProductId equals productsWithAmounts.ProductId
                into setNullforNoStockProducts
                from tempProduct in setNullforNoStockProducts.DefaultIfEmpty()
                select new ProductDTO
                {
                    ProductId = fullProducts.ProductId,
                    Sku = fullProducts.Sku,
                    Description = fullProducts.Description,
                    FulfillmentCost = fullProducts.FulfillmentCost,
                    Cost = fullProducts.Cost,
                    LaborCost = fullProducts.LaborCost,
                    AltItemNumber = fullProducts.AltItemNumber,
                    AlternateProduct = fullProducts.AlternateProduct,
                    OnOrder = fullProducts.OnOrder,
                    IsEmbroidery = fullProducts.IsEmbroidery,
                    IsEngraving = fullProducts.IsEngraving,
                    IsUv = fullProducts.IsUv,
                    IsMetal = fullProducts.IsMetal,
                    LeadTime = fullProducts.LeadTime,
                    WeightAmount = fullProducts.WeightAmount,
                    WeightUnit = fullProducts.WeightUnit,
                    IsActive = fullProducts.IsActive,
                    ModifyByUser = fullProducts.ModifyByUser,
                    ModifyDate = fullProducts.ModifyDate,
                    ModifySource = fullProducts.ModifySource,
                    Departments = fullProducts.Departments.Select(
                        x => new Department
                        {
                            DepartmentId = x.DepartmentId,
                            DepartmentName = x.DepartmentName
                        }
                        ).ToList(),
                    ProductImages = null,
                    Height = fullProducts.Height,
                    Length = fullProducts.Length,
                    Width = fullProducts.Width,
                    DimensionalUnit = fullProducts.DimensionalUnit,
                    IsExternalProduct = fullProducts.IsExternalProduct,
                    StockTotalAvailable = tempProduct?.StockTotalAvailable ?? 0,
                    ImageSrc = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].ThumbnailUrl : "",
                    ImageSrcDtl = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].FileUrl : "",
                    Permission = ViewData["role"].ToString(),
                    Costpermission = ViewData["costP"].ToString(),
                    SubCategory = fullProducts.SubCategory,
                    ProductTags = fullProducts.ProductTags != null ? fullProducts.ProductTags.Select(x => x.Tag).ToList() : new List<ProductTagsRegistry>(),
                    OverseasCost = fullProducts.OverseasCost,
                    MinInventory = fullProducts.MinInventory,
                    MaxInventory = fullProducts.MaxInventory,
                }
                ).OrderBy(x => x.StockTotalAvailable).ThenBy(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else if (columndir == 2)
        {
            products = (
                from fullProducts in myproductDB.ToList()
                join productsWithAmounts in productsWithStock
                on fullProducts.ProductId equals productsWithAmounts.ProductId
                into setNullforNoStockProducts
                from tempProduct in setNullforNoStockProducts.DefaultIfEmpty()
                select new ProductDTO
                {
                    ProductId = fullProducts.ProductId,
                    Sku = fullProducts.Sku,
                    Description = fullProducts.Description,
                    FulfillmentCost = fullProducts.FulfillmentCost,
                    Cost = fullProducts.Cost,
                    LaborCost = fullProducts.LaborCost,
                    AltItemNumber = fullProducts.AltItemNumber,
                    AlternateProduct = fullProducts.AlternateProduct,
                    OnOrder = fullProducts.OnOrder,
                    IsEmbroidery = fullProducts.IsEmbroidery,
                    IsEngraving = fullProducts.IsEngraving,
                    IsUv = fullProducts.IsUv,
                    IsMetal = fullProducts.IsMetal,
                    LeadTime = fullProducts.LeadTime,
                    WeightAmount = fullProducts.WeightAmount,
                    WeightUnit = fullProducts.WeightUnit,
                    IsActive = fullProducts.IsActive,
                    ModifyByUser = fullProducts.ModifyByUser,
                    ModifyDate = fullProducts.ModifyDate,
                    ModifySource = fullProducts.ModifySource,
                    Departments = fullProducts.Departments.Select(
                        x => new Department
                        {
                            DepartmentId = x.DepartmentId,
                            DepartmentName = x.DepartmentName
                        }
                        ).ToList(),
                    ProductImages = null,
                    Height = fullProducts.Height,
                    Length = fullProducts.Length,
                    Width = fullProducts.Width,
                    DimensionalUnit = fullProducts.DimensionalUnit,
                    IsExternalProduct = fullProducts.IsExternalProduct,
                    StockTotalAvailable = tempProduct?.StockTotalAvailable ?? 0,
                    ImageSrc = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].ThumbnailUrl : "",
                    ImageSrcDtl = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].FileUrl : "",
                    Permission = ViewData["role"].ToString(),
                    Costpermission = ViewData["costP"].ToString(),
                    SubCategory = fullProducts.SubCategory,
                    ProductTags = fullProducts.ProductTags != null ? fullProducts.ProductTags.Select(x => x.Tag).ToList() : new List<ProductTagsRegistry>(),
                    OverseasCost = fullProducts.OverseasCost,
                    MinInventory = fullProducts.MinInventory,
                    MaxInventory = fullProducts.MaxInventory,
                }
                ).OrderByDescending(x => x.StockTotalAvailable).ThenByDescending(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            myproductDB = myproductDB.Skip(skip).Take(pageSize);
            products = (
                from fullProducts in myproductDB.ToList()
                join productsWithAmounts in productsWithStock
                on fullProducts.ProductId equals productsWithAmounts.ProductId
                into setNullforNoStockProducts
                from tempProduct in setNullforNoStockProducts.DefaultIfEmpty()
                select new ProductDTO
                {
                    ProductId = fullProducts.ProductId,
                    Sku = fullProducts.Sku,
                    Description = fullProducts.Description,
                    FulfillmentCost = fullProducts.FulfillmentCost,
                    Cost = fullProducts.Cost,
                    LaborCost = fullProducts.LaborCost,
                    AltItemNumber = fullProducts.AltItemNumber,
                    AlternateProduct = fullProducts.AlternateProduct,
                    OnOrder = fullProducts.OnOrder,
                    IsEmbroidery = fullProducts.IsEmbroidery,
                    IsEngraving = fullProducts.IsEngraving,
                    IsUv = fullProducts.IsUv,
                    IsMetal = fullProducts.IsMetal,
                    LeadTime = fullProducts.LeadTime,
                    WeightAmount = fullProducts.WeightAmount,
                    WeightUnit = fullProducts.WeightUnit,
                    IsActive = fullProducts.IsActive,
                    ModifyByUser = fullProducts.ModifyByUser,
                    ModifyDate = fullProducts.ModifyDate,
                    ModifySource = fullProducts.ModifySource,
                    Departments = fullProducts.Departments.Select(
                        x => new Department
                        {
                            DepartmentId = x.DepartmentId,
                            DepartmentName = x.DepartmentName
                        }
                        ).ToList(),
                    ProductImages = null,
                    Height = fullProducts.Height,
                    Length = fullProducts.Length,
                    Width = fullProducts.Width,
                    DimensionalUnit = fullProducts.DimensionalUnit,
                    IsExternalProduct = fullProducts.IsExternalProduct,
                    StockTotalAvailable = tempProduct?.StockTotalAvailable ?? 0,
                    ImageSrc = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].ThumbnailUrl : "",
                    ImageSrcDtl = fullProducts.ProductImages.Count > 0 ? fullProducts.ProductImages[0].FileUrl : "",
                    Permission = ViewData["role"].ToString(),
                    Costpermission = ViewData["costP"].ToString(),
                    SubCategory = fullProducts.SubCategory,
                    ProductTags = fullProducts.ProductTags != null ? fullProducts.ProductTags.Select(x => x.Tag).ToList() : new List<ProductTagsRegistry>(),
                    OverseasCost = fullProducts.OverseasCost,
                    MinInventory = fullProducts.MinInventory,
                    MaxInventory = fullProducts.MaxInventory,
                }
                ).ToList();
        }

        var jsonData = new
        {
            draw = draw,
            recordsFiltered = recordsTotal,
            recordsTotal = recordsTotal,
            data = products
        };
        return Ok(jsonData);
    }

    public IActionResult GetDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var query = (IQueryable<Product> product) => product
            .Where(x => x.ProductId == id.Value)
            .Include(x => x.Departments)
            .Include(x => x.AlternateProduct)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag)
            .Select(
            x => new ProductDTO
            {
                ProductId = x.ProductId,
                SubCategoryId = x.SubCategoryId,
                SubCategory = x.SubCategory,
                Sku = x.Sku,
                Description = x.Description,
                FulfillmentCost = x.FulfillmentCost,
                Cost = x.Cost,
                OverseasCost = x.OverseasCost,
                LaborCost = x.LaborCost,
                AltItemNumber = x.AltItemNumber,
                AlternateProduct = x.AlternateProduct,
                OnOrder = x.OnOrder,
                IsEmbroidery = x.IsEmbroidery,
                IsEngraving = x.IsEngraving,
                IsMetal = x.IsMetal,
                IsUv = x.IsUv,
                LeadTime = x.LeadTime,
                WeightAmount = x.WeightAmount,
                WeightUnit = x.WeightUnit,
                IsActive = x.IsActive,
                ModifyDate = x.ModifyDate,
                ModifyByUser = x.ModifyByUser,
                ModifySource = x.ModifySource,
                Height = x.Height,
                Width = x.Width,
                Length = x.Length,
                DimensionalUnit = x.DimensionalUnit,
                IsExternalProduct = x.IsExternalProduct,
                MinInventory = x.MinInventory,
                MaxInventory = x.MaxInventory,
                StockTotalAvailable = x.StockTotalAvailable,
                StockTotalAvailableFilter = x.StockTotalAvailableFilter,
                ShippingWeightAmount = x.ShippingWeightAmount,
                ShippingWeightUnit = x.ShippingWeightUnit,
                IsShippingContainer = x.IsShippingContainer,
                ShippingLength = x.ShippingLength,
                ShippingWidth = x.ShippingWidth,
                ShippingHeight = x.ShippingHeight,
                ExpectedShipmentCost = x.ExpectedShipmentCost,
                ProductTags = x.ProductTags.Select(y => y.Tag).ToList()
            }
            );

        var product = _productService.Get(query);

        return PartialView("Details", product);
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        try
        {
            if (id == null)
            {
                return NotFound();
            }

            var query = (IQueryable<Product> product) => product
                .Where(x => x.ProductId == id)
                .Include(x => x.Departments)
                .Include(x => x.AlternateProduct)
                .Include(x => x.ProductImages)
                .ThenInclude(x => x.Files)
                .Select(
                x => new ProductDTO
                {
                    ProductId = x.ProductId,
                    SubCategoryId = x.SubCategoryId,
                    SubCategory = x.SubCategory,
                    Sku = x.Sku,
                    Description = x.Description,
                    FulfillmentCost = x.FulfillmentCost,
                    Cost = x.Cost,
                    OverseasCost = x.OverseasCost,
                    LaborCost = x.LaborCost,
                    AltItemNumber = x.AltItemNumber,
                    AlternateProduct = x.AlternateProduct,
                    OnOrder = x.OnOrder,
                    IsEmbroidery = x.IsEmbroidery,
                    IsEngraving = x.IsEngraving,
                    IsMetal = x.IsMetal,
                    IsUv = x.IsUv,
                    LeadTime = x.LeadTime,
                    WeightAmount = x.WeightAmount,
                    WeightUnit = x.WeightUnit,
                    IsActive = x.IsActive,
                    ModifyDate = x.ModifyDate,
                    ModifyByUser = x.ModifyByUser,
                    ModifySource = x.ModifySource,
                    Departments = x.Departments,
                    ProductImages = x.ProductImages,
                    Height = x.Height,
                    Width = x.Width,
                    Length = x.Length,
                    DimensionalUnit = x.DimensionalUnit,
                    IsExternalProduct = x.IsExternalProduct,
                    MinInventory = x.MinInventory,
                    MaxInventory = x.MaxInventory,
                    StockTotalAvailable = x.StockTotalAvailable,
                    StockTotalAvailableFilter = x.StockTotalAvailableFilter,
                    ShippingWeightAmount = x.ShippingWeightAmount,
                    ShippingWeightUnit = x.ShippingWeightUnit,
                    IsShippingContainer = x.IsShippingContainer,
                    ShippingLength = x.ShippingLength,
                    ShippingWidth = x.ShippingWidth,
                    ShippingHeight = x.ShippingHeight,
                    ExpectedShipmentCost = x.ExpectedShipmentCost,
                    ImageSrc = x.ProductImages.Count > 0 ? x.ProductImages[0].ThumbnailUrl : "",
                    ImageSrcDtl = x.ProductImages.Count > 0 ? x.ProductImages[0].FileUrl : "",
                    ProductTags = x.ProductTags.Select(y => y.Tag).ToList()
                }
                );

            var product = await _productService.GetAsync(query);

            if (product == null)
            {
                return NotFound();
            }

            //add alternate product object
            if (product.AlternateProductId is not null)
            {
                product.AlternateProduct = _productService.Get(x => x.ProductId == product.AlternateProductId.Value);
            }

            product.PurchaseOrders = await _purchaseOrderService.GetActivePurchaseOrdersByProductAsync(product.ProductId);
            product.ProductVendors = await _productVendorMappingService.GetListAsync(
                x => x.ProductId == product.ProductId && x.IsActive,
                includes: [x => x.Vendor]
                );

            return View(product);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
        }
    }

    // GET: Products/Create
    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        )]
    public IActionResult Create()
    {
        var orderedDepartmentList = _departmentService.GetList(
            (IQueryable<Department> query) => query.Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                .OrderBy(d => d.DepartmentName)
            );
        var subCategories = _subCategoryService.GetList(
            (IQueryable<SubCategory> query) => query.Select(s => new SubCategory { SubCategoryId = s.SubCategoryId, Description = s.Description })
            );

        ViewData["ExternalDeptId"] = orderedDepartmentList.Single(e => e.DepartmentName.Equals("External")).DepartmentId;
        ViewData["DepartmentList"] = new SelectList(
            orderedDepartmentList,
            "DepartmentId",
            "DepartmentName"
            );
        ViewData["SubCategoryList"] = new SelectList(
            subCategories,
            "SubCategoryId",
            "Description"
            );

        ViewData["Tags"] = _productTagService.GetList(
            (IQueryable<ProductTagsRegistry> query) => query.Select(t => new ProductTagsRegistry { TagId = t.TagId, Color = t.Color, Description = t.Description })
            );

        return View();
    }

    // POST: Products/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        )]
    [HttpPost]
    
    public async Task<IActionResult> Create(
        [Bind(
            @"ProductId,Sku,Description,FulfillmentCost,Cost,OverseasCost,LaborCost,AltItemNumber,AlternateProductId,OnOrder,
                LeadTime,WeightAmount,WeightUnit,Height,Length,Width,DimensionalUnit,
                IsEmbroidery,IsEngraving,IsMetal,IsUv,IsActive,SubCategoryId,IsExternalProduct,
                MinInventory,MaxInventory,ProductTag,ProductTagColor,DepartmentList, ShippingWeightAmount, ShippingWeightUnit, IsShippingContainer, ShippingLength, ShippingWidth, ShippingHeight, ExpectedShipmentCost"
            )]
        Product product,
        List<IFormFile> uploads,
        IFormFile defaultImage,
        string productTags = null
        )
    {
        try
        {
            var orderedDepartmentList = _departmentService.GetList(
                (IQueryable<Department> query) => query.Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .OrderBy(d => d.DepartmentName)
                );
            var subCategories = _subCategoryService.GetList(
                (IQueryable<SubCategory> query) => query.Select(s => new SubCategory { SubCategoryId = s.SubCategoryId, Description = s.Description })
                );

            orderedDepartmentList ??= new List<Department>();
            subCategories ??= new List<SubCategory>();

            ViewData["ExternalDeptId"] = orderedDepartmentList.SingleOrDefault(e => e.DepartmentName.Equals("External"))?.DepartmentId;
            ViewData["DepartmentList"] = new MultiSelectList(orderedDepartmentList, "DepartmentId", "DepartmentName", product.DepartmentList.ToArray());
            ViewData["SubCategoryList"] = new SelectList(subCategories, "SubCategoryId", "Description", product.SubCategoryId);

            if (ModelState.IsValid)
            {

                if (_productService.Get(x => x.Sku == product.Sku) != null)
                {
                    ModelState.AddModelError("productSUK", "Product with this Sku already exists.");
                    return View(product);
                }
                product.ModifyByUser = User.Identity.Name;
                product.ModifyDate = _now;
                product.ModifySource = "WebApp";
                product.ProductTags = null;

                if (product.DepartmentList != null)
                {
                    var splitString = product.DepartmentList.Select(x => x.ToString()).ToArray();
                    product = _departmentService.UpdateProductDepartments(splitString, product);
                }

                product = await _productService.AddAsync(product);

                var tags = new List<ProductTagsRegistry>();
                if (!string.IsNullOrEmpty(productTags)) tags = JsonConvert.DeserializeObject<List<ProductTagsRegistry>>(productTags);

                if (tags.Count > 0)
                {
                    await _productTagService.AssignProductTagRangeAsync(tags, product.ProductId);
                }

                if (defaultImage is { Length: > 0 })
                {
                    var imgUrl = await _filesService.UploadToAzureAsync(defaultImage, FileType.Image);
                    var image = new Files
                    {
                        FileName = System.IO.Path.GetFileName(defaultImage.FileName),
                        FileType = FileType.Image,
                        ContentType = defaultImage.ContentType,
                        IsThumbnail = false,
                        IsDetailed = true,
                        FileUrl = imgUrl
                    };

                    await _filesService.AddAsync(image);

                    var thumbnailUrl = await _filesService.UploadThumbnailToAzureAsync(defaultImage);

                    await _productImageService.AddAsync(new ProductImage()
                    {
                        ProductId = product.ProductId,
                        FileId = image.FileId,
                        FileUrl = image.FileUrl,
                        ThumbnailUrl = thumbnailUrl,
                        IsDefault = true
                    });
                }

                if (uploads != null && uploads.Count > 0)
                {
                    foreach (var upload in uploads)
                    {
                        if (upload is { Length: > 0 })
                        {
                            var imgUrl = await _filesService.UploadToAzureAsync(upload, FileType.Image);
                            var image = new Files
                            {
                                FileName = System.IO.Path.GetFileName(upload.FileName),
                                FileType = FileType.Image,
                                ContentType = upload.ContentType,
                                IsThumbnail = false,
                                IsDetailed = true,
                                FileUrl = imgUrl
                            };

                            await _filesService.AddAsync(image);

                            await _productImageService.AddAsync(new ProductImage()
                            {
                                ProductId = product.ProductId,
                                FileId = image.FileId,
                                FileUrl = image.FileUrl,
                                IsDefault = false
                            });
                        }
                    }
                }



                return RedirectToAction(nameof(Index));
            }

            if (product.Departments != null)
                PopulateAssignedDepartment(product);

            return View(product);
        }
        catch (RetryLimitExceededException)
        {
            ModelState.AddModelError(
                "",
                "Unable to save changes. Try again, and if the problem persists see your system administrator."
                );
        }

        return View(product);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ProductIndexData> GetProductsByDepartment(string[] departments)
    {
        var selectedContainers = new List<Product>();
        if (departments[0] == "Any")
        {
            var containersAll = await _productService.GetAllAsync(includes: [x => x.Departments]);
            selectedContainers.AddRange(containersAll);
        }
        else
        {
            var containersAllVendors = await _productService.GetListAsync(
                x => x.Departments.Any(x => x.DepartmentName.Contains(departments[0])),
                null,
                [x => x.Departments]
                );
            selectedContainers.AddRange(containersAllVendors);
        }

        var _productDbFull = new ProductIndexData
        {
            Products = selectedContainers
        };

        return _productDbFull;
    }

    // GET: Products/Edit/5
    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        + ","
        + RoleList.FinancialManager
        )]
    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["ProductDepartmentList"] = "";
        if (id == null)
        {
            return NotFound();
        }

        var query = (IQueryable<Product> product) => product
            .Where(x => x.ProductId == id)
            .Include(x => x.Departments)
            .Include(x => x.SubCategory)
            .Include(x => x.AlternateProduct)
            .Include(x => x.ProductImages)
            .ThenInclude(x => x.Files)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag);

        var product = await _productService.GetAsync(query);

        if (product == null)
        {
            return NotFound();
        }

        product.DepartmentList = product.Departments.Select(x => x.DepartmentId).ToList();
        foreach (var depts in product.Departments)
        {
            if (!ViewData["ProductDepartmentList"].Equals(""))
            {
                ViewData["ProductDepartmentList"] += ",";
            }

            ViewData["ProductDepartmentList"] += depts.DepartmentId.ToString();
        }

        var orderedDepartmentList = _departmentService.GetList(
            (IQueryable<Department> query) => query.Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                .OrderBy(d => d.DepartmentName)
            );

        ViewData["ExternalDeptId"] = orderedDepartmentList.FirstOrDefault(e => e.DepartmentName.Equals("External")).DepartmentId;
        ViewData["DepartmentList"] = new MultiSelectList(
            orderedDepartmentList,
            "DepartmentId",
            "DepartmentName",
            product.DepartmentList.ToArray()
            );

        var subCategories = _subCategoryService.GetList(
            (q) => q.Select(x => (object)new { x.SubCategoryId, x.Description })
            );
        ViewData["SubCategoryList"] = new SelectList(
            subCategories,
            "SubCategoryId",
            "Description",
            product.SubCategoryId
            );

        ViewData["Tags"] = _productTagService.GetList(
            (IQueryable<ProductTagsRegistry> query) => query.Select(t => new ProductTagsRegistry { TagId = t.TagId, Color = t.Color, Description = t.Description })
            );

        var getPVM = _productVendorMappingService.GetList(
            x => x.ProductId == product.ProductId && x.isPrimaryVendor && x.IsActive
            );

        if (getPVM.Count() > 0)
        {
            ViewData["IsPrimaryVendor"] = "yes";
        }
        else
        {
            ViewData["IsPrimaryVendor"] = "no";
        }

        return View(product);
    }

    [NonAction]
    public virtual async Task<bool> TryUpdateModelAsync(Product model)
    {
        return await TryUpdateModelAsync(
            model,
            "",
            s => s.ProductId,
            s => s.Sku,
            s => s.Description,
            s => s.FulfillmentCost,
            s => s.Cost,
            s => s.LaborCost,
            s => s.AltItemNumber,
            s => s.OnOrder,
            s => s.LeadTime,
            s => s.WeightAmount,
            s => s.WeightUnit,
            s => s.Length,
            s => s.Width,
            s => s.Height,
            s => s.DimensionalUnit,
            s => s.IsEmbroidery,
            s => s.IsEngraving,
            s => s.IsMetal,
            s => s.IsUv,
            s => s.IsActive,
            s => s.IsExternalProduct,
            s => s.MinInventory,
            s => s.MaxInventory,
            s => s.DepartmentList,
            s => s.AlternateProductId,
            s => s.OverseasCost,
            s => s.ShippingWeightAmount,
            s => s.ShippingWeightUnit,
            s => s.IsShippingContainer,
            s => s.ExpectedShipmentCost,
            s => s.ShippingHeight,
            s => s.ShippingLength,
            s => s.ShippingWidth
            );
    }

    // POST: Products/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        )]
    public async Task<IActionResult> EditAsync(
        [Bind(@"ProductId,Sku,Description,FulfillmentCost,Cost,OverseasCost,LaborCost,AltItemNumber,OnOrder,
                LeadTime,WeightAmount,WeightUnit,Height,Length,Width,DimensionalUnit,
                IsEmbroidery,IsEngraving,IsMetal,IsUv,IsActive,SubCategoryId,IsExternalProduct,
                MinInventory,MaxInventory,ProductTag,ProductTagColor,DepartmentList,AlternateProductId, ShippingWeightAmount, ShippingWeightUnit, IsShippingContainer, ExpectedShipmentCost, ShippingHeight, ShippingWidth, ShippingLength"
            )] Product product,
        int? id,
        List<IFormFile> uploads,
        IFormFile defaultImage,
        int? SubCategoryId,
        string productTags = null
        )
    {
        if (!ModelState.IsValid)
        {
            var orderedDepartmentList = _departmentService.GetList(
                (IQueryable<Department> query) => query.Select(d => new Department { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .OrderBy(d => d.DepartmentName)
                );

            ViewData["ExternalDeptId"] = orderedDepartmentList.Single(e => e.DepartmentName.Equals("External")).DepartmentId;
            ViewData["DepartmentList"] = new SelectList(
                orderedDepartmentList,
                "DepartmentId",
                "DepartmentName"
                );

            var subCategories = _subCategoryService.GetList(
                (q) => q.Select(x => (object)new { x.SubCategoryId, x.Description })
                );
            ViewData["SubCategoryList"] = new SelectList(
                subCategories,
                "SubCategoryId",
                "Description",
                product.SubCategoryId
                );

            ViewData["Tags"] = _productTagService.GetList(
                (IQueryable<ProductTagsRegistry> query) => query.Select(t => new ProductTagsRegistry { TagId = t.TagId, Color = t.Color, Description = t.Description })
                );

            View(product);
        }
        if (uploads != null)
        {
            var isInvalid = uploads.Any(x =>
            {
                return Path.GetExtension(x.FileName).ToLower() != ".jpg"
                    && Path.GetExtension(x.FileName).ToLower() != ".jpeg"
                    && Path.GetExtension(x.FileName).ToLower() != ".png";
            });

            if (isInvalid)
            {
                return RedirectToAction("Edit", new { id = id });
            }
        }

        if (defaultImage != null)
        {
            var isInvalid = Path.GetExtension(defaultImage.FileName).ToLower() != ".jpg"
                && Path.GetExtension(defaultImage.FileName).ToLower() != ".jpeg"
                && Path.GetExtension(defaultImage.FileName).ToLower() != ".png";

            if (isInvalid)
            {
                return RedirectToAction("Edit", new { id = id });
            }
        }

        var productToUpdate = _productService.Get(
            i => i.ProductId == id,
            includes: new Expression<Func<Product, object>>[]{
                i => i.Departments
            }
            );


        bool updateSuccessful = await TryUpdateModelAsync(productToUpdate);

        if (updateSuccessful)
        {
            try
            {

                var tags = new List<ProductTagsRegistry>();
                if (!string.IsNullOrEmpty(productTags)) tags = JsonConvert.DeserializeObject<List<ProductTagsRegistry>>(productTags);

                if (tags.Count > 0)
                {
                    await _productTagService.AssignProductTagRangeAsync(tags, productToUpdate.ProductId);
                }

                if (productToUpdate.DepartmentList != null)
                {
                    var splitString = productToUpdate.DepartmentList.Select(x => x.ToString()).ToArray();
                    _departmentService.UpdateProductDepartments(splitString, productToUpdate);
                }
                productToUpdate.Description = productToUpdate.Description.Trim().Normalize();
                productToUpdate.ModifyByUser = User.Identity.Name;
                productToUpdate.ModifyDate = _now;
                productToUpdate.ModifySource = "WebApp";

                if (SubCategoryId == -1)
                {
                    productToUpdate.SubCategoryId = null;
                }
                else
                {
                    productToUpdate.SubCategoryId = SubCategoryId;
                }

                await _productService.UpdateAsync(productToUpdate);

                if (defaultImage is { Length: > 0 })
                {
                    var previousimage = await _productImageService.GetAsync(x => x.ProductId == productToUpdate.ProductId && x.IsDefault);
                    if (previousimage != null)
                    {
                        await _filesService.RemoveAzureBlobAsync(previousimage.ThumbnailUrl, FileType.Thumbnail);
                        await _filesService.RemoveAzureBlobAsync(previousimage.FileUrl, FileType.Image);
                        await _productImageService.RemoveAsync(previousimage.ProductImageId);
                        await _filesService.RemoveAsync(previousimage.FileId);
                    }

                    var imgUrl = await _filesService.UploadToAzureAsync(defaultImage, FileType.Image);
                    var image = new Files
                    {
                        FileName = System.IO.Path.GetFileName(defaultImage.FileName),
                        FileType = FileType.Image,
                        ContentType = defaultImage.ContentType,
                        IsThumbnail = false,
                        IsDetailed = true,
                        FileUrl = imgUrl
                    };

                    await _filesService.AddAsync(image);

                    var thumbnailUrl = await _filesService.UploadThumbnailToAzureAsync(defaultImage);

                    await _productImageService.AddAsync(new ProductImage()
                    {
                        ProductId = productToUpdate.ProductId,
                        FileId = image.FileId,
                        FileUrl = image.FileUrl,
                        ThumbnailUrl = thumbnailUrl,
                        IsDefault = true
                    });
                }

                if (uploads != null && uploads.Count > 0)
                {
                    foreach (var upload in uploads)
                    {
                        if (upload is { Length: > 0 })
                        {
                            var imgUrl = await _filesService.UploadToAzureAsync(upload, FileType.Image);
                            var image = new Files
                            {
                                FileName = System.IO.Path.GetFileName(upload.FileName),
                                FileType = FileType.Image,
                                ContentType = upload.ContentType,
                                IsThumbnail = false,
                                IsDetailed = true,
                                FileUrl = imgUrl
                            };

                            await _filesService.AddAsync(image);

                            await _productImageService.AddAsync(new ProductImage()
                            {
                                ProductId = productToUpdate.ProductId,
                                FileId = image.FileId,
                                FileUrl = image.FileUrl,
                                IsDefault = false
                            });
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!_productService.IsExists(e => e.ProductId == productToUpdate.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        PopulateAssignedDepartment(productToUpdate);
        return View(productToUpdate);
    }

    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        )]
    public IActionResult GetDelete(int id)
    {

        var query = (IQueryable<Product> product) => product
            .Where(x => x.ProductId == id)
            .Include(x => x.Departments)
            .Include(x => x.AlternateProduct)
            .Include(x => x.ProductImages)
            .ThenInclude(x => x.Files)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag);

        var resultToRemove = _productService.Get(query);

        if (resultToRemove == null)
        {
            return NotFound();
        }

        return PartialView("Delete", resultToRemove);
    }

    // GET: Products/Delete/5
    [Authorize(
        Roles = RoleList.Administrator
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingManager
        )]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var query = (IQueryable<Product> product) => product
            .Where(x => x.ProductId == id)
            .Include(x => x.Departments)
            .Include(x => x.AlternateProduct)
            .Include(x => x.ProductImages)
            .ThenInclude(x => x.Files)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag);

        var product = await _productService.GetAsync(query);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: Products/Delete/5
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
        try
        {
            var product = await _productService.GetAsync(x => x.ProductId == id);
            var productImages = await _productImageService.GetListAsync(x => x.ProductId == id);

            if (productImages != null)
            {
                foreach (var image in productImages)
                {
                    await _filesService.RemoveAzureBlobAsync(image.FileUrl, FileType.Image);

                    if (!String.IsNullOrEmpty(image.ThumbnailUrl))
                        await _filesService.RemoveAzureBlobAsync(image.ThumbnailUrl, FileType.Thumbnail);

                    await _productImageService.RemoveAsync(image.ProductImageId);
                    await _filesService.RemoveAsync(image.FileId);
                }
            }

            await _productService.RemoveAsync(product.ProductId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex;
            return RedirectToAction("Index");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuCategory>))]
    public async Task<IEnumerable<SkuCategory>> GetSkuCategories()
    {
        return await _skuCategoryService.GetListAsync(x => x.IsActive);
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuColor>))]
    public async Task<IEnumerable<SkuColor>> GetSkuColors()
    {
        return await _skuColorService.GetListAsync(x => x.IsActive);
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<SkuUnitOfMeasure>))]
    public async Task<IEnumerable<SkuUnitOfMeasure>> GetSkuUnitOfMeasure()
    {
        return await _skuUnitOfMeasureService.GetListAsync(x => x.IsActive);
    }

    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var image = await _productImageService.GetAsync(x => x.ProductImageId == imageId);

        if (image != null)
        {
            await _filesService.RemoveAzureBlobAsync(image.FileUrl, FileType.Image);

            if (!String.IsNullOrEmpty(image.ThumbnailUrl))
                await _filesService.RemoveAzureBlobAsync(image.ThumbnailUrl, FileType.Thumbnail);

            await _productImageService.RemoveAsync(image.ProductImageId);
            await _filesService.RemoveAsync(image.FileId);
        }

        return RedirectToAction("Edit", new { id = image.ProductId });
    }

    public async Task<IActionResult> UpdateCostAndLeadTime(int? id)
    {
        var getPVM = _productVendorMappingService.Get(x => x.ProductId == id && x.isPrimaryVendor && x.IsActive);
        if (getPVM == null) return NotFound();
        var getProduct = _productService.Get(p => p.ProductId == id);

        getProduct.Cost = getPVM.Cost;
        getProduct.LeadTime = getPVM.LeadTime;

        await _productService.UpdateAsync(getProduct);

        return Ok();
    }

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
                TempData["ErrorMessage"] = "Please upload a valid Excel (.xlsx) file.";
                return RedirectToAction("Index");
            }

            var allowedProperties = GetImportProperties()
                .Select(x => x.Name)
                .ToList();

            using (var stream = new MemoryStream())
            {
                await userUpload.CopyToAsync(stream);
                stream.Position = 0;
                var failedRows = new List<int>();

                using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, true))
                {
                    var sheetData = ExcelFileExtensions.GetSheetData(document);
                    var (headers, lastcolindex) = ExcelFileExtensions.GetHeaders(document, sheetData, allowedProperties);


                    var properties = typeof(Product).GetProperties();
                    foreach (var row in sheetData.Elements<Row>().Skip(1)) // Skip header row
                    {
                        try
                        {
                            await ProcessProductRow(row, headers, properties, document);
                        }
                        catch (Exception ex)
                        {
                            failedRows.Add((int)row.RowIndex.Value);
                            ExcelFileExtensions.AppendErrorMessage(row, $"Error: {ex.Message}", document, lastcolindex);
                        }
                    }
                    document.WorkbookPart.Workbook.Save();
                }

                // Return error file if any failed rows
                if (failedRows.Count > 0)
                {
                    // Ensure all changes are saved
                    stream.Position = 0; // Rewind the stream to the beginning
                    TempData["ErrorMessage"] = "Error processing file.";

                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = "Product_Upload_Errors.xlsx"
                    };
                }
            }

            TempData["SuccessMessage"] = "Data imported successfully.";

        }
        catch (Exception ex)
        {
            var message = $"Error processing file: {ex.Message}";
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction("Index");
    }

    private async Task ProcessProductRow(Row row, Dictionary<string, int> headers, PropertyInfo[] properties, SpreadsheetDocument document)
    {
        int skuindex = headers.First(x => x.Key.Equals("sku", StringComparison.CurrentCultureIgnoreCase)).Value;
        string sku = ExcelFileExtensions.GetCellValue(document, ExcelFileExtensions.GetCellByColumnIndex(row, skuindex), typeof(string))?.ToString().Trim();
        if (string.IsNullOrEmpty(sku)) return;

        var product = await _productService.GetAsync(x => x.Sku == sku) ?? new Product { Sku = sku };
        product.ModifyByUser = User.Identity.Name;
        product.ModifyDate = _now;
        product.ModifySource = "WebApp";

        foreach (var header in headers)
        {
            string propertyName = header.Key.ToLower();
            Cell cell = ExcelFileExtensions.GetCellByColumnIndex(row, header.Value);
            object cellValue = await GetCellValueForProperty(propertyName, cell, document, properties, product);

            if (cellValue == null && IsRequiredField(propertyName, properties))
            {
                throw new Exception($"Missing required value for {propertyName}");
            }
        }

        await _productService.UpdateAsync(product);
    }

    private async Task<object> GetCellValueForProperty(string propertyName, Cell cell, SpreadsheetDocument document, PropertyInfo[] properties, Product product)
    {
        try
        {
            if (propertyName.Equals(nameof(product.SubCategory), StringComparison.CurrentCultureIgnoreCase))
            {
                var cellValue = ExcelFileExtensions.GetCellValue(document, cell, typeof(string));
                if (!string.IsNullOrEmpty(cellValue?.ToString()))
                {
                    var subCategory = await _subCategoryService.GetAsync(x => x.Description == cellValue.ToString());
                    if (subCategory != null) product.SubCategoryId = subCategory.SubCategoryId;
                    return cellValue;
                }
                return null;
            }

            if (propertyName.Equals(nameof(product.AlternateProduct), StringComparison.CurrentCultureIgnoreCase))
            {
                var cellValue = ExcelFileExtensions.GetCellValue(document, cell, typeof(string));
                if (!string.IsNullOrEmpty(cellValue?.ToString()))
                {
                    var altprod = await _productService.GetAsync(x => x.Sku == cellValue.ToString());
                    if (altprod != null) product.AlternateProductId = altprod.ProductId;
                    return cellValue;
                }
                return null;
            }

            var property = properties.FirstOrDefault(x => propertyName.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));
            if (property != null)
            {
                var cellValue = ExcelFileExtensions.GetCellValue(document, cell, property.PropertyType);
                property.SetValue(product, cellValue);

                if (propertyName.Equals(nameof(product.Cost), StringComparison.CurrentCultureIgnoreCase) && cellValue == null)
                {
                    var costProperty = properties.FirstOrDefault(p => p.Name.Equals(nameof(product.Cost), StringComparison.CurrentCultureIgnoreCase));
                    cellValue = ExcelFileExtensions.GetCellValue(document, cell, costProperty.PropertyType);
                    product.Cost = cellValue != null ? Convert.ToDecimal(cellValue) : product.Cost;
                }

                return cellValue;
            }
            return null;
        }
        catch
        {

            throw new Exception($"Invalid {propertyName}.");
        }
    }

    private static bool IsRequiredField(string propertyName, PropertyInfo[] properties)
    {
        var property = properties.FirstOrDefault(x => propertyName.Contains(x.Name));
        return property != null && Attribute.IsDefined(property, typeof(RequiredAttribute));
    }

    public IActionResult DownloadExcelTemplate()
    {
        try
        {
            var getFirstProduct = _productService.Get(x => x.IsActive == true, includes: [x => x.SubCategory, x => x.AlternateProduct]);
            List<string> propertyValues = [];
            List<string> propertyNames = [];
            List<List<string>> propertyValuesList = [];
            var properties = GetImportProperties();

            foreach (var property in properties)
            {
                var val = property.GetValue(getFirstProduct)?.ToString() ?? string.Empty;
                var name = Attribute.IsDefined(property, typeof(RequiredAttribute)) ? property.Name + " (Required)" : property.Name;
                if (property.Name.Equals(nameof(getFirstProduct.SubCategory)))
                {
                    val = getFirstProduct.SubCategory?.Description ?? string.Empty;
                }
                if (property.Name.Equals(nameof(getFirstProduct.AlternateProduct)))
                {
                    val = getFirstProduct.AlternateProduct?.Sku ?? string.Empty;
                }
                propertyNames.Add(name);
                propertyValues.Add(val);
            }

            propertyValuesList.Add(propertyValues);

            using MemoryStream ms = new();
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                ExcelFileExtensions.PrepareTemplate<Product>(propertyNames, propertyValuesList, document);
            }

            ms.Seek(0, SeekOrigin.Begin);

            return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "Products_Template.xlsx"
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating Excel template: {ex.Message}", ex);
        }
    }


    public async Task<IActionResult> DownloadBarcode(int id)
    {
        var product = await _productService.GetAsync(x => x.ProductId == id);
        using MemoryStream ms = new();
        var b = new Barcode
        {
            IncludeLabel = true,
            LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
        };
        using var bitmap = SKBitmap.FromImage(b.Encode(BarcodeStandard.Type.Code128B, product.Sku));
        bitmap.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(ms);
        string fileName = "barcode.jpg";
        return File(ms.ToArray(), "image/jpeg", fileName);
    }

    public async Task<IActionResult> ScanProductBarcode(string barcode)
    {
        var product = await _productService.GetAsync(x => x.Sku == barcode);
        if (product == null) return NotFound();

        var query = (IQueryable<Stock> s) => s.Where(
            x => x.ProductId == product.ProductId
                && x.Location.IsActive
            )
            .Include(x => x.Location)
            .Select(x => new
            {
                x.StockId,
                x.LocationId,
                x.Location.LocationName,
                x.TotalAvailable,
                x.Location.Type
            });

        var stocks = await _stockService.GetListAsync(query);

        return Ok(new
        {
            product.ProductId,
            product.Sku,
            product.Description,
            stocks
        });
    }

    [HttpPost]
    public async Task<IActionResult> UnAssignProductTag(int productId, int tagId)
    {
        await _productTagService.UnAssignProductTagAsync(productId, tagId);
        return Ok();
    }

    /// <summary>
    /// Retrun the list of products form Product table that satisfy the given query. The given query string
    /// will be searched agianst the ProductId, Description and Sku columns.
    /// </summary>
    /// <param name="queryString">query string to use when performing the search through Product table</param>
    /// <returns>List of Products that satisfy the query string</returns>
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAllProducts(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
        {
            return null;
        }

        var query = (IQueryable<Product> s) => s
            .Where(
            x => x.IsActive && (x.ProductId.ToString().Contains(queryString) || x.Description.Contains(queryString) || x.Sku.Contains(queryString))
            )
            .Select(p => new Product { ProductId = p.ProductId, Sku = p.Sku, Description = p.Description });

        var products = await _productService.GetListAsync(query);

        if (products == null || !products.Any()) return null;

        return Ok(products);
    }



    private void PopulateAssignedDepartment(Product product)
    {
        var viewModel = _departmentService.PopulateAssignedDepartment(product);
        ViewBag.Departments = viewModel;
    }

    [HttpGet]
    public IActionResult IsStockExists(int productId)
    {
        bool isAvailable = _stockService.IsExists(x => x.ProductId == productId);
        return Json(new { isAvailable });
    }

    private static List<PropertyInfo> GetImportProperties() => typeof(Product).GetProperties()
            .Where(x =>
                (
                    x.PropertyType == typeof(string) ||
                    (!x.PropertyType.IsArray && !typeof(IEnumerable).IsAssignableFrom(x.PropertyType))
                ) &&
                !x.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                !x.Name.Contains("Image", StringComparison.OrdinalIgnoreCase) &&
                !x.Name.Contains("Modify", StringComparison.OrdinalIgnoreCase)
            ).ToList();
}
