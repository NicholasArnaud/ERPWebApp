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
using ERPWebApp.Data.Extensions;

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
public class LocationsController(
    ILocationService locationService,
    ISiteService siteService,
    IStocksService stocksService
) : Controller
{
    // GET: Locations
    public ActionResult Index()
    {
        bool? isExternal = User.IsInRole(RoleList.ExternalUser) ? true
            : (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)) 
                ? null : false;
        GetSiteList(null, isExternal);
        return View();
    }

    private void GetSiteList(int? selectedId, bool? isExternal, int[] ids = null)
    {
        var sites = siteService.GetList(
            (IQueryable<Site> p) => p
                .Where(x => x.IsActive == true)
                .WhereIf(isExternal.HasValue, x => x.IsExternal == isExternal)
                .WhereIf(ids is not null && ids.Any(), x => ids.Contains(x.SiteId))
                .Select(x => new SelectListItem { Value = x.SiteId.ToString(), Text = x.SiteName })
        );
        ViewData["Site"] = new SelectList(sites, "Value", "Text", selectedId);
    }

    private async Task GetSiteListAsync(int? selectedId, bool? isExternal, int[] ids = null)
    {
        var sites = await siteService.GetListAsync(
            (IQueryable<Site> p) => p
                .Where(x => x.IsActive == true)
                .WhereIf(isExternal.HasValue, x => x.IsExternal == isExternal)
                .WhereIf(ids is not null && ids.Any(), x => ids.Contains(x.SiteId))
                .Select(x => new SelectListItem { Value = x.SiteId.ToString(), Text = x.SiteName })
        );
        ViewData["Site"] = new SelectList(sites, "Value", "Text", selectedId);
    }

    [HttpPost("GetLocations")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetLocations(bool showInactive, int siteFilter)
    {
        // gets form data for serverside processing
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault().ToLower();
        
        var myPermission = (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.InventoryManager)
            || User.IsInRole(RoleList.ShippingManager)
        ) ? "yes" : "no";

        
        bool? isExternal = User.IsInRole(RoleList.ExternalUser) ? true
            : (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer)) 
                ? null : false;

        var parameters = new SearchParameters
        {
            SortBy = sortColumn,
            SearchValue = searchValue,
            IsDescending = !string.IsNullOrEmpty(sortColumnDirection) && sortColumnDirection == "desc",
            Start = start != null ? Convert.ToInt32(start) : 0,
            PageSize = length != null ? Convert.ToInt32(length) : -1
        };

        var (locations, count) = await locationService.GetLocationsAsync(
            showInactive,
            isExternal,
            siteFilter,
            myPermission,
            parameters
        );

       await GetSiteListAsync(null, isExternal);

        var jsonData = new
        {
            draw,
            recordsFiltered = count,
            recordsTotal = count,
            data = locations
        };

        return Ok(jsonData);
    }

    // GET: Locations/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await locationService.GetAsync(
            m => m.LocationId == id,
            new Expression<Func<Location, object>>[] { l => l.Sites }
        );

        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    // GET: Locations/Create
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]
    public IActionResult Create()
    {
        if (User.IsInRole(RoleList.Administrator))
        {
            GetSiteList(null, null);
            return View();
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            GetSiteList(null, true);
            return View();
        }

        // admin and external user will not have ExternalViewer, if those terminal cases aren't true then we check
        // if ExternalViewer is true and append the external sites to the existing return
        var isExternal = User.IsInRole(RoleList.ExternalViewer);
        int[] siteIds = [];

        if (User.IsInRole(RoleList.InventoryManager) && User.IsInRole(RoleList.ShippingManager))
        {
            siteIds = [2, 49];
        }
        else if (User.IsInRole(RoleList.InventoryManager))
        {
            siteIds = [2];
        }
        else if (User.IsInRole(RoleList.ShippingManager))
        {
            siteIds = [49];
        }
            
        GetSiteList(null, isExternal, siteIds);

        return View();
    }

    // POST: Locations/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create(
        [Bind("LocationId,SiteId,LocationName,LocationDescription,Type,IsActive,IsExternal")] Location location)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate location names 
                var isExists = locationService.IsExists(l => l.LocationName == location.LocationName);
                if (isExists) ModelState.AddModelError("LocationName", "A location with this name already exists.");

                var isSiteValid = siteService.IsExists(x => x.SiteId == location.SiteId && x.IsActive);
                if (!isSiteValid) ModelState.AddModelError("Site", "Invalid or InActive site");

                if (isSiteValid && !isExists)
                {
                    location.LocationName = location.LocationName.ToUpperInvariant();
                    location.LocationDescription ??= "";
                    await locationService.AddAsync(location);
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

        if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
        {
            await GetSiteListAsync(location.SiteId, null, null);
        }
        else if (
            User.IsInRole(RoleList.InventoryManager) && User.IsInRole(RoleList.ShippingManager)
        )
        {
            await GetSiteListAsync(location.SiteId, null, [2, 49]);
        }
        else if (User.IsInRole(RoleList.InventoryManager))
        {
            await GetSiteListAsync(location.SiteId, null, [2]);
        }
        else if (User.IsInRole(RoleList.ShippingManager))
        {
            await GetSiteListAsync(location.SiteId, null, [49]);
        }
        else if (User.IsInRole(RoleList.ExternalUser))
        {
            await GetSiteListAsync(location.SiteId, true, null);
        }

        return View(location);
    }

    // GET: Locations/Edit/5
    [Authorize(
        Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
    )]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await locationService.GetAsync(x => x.LocationId == id);

        if (location == null)
        {
            return NotFound();
        }

        if (User.IsInRole(RoleList.Administrator))
        {
            await GetSiteListAsync(location.SiteId, null, null);
            return View(location);
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            await GetSiteListAsync(location.SiteId, true, null);
            return View(location);
        }

        // admin and external user will not have ExternalViewer, if those terminal cases aren't true then we check
        // if ExternalViewer is true and append the external sites to the existing return
        var isExternal = User.IsInRole(RoleList.ExternalViewer);

        int[] siteIds = [];

        if (User.IsInRole(RoleList.InventoryManager) && User.IsInRole(RoleList.ShippingManager))
        {
            siteIds = [2, 49];
        }
        else if (User.IsInRole(RoleList.InventoryManager))
        {
            siteIds = [2];
        }
        else if (User.IsInRole(RoleList.ShippingManager))
        {
            siteIds = [2, 49];
        }

        await GetSiteListAsync(location.SiteId, isExternal, siteIds);

        return View(location);
    }

    // POST: Locations/Edit/5
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
    
    public async Task<IActionResult> Edit(
        int id,
        [Bind("LocationId,SiteId,LocationName,LocationDescription,Type,IsActive,IsExternal")]
        Location location
    )
    {
        if (id != location.LocationId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                location.LocationName = location.LocationName.ToUpperInvariant();
                location.LocationDescription ??= "";
                await locationService.UpdateAsync(location);
            }
            catch (DbUpdateConcurrencyException)
            {
                var isExists = locationService.IsExists(x => x.LocationId == location.LocationId);
                if (!isExists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        if (User.IsInRole(RoleList.Administrator))
        {
            var sites = await siteService.GetAllAsync();
            ViewData["SiteName"] = new SelectList(
                sites,
                "SiteId",
                "SiteName",
                location.SiteId
            );
        }

        if (User.IsInRole(RoleList.ExternalUser))
        {
            var sites = await siteService.GetListAsync(s => s.IsExternal && s.IsActive);
            ViewData["SiteName"] = new SelectList(sites, "SiteId", "SiteName", location.SiteId);
            return View(location);
        }

        // admin and external user will not have ExternalViewer, if those terminal cases aren't true then we check
        // if ExternalViewer is true and append the external sites to the existing return
        IEnumerable<Site> externalSites = null;
        if (User.IsInRole(RoleList.ExternalViewer))
        {
            externalSites = siteService.GetList(s => s.IsExternal && s.IsActive).AsEnumerable();
        }

        if (
            User.IsInRole(RoleList.InventoryManager) && User.IsInRole(RoleList.ShippingManager)
        )
        {
            IEnumerable<Site> sites =
                await siteService.GetListAsync(x => (x.SiteId == 2 || x.SiteId == 49) && x.IsActive);
            if (externalSites != null) sites = sites.Union(externalSites);

            ViewData["SiteName"] = new SelectList(
                sites,
                "SiteId",
                "SiteName",
                location.SiteId
            );
        }
        else if (User.IsInRole(RoleList.InventoryManager))
        {
            IEnumerable<Site> sites = await siteService.GetListAsync(x => x.SiteId == 2 && x.IsActive);
            if (externalSites != null) sites = sites.Union(externalSites);

            ViewData["SiteName"] = new SelectList(
                sites,
                "SiteId",
                "SiteName",
                location.SiteId
            );
        }
        else if (User.IsInRole(RoleList.ShippingManager))
        {
            IEnumerable<Site> sites = await siteService.GetListAsync(x => x.SiteId == 49 && x.IsActive);
            if (externalSites != null) sites = sites.Union(externalSites);

            ViewData["SiteName"] = new SelectList(
                sites,
                "SiteId",
                "SiteName",
                location.SiteId
            );
        }

        return View(location);
    }

    // GET: Locations/Delete/5
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

        var location = await locationService.GetAsync(x => x.LocationId == id,
            new Expression<Func<Location, object>>[] { l => l.Sites });

        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    // POST: Locations/Delete/5
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
            var isActive = locationService.IsExists(x => x.LocationId == id && x.IsActive);

            if (!isActive)
            {
                await locationService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Location is still ACTIVE. Please mark as INACTIVE and try the operation again.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Location is still being used elsewhere.";
            return RedirectToAction(nameof(Index));
        }
    }


    public IActionResult DownloadBarcode(int id)
    {
        using MemoryStream ms = new();
        var b = new Barcode
        {
            IncludeLabel = true,
            LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
        };
        using var bitmap = SKBitmap.FromImage(b.Encode(BarcodeStandard.Type.Code128B, id.ToString()));
        bitmap.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(ms);
        string fileName = "barcode.jpg";
        return File(ms.ToArray(), "image/jpeg", fileName);
    }

    public async Task<IActionResult> GetStockDetails(int id)
    {
        var result = await stocksService.GetListAsync(x => x.LocationId == id,
            includes: new Expression<Func<Stock, object>>[]
            {
                x => x.Products
            });
        return Ok(result);
    }
}