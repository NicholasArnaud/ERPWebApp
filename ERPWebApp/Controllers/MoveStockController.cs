using BarcodeStandard;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Linq.Expressions;
using System.Security.Claims;
using ERPWebApp.Data.Extensions;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Controllers;

[Authorize(
    Roles = RoleList.Administrator
        + ","
        + RoleList.Manager
        + ","
        + RoleList.InventoryBasic
        + ","
        + RoleList.InventoryManager
        + ","
        + RoleList.ShippingBasic
        + ","
        + RoleList.ShippingManager
        + ","
        + RoleList.ExternalUser
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class MoveStockController(
    IMoveStockHistoryService moveStockHistoryService,
    ILocationService locationService,
    IStocksService stocksService,
    IProductService productService,
    ISiteService siteService,
    IUserSiteMappingService userSiteMappingService
) : Controller
{
    public IActionResult Index()
    {
        ResetViewBags();

        return View(new MoveStock());
    }

    [HttpPost("GetTableData")]
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult> GetTableData(string productsku = null)
    {
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"]
            .FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();

        var search = new SearchParameters
        {
            Start = string.IsNullOrEmpty(start) ? 0 : int.Parse(start),
            PageSize = string.IsNullOrEmpty(length) ? -1 : int.Parse(length),
            SearchValue = searchValue,
            SortBy = sortColumn,
            IsDescending = !string.IsNullOrEmpty(sortColumnDirection) && sortColumnDirection.ToLower() is not "asc",
            UserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        };
        
        bool? isExternal = User.IsInRole(RoleList.ExternalUser) ? true : 
            (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)) ? null : false;
        

        var (history, count) = await moveStockHistoryService.GetStockMovementHistoryAsync(search, isExternal, productsku);


        var stockMovementHistories = history.ToList();
        
        var jsonData = new
        {
            draw,
            data = stockMovementHistories,
            recordsTotal = count,
            recordsFiltered = count
        };
        
        return Ok(jsonData);
    }

    public async Task<IActionResult> LocationsBySiteId(int SiteId)
    {

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            List<Location> locationsList = await locationService.GetListAsync(
                x => x.SiteId == SiteId && x.IsActive,
                orderSelectors: new Expression<Func<Location, string>>[]{
                    o => o.LocationName
                }
            );

            return Json(locationsList);
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            var locationsList = await locationService.GetListAsync(
                x => x.SiteId == SiteId && x.IsActive && x.IsExternal,
                orderSelectors: new Expression<Func<Location, string>>[]{
                    o => o.LocationName
                }
            );

            return Json(locationsList);
        }

        // implied else, if we hit this, user is not admin/manager or external roles, thus is internal user
        var resultList = await locationService.GetListAsync(
            x => x.SiteId == SiteId && x.IsActive && !x.IsExternal,
            orderSelectors: new Expression<Func<Location, string>>[]{
                o => o.LocationName
            }
        );

        return Json(resultList);
    }

    public IActionResult SitesByProduct(int ProductId)
    {
        var query = (IQueryable<Stock> stock) =>
        {
            stock = stock.Where(x => x.ProductId == ProductId && x.TotalAvailable > 0)
                .Include(x => x.Location)
                .Include(y => y.Location.Sites);

            if (User.IsInRole(RoleList.ExternalUser))
            {
                stock = stock.Where(x => x.IsExternal);
            }
            else if (!(User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)))
            {
                stock = stock.Where(x => !x.IsExternal);
            }

            return stock.Select(
                z => new
                {
                    SiteId = z.Location.SiteId,
                    LocationId = z.LocationId,
                    MaxQuantity = z.TotalAvailable,
                    SiteLocation = $"{z.Location.Sites.SiteName} : {z.Location.LocationName} : {z.TotalAvailable}"
                }
            );
        };

        var result = stocksService.GetList<object>(query);

        return Json(result);
    }

    [HttpPost]
    public IActionResult GenerateBarcode(int ProductId)
    {
        if (ProductId == 0)
        {
            return Json("please select a product");
        }

        var product = productService.Get(x => x.ProductId == ProductId);
        if (product == null)
        {
            return Json("Product not found");
        }

        string img;
        using (var ms = new MemoryStream())
        {
            var b = new Barcode
            {
                IncludeLabel = true,
                LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
            };
            var image = b.Encode(BarcodeStandard.Type.Code128B, product.Sku, SKColors.Black, SKColors.White, 290, 120);

            // Generate the barcode image
            using var bitmap = SKBitmap.FromImage(image);
            bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
            img = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }

        return Json(img);
    }

    [HttpPost]
    
    public async Task<IActionResult> MoveStockForm([Bind("FromStock,ToStock,quantity")] MoveStock moveStock)
    {
        try
        {

            if (moveStock.FromStock is null)
            {
                ModelState.AddModelError("FromStock.LocationId", "Location is required");
                ModelState.AddModelError("FromStock.ProductId", "Product is required");
            }
            if (moveStock.FromStock?.LocationId == 0) ModelState.AddModelError("FromStock.LocationId", "Location is required");
            if (moveStock.FromStock?.ProductId == 0) ModelState.AddModelError("FromStock.ProductId", "Product is required");

            if (moveStock.ToStock is null)
            {
                ModelState.AddModelError("ToStock.Location.SiteId", "To site is required");
                ModelState.AddModelError("ToStock.LocationId", "To location is required");
            }
            if (moveStock.ToStock?.Location?.SiteId == 0) ModelState.AddModelError("ToStock.Location.SiteId", "Site is required");
            if (moveStock.ToStock?.LocationId == 0) ModelState.AddModelError("ToStock.LocationId", "Location is required");

            if (!ModelState.IsValid)
            {
                ResetViewBags();
                return View("Index", moveStock);
            }

            moveStock.ModifiedBy = User.Identity.Name;

            var result = await moveStockHistoryService.MoveStock(moveStock);

            var sku = result.FromStock.Products.Sku;
            var location_to = result.ToStock.Location.LocationName;
            var location_from = result.FromStock.Location.LocationName;

            ResetViewBags();
            TempData["successMessage"] = "Move Successful.";
            TempData["detailMessage"] = $"SKU: ({sku}) Quantity: ({moveStock.quantity}) From: ({location_from}) To: ({location_to})";
            return RedirectToAction("Index");

        }
        catch (Exception e)
        {
            ResetViewBags();

            Console.WriteLine("An issue has arisen:" + e.Message);
            TempData["errorMessage"] = e.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    
    public async Task<ActionResult> AddStockForm([Bind("FromStock,ToStock,quantity")] MoveStock addStock)
    {
        try
        {
            if (addStock.FromStock is null)
            {
                ModelState.AddModelError("FromStock.ProductId", "Product is required");
            }
            if (addStock.ToStock is null)
            {
                ModelState.AddModelError("ToStock.Location.SiteId", "To site is required");
                ModelState.AddModelError("ToStock.LocationId", "To location is required");
            }
            if (addStock.FromStock?.ProductId == 0) ModelState.AddModelError("FromStock.ProductId", "Product is required");
            if (addStock.ToStock?.Location?.SiteId == 0) ModelState.AddModelError("ToStock.Location.SiteId", "Site is required");
            if (addStock.ToStock?.LocationId == 0) ModelState.AddModelError("ToStock.LocationId", "Location is required");

            if (!ModelState.IsValid)
            {
                ResetViewBags();
                return View("Index", addStock);
            }

            addStock.ModifiedBy = User.Identity.Name;

            var result = await moveStockHistoryService.AddStock(addStock);

            var sku = result.ToStock.Products.Sku;
            var locationName = result.ToStock.Location.LocationName;
            ResetViewBags();
            TempData["successMessage"] = "Add Successful.";
            TempData["detailMessage"] = $"SKU: ({sku}) Quantity: ({addStock.quantity}) Location: ({locationName})";

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ResetViewBags();

            Console.WriteLine("An issue has arisen:" + e.Message);
            TempData["errorMessage"] = e.Message;

            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    
    public async Task<ActionResult> RemoveStockForm([Bind("FromStock,ToStock,quantity")] MoveStock moveStock)
    {
        try
        {
            if (moveStock.FromStock is null)
            {
                ModelState.AddModelError("FromStock.LocationId", "Location is required");
                ModelState.AddModelError("FromStock.ProductId", "Product is required");
            }
            if (moveStock.FromStock?.LocationId == 0) ModelState.AddModelError("FromStock.LocationId", "Location is required");
            if (moveStock.FromStock?.ProductId == 0) ModelState.AddModelError("FromStock.ProductId", "Product is required");

            if (!ModelState.IsValid)
            {
                ResetViewBags();
                return View("Index", moveStock);
            }

            moveStock.ModifiedBy = User.Identity.Name;

            var result = await moveStockHistoryService.RemoveStock(moveStock);

            var sku = result.FromStock.Products.Sku;
            var location = result.FromStock.Location.LocationName;
            ResetViewBags();
            TempData["successMessage"] = "Remove Successful.";
            TempData["detailMessage"] = $"SKU: ({sku}) Quantity: ({moveStock.quantity}) Location: ({location})";

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ResetViewBags();

            Console.WriteLine("An issue has arisen:" + e.Message);
            TempData["errorMessage"] = e.Message;
            return RedirectToAction("Index");
        }
    }

    public async Task<JsonResult> SearchProducts(string searchTerm)
    {
        bool? isExternal = User.IsInRole(RoleList.ExternalUser) ? true : 
            (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)) ? null : false;
        
        var products = await productService.GetListAsync(
            (IQueryable<Product> q) => q
                .Where(x=>x.IsActive && (
                      x.Sku.Contains(searchTerm)
                      || x.Description.Contains(searchTerm)
                ))
                .WhereIf(isExternal != null, x => x.IsExternalProduct == isExternal && x.IsActive)
                .OrderBy(x=>x.Sku)
                .Take(10)
                .Select(x=> new
                {
                    text = $"{x.Sku} : {x.Description}",
                    id = x.ProductId
                })
        );
        
        return new JsonResult(products);
    }

    private void ResetViewBags()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        var userSites = userSiteMappingService.GetList(
            (IQueryable<UserSiteMapping> m) => m.Where(x=>x.UserId == userId)
                .Select(x=> x.SiteId)
        );
        
        bool? isExternal = User.IsInRole(RoleList.ExternalUser) ? true : 
            (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)) ? null : false;
        
        var sites = siteService.GetList(
            (IQueryable<Site> s) => s
                .Where(x => userSites.Contains(x.SiteId))
                .WhereIf(isExternal != null, x => x.IsExternal == isExternal && x.IsActive)
                .Select(x => new SelectListItem
                    {
                        Text = x.SiteName,
                        Value = x.SiteId.ToString()
                    }
                ).OrderBy(x => x.Text)
        );
        var locations = locationService.GetList(
            (IQueryable<Location> q) => q
                .Where(x => x.IsActive)
                .WhereIf(isExternal != null, x => x.IsExternal == isExternal && x.IsActive)
                .Select(x => new SelectListItem
                {
                    Value =x.LocationId.ToString(),
                    Text = x.LocationName 
                        
                })
        );
        
        ViewData["SiteName"] = new SelectList(sites, "Value", "Text");
        ViewData["LocationName"] = new SelectList(locations, "Value", "Text");
    }
}
