using BarcodeStandard;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Diagnostics;

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
public class SitesController : Controller
{
    private readonly ISiteService _siteService;

    public SitesController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    // GET: Sites
    public async Task<IActionResult> Index()
    {
        var query = (IQueryable<Site> site) =>
        {
            if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
                site = site.Where(s => !s.IsExternal);
            else if (User.IsInRole(RoleList.ExternalUser))
                site = site.Where(s => s.IsExternal);

            return site.Where(x => x.IsActive).OrderBy(x => x.SiteName);
        };

        List<Site> listSite = await _siteService.GetListAsync(query);
        return View(listSite);

    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<List<Site>> ToggleActive(bool isActive)
    {
        return await _siteService.GetListAsync(x => x.IsActive == isActive);

    }

    [HttpGet]
    public async Task<IActionResult> PartialViewTableShow()
    {
        var query = (IQueryable<Site> site) => site.Where(s => s.IsExternal && s.IsActive).OrderBy(x => x.SiteName);
        var result = await _siteService.GetListAsync(query);
        return PartialView("PartialIndex", result);
    }

    // GET: Sites/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var site = await _siteService.GetAsync(x => x.SiteId == id.Value);
        return site == null ? NotFound() : View(site);
    }

    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.InventoryManager
            + ","
            + RoleList.ShippingManager
    )]
    // GET: Sites/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Sites/Create
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
        [Bind("SiteId,SiteName,SiteDescription,SiteVolume,IsActive,IsRestricted,IsExternal")]
            Site site
    )
    {
        if (ModelState.IsValid)
        {
            await _siteService.AddAsync(site);
            return RedirectToAction(nameof(Index));
        }
        return View(site);
    }

    // GET: Sites/Edit/5
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

        var site = await _siteService.GetAsync(x => x.SiteId == id.Value);
        return site == null ? NotFound() : View(site);
    }

    // POST: Sites/Edit/5
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
        [Bind("SiteId,SiteName,SiteDescription,SiteVolume,IsActive,IsRestricted,IsExternal")]
            Site site
    )
    {
        if (id != site.SiteId)
        {
            return NotFound();
        }

        // guard clause exits if not true, to reduce if-nesting
        if (!ModelState.IsValid)
        {
            Debug.WriteLine("SitesController - Edit(): Model was invalid.");
            return View(site);
        }

        try
        {
            await _siteService.UpdateAsync(site);
        }
        catch (DbUpdateConcurrencyException)
        {
            var IsExist = await _siteService.IsExistsAsync(x => x.SiteId == site.SiteId);
            if (!IsExist)
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Sites/Delete/5
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

        var site = await _siteService.GetAsync(x => x.SiteId == id.Value);
        return site == null ? NotFound() : View(site);
    }

    // POST: Sites/Delete/5
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
            var site = await _siteService.GetAsync(x => x.SiteId == id);
            if (!site.IsActive)
            {
                await _siteService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Site is still active! Mark the Site as INACTIVE and try the operation again.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Site is still being used elsewhere.";
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
}
