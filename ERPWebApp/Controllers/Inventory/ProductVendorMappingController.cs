using System.Linq.Expressions;
using ERPWebApp.Data.Enum;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ProductVendorMappingController : Controller
{
    private static ProductVendorMappingFilter _productVendorMapperDbFull = new();

    private readonly IProductVendorMappingService _productVendorMappingService;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;

    public ProductVendorMappingController(
        IProductVendorMappingService productVendorMappingService,
        IProductService productService,
        IVendorService vendorService
    )
    {
        _vendorService = vendorService;
        _productService = productService;
        _productVendorMappingService = productVendorMappingService;
    }

    //index page, gets the initial lists of created product vendor mappings, if there are none a blank list will be sent
    public async Task<IActionResult> Index()
    {

        var productQuery = (IQueryable<Product> P) =>
        {
            P = P.Where(x => x.IsActive);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                P = User.IsInRole(RoleList.ExternalUser) ? P.Where(x => x.IsExternalProduct) : P.Where(x => !x.IsExternalProduct);
            }

            return P.OrderBy(x => x.Sku)
                .Select(
                    x => new Product
                    {
                        ProductId = x.ProductId,
                        Sku = x.Sku + " " + x.Description
                    }
                );
        };

        var vendorQuery = (IQueryable<Vendor> V) =>
        {
            V = V.Where(x => x.IsActive);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                V = User.IsInRole(RoleList.ExternalUser) ? V.Where(x => x.IsExternal) : V.Where(x => !x.IsExternal);
            }

            return V.OrderBy(x => x.VendorId)
                .Select(
                    x => new Vendor
                    {
                        VendorId = x.VendorId,
                        VendorName = x.VendorName,
                        VendorNumber = x.VendorNumber
                    }
                );
        };

        List<Product> products = await _productService.GetListAsync(productQuery);
        List<Vendor> vendors = await _vendorService.GetListAsync(vendorQuery);

        ViewData["ProductList"] = new SelectList(products, "ProductId", "Sku");
        ViewData["VendorList"] = new SelectList(vendors, "VendorId", "VendorName");

        _productVendorMapperDbFull.Sku = new SelectList(products, "ProductId", "Sku");
        _productVendorMapperDbFull.Vendor = new SelectList(vendors, "VendorId", "VendorName");

        ViewData["ProductFirst"] = _productVendorMapperDbFull.Sku.Any() ? _productVendorMapperDbFull.Sku.First().Value : new SelectList("1", "No Data");

        ViewData["VendorFirst"] = _productVendorMapperDbFull.Vendor.Any() ? _productVendorMapperDbFull.Vendor.First().Value : new SelectList("1", "No Data");
        List<string> unitOfMeasureStrings = Enum.GetNames(typeof(UnitofMeasure)).ToList();
        List<SelectListItem> selectListItems = unitOfMeasureStrings
        .Select(s => new SelectListItem { Text = s, Value = s })
        .ToList();
        ViewData["UnitofMeasureList"] = new SelectList(selectListItems, "Value", "Text");

        List<string> termStrings = Enum.GetNames(typeof(Terms)).ToList();
        List<SelectListItem> selectTermListItems = termStrings
        .Select(t => new SelectListItem { Text = t, Value = t })
        .ToList();
        ViewData["TermsList"] = new SelectList(selectTermListItems, "Value", "Text");

        return View();
    }

    //gets the list of products for the partial product view
    public async Task<ProductVendorMappingFilter> GetProductList(string psku)
    {

        var productQuery = (IQueryable<ProductVendorMapping> P) =>
        {
            P = P.Where(x => x.IsActive)
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.Product.ProductTags)
                .ThenInclude(x => x.Tag);

            if (!string.IsNullOrEmpty(psku) && psku != "Any")
            {
                P = P.Where(x => x.ProductId.ToString().Equals(psku));
            }

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                P = User.IsInRole(RoleList.ExternalUser) ? P.Where(x => x.Product.IsExternalProduct) : P.Where(x => !x.Product.IsExternalProduct);
            }

            return P;
        };

        _productVendorMapperDbFull.ProductVendorMappingsProduct = await _productVendorMappingService.GetListAsync(productQuery) ?? new List<ProductVendorMapping>();

        return _productVendorMapperDbFull;
    }

    public IActionResult PartialViewTable()
    {
        return PartialView(
            "PartialIndex",
            _productVendorMapperDbFull.ProductVendorMappingsProduct
        );
    }

    //gets the list of vendors for the partial view
    public async Task<ProductVendorMappingFilter> GetVendorList(string vname)
    {
        var vendorQuery = (IQueryable<ProductVendorMapping> P) =>
        {
            P = P.Where(x => x.IsActive)
                .Include(x => x.Product)
                .Include(x => x.Vendor);

            if (!string.IsNullOrEmpty(vname) && vname != "All")
            {
                P = P.Where(x => x.VendorId.ToString().Equals(vname));
            }

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                P = User.IsInRole(RoleList.ExternalUser) ?
                    P.Where(x => x.Product.IsExternalProduct && x.Vendor.IsExternal) :
                    P.Where(x => !x.Product.IsExternalProduct && !x.Vendor.IsExternal);
            }

            return P;
        };

        _productVendorMapperDbFull.ProductVendorMappingsVendor = await _productVendorMappingService.GetListAsync(vendorQuery) ?? new List<ProductVendorMapping>();
        return _productVendorMapperDbFull;
    }

    public IActionResult PartialViewTableVendor()
    {
        return PartialView(
            "PartialIndexVendor",
            _productVendorMapperDbFull.ProductVendorMappingsVendor
        );
    }

    //grabs the details of the selected product vendor
    [ActionName("Details")]
    public IActionResult GetDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var resultToUpdate = _productVendorMappingService.Get(
            x => x.ProductVendorMappingId == id && x.IsActive,
            includes: new Expression<Func<ProductVendorMapping, object>>[]{
                x=>x.Product,
                x=>x.Vendor
            }
        );
        return PartialView("Details", resultToUpdate);
    }


    [Authorize(
       Roles = RoleList.Administrator
           + ","
           + RoleList.InventoryManager
           + ","
           + RoleList.ShippingManager
   )]
    public IActionResult Create()
    {
        GetProductAndVendorDetails();
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("ProductId,VendorId,isPrimaryVendor,Cost,MOQ,OrderMultiples,LeadTime,VendorSku,IsActive,Notes,IsRawMaterial,UnitofMeasure,Term")] ProductVendorMapping productVendorMapping)
    {
        if (ModelState.IsValid)
        {
            await _productVendorMappingService.AddAsync(productVendorMapping);
            return RedirectToAction(nameof(Index));
        }
        GetProductAndVendorDetails(productVendorMapping.ProductId, productVendorMapping.VendorId,productVendorMapping.UnitofMeasure, productVendorMapping.Term);
        return View(productVendorMapping);
    }

    //gets the data for the delete model
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.InventoryManager
            + ","
            + RoleList.ShippingManager
    )]
    [ActionName("Delete")]
    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var resultToRemove = _productVendorMappingService.Get(
            x => x.ProductVendorMappingId == id && x.IsActive,
            includes: new Expression<Func<ProductVendorMapping, object>>[]{
                x=>x.Product,
                x=>x.Vendor
            }
        );

        if (resultToRemove == null)
        {
            return NotFound();
        }
        return View(resultToRemove);
    }

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
            await _productVendorMappingService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (ex.InnerException is SqlException sqlException && (sqlException.Number == 547 || sqlException.Number == 2601))
            {        
                    TempData["ErrorMessage"] = "Product Vendor Mapping is used in other places. Please delete those references before deleting it.";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the record.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.InventoryManager
            + ","
            + RoleList.ShippingManager
    )]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var resultToEdit = _productVendorMappingService.Get(
            x => x.ProductVendorMappingId == id && x.IsActive,
            includes: new Expression<Func<ProductVendorMapping, object>>[]{
                x=>x.Product,
                x=>x.Vendor
            }
        );

        if (resultToEdit == null)
        {
            return NotFound();
        }
        List<string> unitOfMeasureStrings = Enum.GetNames(typeof(UnitofMeasure)).ToList();
        List<SelectListItem> selectListItems = unitOfMeasureStrings
        .Select(s => new SelectListItem { Text = s, Value = s })
        .ToList();
        ViewData["UnitofMeasureList"] = new SelectList(selectListItems, "Value", "Text", resultToEdit.UnitofMeasure);

        List<string> termStrings = Enum.GetNames(typeof(Terms)).ToList();
        List<SelectListItem> selectTermListItems = termStrings
        .Select(t => new SelectListItem { Text = t, Value = t })
        .ToList();
        ViewData["TermsList"] = new SelectList(selectTermListItems, "Value", "Text", resultToEdit.Term);

        return View(resultToEdit);
    }

    //edits the value and updates the view
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.InventoryManager
            + ","
            + RoleList.ShippingManager
    )]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("ProductVendorMappingId,ProductId,VendorId,isPrimaryVendor,Cost,MOQ,OrderMultiples,LeadTime,VendorSku,IsActive,Notes,IsRawMaterial,UnitofMeasure,Term")] ProductVendorMapping productVendorMapping)
    {
        if (id != productVendorMapping.ProductVendorMappingId)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            try
            {
                await _productVendorMappingService.UpdateAsync(productVendorMapping);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductVendorMappingExists(productVendorMapping.ProductVendorMappingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            await _productVendorMappingService.UpdateAsync(productVendorMapping);
            return RedirectToAction(nameof(Index));
        }
        GetProductAndVendorDetails(productVendorMapping.ProductId, productVendorMapping.VendorId,productVendorMapping.UnitofMeasure, productVendorMapping.Term);
        return View(productVendorMapping);
    }


    public IActionResult VendorsByProductId(int id)
    {
        var vMaps = _productVendorMappingService.GetList(x => x.ProductId == id & x.IsActive);
        var vendors = new List<Vendor>();
        if (vMaps == null || vMaps.Count == 0)
        {
            vendors = _vendorService.GetList(x => x.IsActive);
        }
        else
        {
            var vendorIds = vMaps.Select(x => x.VendorId).ToList();

            vendors = _vendorService.GetList(
                x => !vendorIds.Contains(x.VendorId) && x.IsActive
            );
        }

        return Json(vendors);
    }

    public IActionResult VPMList()
    {
        var products = _productService.GetList(
            (P) => P.OrderBy(o => o.Sku)
                .Select(
                    x => new Product
                    {
                      ProductId =  x.ProductId, 
                        Sku = x.Sku + " " + x.Description
                    })
        );
        _productVendorMapperDbFull.Sku = new SelectList(products, "ProductId", "Sku");
        return Json(_productVendorMapperDbFull.Sku.ToList());
    }

    public IActionResult VPMVendorList()
    {
        var vendors = _vendorService.GetList(
            (p)=> p
                .OrderBy(o => o.VendorName)
                .Select(o=>new SelectListItem{ Text = o.VendorName, Value = o.VendorId.ToString()})
        );
        _productVendorMapperDbFull.Vendor = new SelectList(vendors, "Value", "Text");
        return Json(_productVendorMapperDbFull.Vendor.ToList());
    }

    public async Task<IActionResult> UpdateCostAndLeadTime(int? id)
    {
        var getPVM = await _productVendorMappingService.GetAsync(x => x.ProductVendorMappingId == id && x.IsActive);
        if (getPVM == null) return NotFound();
        var getProduct = await _productService.GetAsync(p => p.ProductId == getPVM.ProductId);

        getProduct.Cost = getPVM.Cost;
        getProduct.LeadTime = getPVM.LeadTime;

        await _productService.UpdateAsync(getProduct);

        return Ok();
    }

    [HttpPost("GetProductList")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetProductList()
    {
        int recordsTotal = 0;
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault().ToLower();
        int pageSize = 0;
        var myPermission = "np";
        if (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.InventoryManager)
            || User.IsInRole(RoleList.ShippingManager)
        )
        {
            myPermission = "yes";
        }
        else
        {
            myPermission = "no";
        }

        if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
        {
            if (User.IsInRole(RoleList.ExternalUser))
            {
                recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive && x.Product.IsExternalProduct && x.Vendor.IsExternal);
            }
            else
            {
                recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive && !x.Product.IsExternalProduct && !x.Vendor.IsExternal);
            }
        }
        else
        {
            recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive);
        }

        if (length != null)
        {
            if (length == "-1")
            {
                pageSize = recordsTotal;
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        IQueryable<ProductVendorMapping> vMapProductQuery(IQueryable<ProductVendorMapping> V)
        {
            V = V.Where(x => x.IsActive)
                .Include(x => x.Product)
                .Include(x => x.Product.ProductTags)
                    .ThenInclude(x => x.Tag)
                .Include(x => x.Vendor);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                V = User.IsInRole(RoleList.ExternalUser)
                    ? V.Where(x => x.Product.IsExternalProduct && x.Vendor.IsExternal)
                    : V.Where(x => !x.Product.IsExternalProduct && !x.Vendor.IsExternal);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                V = V.Where(x =>
                    x.VendorSku.ToLower().Contains(searchValue) ||
                    x.Product.Sku.Contains(searchValue) ||
                    x.UnitofMeasure.Contains(searchValue) ||
                    x.Term.Contains(searchValue) ||
                    x.Cost.ToString().Contains(searchValue) ||
                    x.Product.Description.ToLower().Contains(searchValue) ||
                    x.Vendor.VendorName.ToLower().Contains(searchValue)
                );
            }

            return V.Select(x => new ProductVendorMapping
            {
                Product = x.Product,
                ProductId = x.ProductId,
                ProductVendorMappingId = x.ProductVendorMappingId,
                VendorSku = x.VendorSku,
                Vendor = x.Vendor,
                VendorId = x.VendorId,
                Cost = x.Cost,
                LeadTime = x.LeadTime,
                isPrimaryVendor = x.isPrimaryVendor,
                IsRawMaterial = x.IsRawMaterial,
                UnitofMeasure = x.UnitofMeasure,
                Term = x.Term,
                Permission = myPermission == "yes" ? "Yes" : "No",
            }).Skip(skip).Take(pageSize);
        }

        _productVendorMapperDbFull.ProductVendorMappingsProduct = await _productVendorMappingService.GetListAsync(vMapProductQuery);

        var jsonData = new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data = _productVendorMapperDbFull.ProductVendorMappingsProduct
        };


        return Ok(jsonData);
    }

    [HttpPost("GetVendorList")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetVendorList()
    {
        int recordsTotal = 0;
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault().ToLower();
        int pageSize = 0;
        var myPermission = "np";
        if (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.InventoryManager)
            || User.IsInRole(RoleList.ShippingManager)
        )
        {
            myPermission = "yes";
        }
        else
        {
            myPermission = "no";
        }

        if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
        {
            if (User.IsInRole(RoleList.ExternalUser))
            {
                recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive && x.Product.IsExternalProduct && x.Vendor.IsExternal);
            }
            else
            {
                recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive && !x.Product.IsExternalProduct && !x.Vendor.IsExternal);
            }
        }
        else
        {
            recordsTotal = await _productVendorMappingService.GetCountAsync(x => x.IsActive);
        }

        if (length != null)
        {
            if (length == "-1")
            {
                pageSize = recordsTotal;
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        IQueryable<ProductVendorMapping> vMapVendorQuery(IQueryable<ProductVendorMapping> V)
        {
            V = V.Where(x => x.IsActive)
                .Include(x => x.Product)
                .Include(x => x.Vendor);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                V = User.IsInRole(RoleList.ExternalUser)
                    ? V.Where(x => x.Product.IsExternalProduct && x.Vendor.IsExternal)
                    : V.Where(x => !x.Product.IsExternalProduct && !x.Vendor.IsExternal);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                V = V.Where(x =>
                    x.VendorSku.ToLower().Contains(searchValue) ||
                    x.Product.Sku.Contains(searchValue) ||
                    x.UnitofMeasure.Contains(searchValue) ||
                    x.Term.Contains(searchValue) ||
                    x.Cost.ToString().Contains(searchValue) ||
                    x.Product.Description.ToLower().Contains(searchValue) ||
                    x.Vendor.VendorName.ToLower().Contains(searchValue)
                );
            }

            return V.Select(x => new ProductVendorMapping
            {
                Product = x.Product,
                ProductId = x.ProductId,
                VendorId = x.VendorId,
                ProductVendorMappingId = x.ProductVendorMappingId,
                VendorSku = x.VendorSku,
                Vendor = x.Vendor,
                Cost = x.Cost,
                LeadTime = x.LeadTime,
                isPrimaryVendor = x.isPrimaryVendor,
                IsRawMaterial = x.IsRawMaterial,
                UnitofMeasure = x.UnitofMeasure,
                Term = x.Term,
                Permission = myPermission == "yes" ? "Yes" : "No",
            }).Skip(skip).Take(pageSize);
        }

        _productVendorMapperDbFull.ProductVendorMappingsVendor = await _productVendorMappingService.GetListAsync(vMapVendorQuery);

        var jsonData = new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data = _productVendorMapperDbFull.ProductVendorMappingsVendor
        };


        return Ok(jsonData);
    }

    private void GetProductAndVendorDetails(int? productId = null, int? venderId = null, string unitofMeasure = null, string term = null)
    {
        var productQuery = (IQueryable<Product> P) =>
        {
            P = P.Where(x => x.IsActive);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                P = User.IsInRole(RoleList.ExternalUser) ? P.Where(x => x.IsExternalProduct) : P.Where(x => !x.IsExternalProduct);
            }

            return P.OrderBy(x => x.Sku)
                .Select(
                    x => new Product
                    {
                        ProductId = x.ProductId,
                        Sku = x.Sku + " " + x.Description.TrimEnd()
                    }
                );
        };

        var vendorQuery = (IQueryable<Vendor> V) =>
        {
            V = V.Where(x => x.IsActive);

            if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                V = User.IsInRole(RoleList.ExternalUser) ? V.Where(x => x.IsExternal) : V.Where(x => !x.IsExternal);
            }

            return V.OrderBy(x => x.VendorId)
                .Select(
                   x => new Vendor
                   {
                       VendorId = x.VendorId,
                       VendorName = x.VendorName,
                       VendorNumber = x.VendorNumber,
                   }
                );
        };

        var products = _productService.GetList(productQuery);
        var vendors = _vendorService.GetList(vendorQuery);

        ViewData["ProductList"] = productId == null ? new SelectList(products, "ProductId", "Sku") : new SelectList(products, "ProductId", "Sku", productId);
        ViewData["VendorList"] = venderId == null ? new SelectList(vendors, "VendorId", "VendorName") : new SelectList(vendors, "VendorId", "VendorName", venderId);

        List<string> unitOfMeasureStrings = Enum.GetNames(typeof(UnitofMeasure)).ToList();
        List<SelectListItem> selectListItems = unitOfMeasureStrings
        .Select(s => new SelectListItem { Text = s, Value = s })
        .ToList();
        ViewData["UnitofMeasureList"] = unitofMeasure == null ? new SelectList(selectListItems, "Value", "Text") : new SelectList(selectListItems, "Value", "Text", unitofMeasure);

        List<string> termStrings = Enum.GetNames(typeof(Terms)).ToList();
        List<SelectListItem> selectTermListItems = termStrings
        .Select(t => new SelectListItem { Text = t, Value = t })
        .ToList();
        ViewData["TermsList"] = new SelectList(selectTermListItems, "Value", "Text");
    }

    private bool ProductVendorMappingExists(int id)
    {
        var PVM = _productVendorMappingService.Get(
            x => x.ProductVendorMappingId == id,
            includes: new Expression<Func<ProductVendorMapping, object>>[]{
                x=>x.Product,
                x=>x.Vendor
            });
        return PVM != null;
    }
}