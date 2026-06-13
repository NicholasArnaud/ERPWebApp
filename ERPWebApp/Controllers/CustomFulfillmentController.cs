using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.FinancialBasic)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class CustomFulfillmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomFulfillmentController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View(_context.ProductCustomFulFillment.Include(x => x.Product).Include(x => x.ShipStationStore).ToList());
    }

    public IActionResult Create()
    {
        ViewData["BundleId"] = new SelectList(_context.Bundle.Select(z => new { z.BundleId, z.BundleName }), "BundleId", "BundleName");
        ViewData["ProductId"] = new SelectList(_context.Product.Where(x => x.IsActive).Select(z => new { z.ProductId, Sku = z.Sku + " : " + z.Description }), "ProductId", "Sku");
        ViewData["StoreId"] = new SelectList(_context.ShipStationStore.Where(x => x.IsActive).Select(z => new { z.ShipStationStoreId, StoreName = z.StoreName + " : " + z.Email }), "ShipStationStoreId", "StoreName");
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("ShipStationStoreId,ProductId,BundleId,CustomFulfillmentCost,EffectiveDate,IsActive")] ProductCustomFulfillment customFulfillment)
    {
        ViewData["BundleId"] = new SelectList(_context.Bundle.Select(z => new { z.BundleId, z.BundleName }), "BundleId", "BundleName");
        ViewData["ProductId"] = new SelectList(_context.Product.Where(x => x.IsActive).Select(z => new { z.ProductId, Sku = z.Sku + " : " + z.Description }), "ProductId", "Sku");
        ViewData["StoreId"] = new SelectList(_context.ShipStationStore.Where(x => x.IsActive).Select(z => new { z.ShipStationStoreId, StoreName = z.StoreName + " : " + z.Email }), "ShipStationStoreId", "StoreName");

        if (customFulfillment.ProductId == null && customFulfillment.BundleId == null)
        {
            ModelState.AddModelError(nameof(ProductCustomFulfillment.ProductId), "A Product or Bundle must be selected.");
            ModelState.AddModelError(nameof(ProductCustomFulfillment.BundleId), "A Product or Bundle must be selected.");
        }
        else if (customFulfillment.ProductId != null && customFulfillment.BundleId != null)
        {
            ModelState.AddModelError(nameof(ProductCustomFulfillment.ProductId), "Cannot select both a Product and Bundle.");
            ModelState.AddModelError(nameof(ProductCustomFulfillment.BundleId), "Cannot select both a Product and Bundle.");
        }

        else if (ModelState.IsValid)
        {
            _context.Add(customFulfillment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(customFulfillment);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customFulfillment = await _context.ProductCustomFulFillment.FindAsync(id);
        if (customFulfillment == null)
        {
            return NotFound();
        }
        ViewData["BundleId"] = new SelectList(_context.Bundle.Select(z => new { z.BundleId, z.BundleName }), "BundleId", "BundleName", customFulfillment.BundleId);
        ViewData["ProductId"] = new SelectList(_context.Product.Where(x => x.IsActive).Select(z => new { z.ProductId, Sku = z.Sku + " : " + z.Description }), "ProductId", "Sku", customFulfillment.ProductId);
        ViewData["StoreId"] = new SelectList(_context.ShipStationStore.Where(x => x.IsActive).Select(z => new { z.ShipStationStoreId, StoreName = z.StoreName + " : " + z.Email }), "ShipStationStoreId", "StoreName", customFulfillment.ShipStationStoreId);

        return View(customFulfillment);
    }

    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("ProductCustomFulfillmentId, ProductId, BundleId, ShipStationStoreId, EffectiveDate, CustomFulfillmentCost,IsActive")] ProductCustomFulfillment customFulfillment)
    {
        if (id != customFulfillment.ProductCustomFulfillmentId)
        {
            return NotFound();
        }
        if (customFulfillment.ProductId == null && customFulfillment.BundleId == null)
        {
            ModelState.AddModelError(nameof(ProductCustomFulfillment.ProductId), "A Product or Bundle must be selected.");
            ModelState.AddModelError(nameof(ProductCustomFulfillment.BundleId), "A Product or Bundle must be selected.");
        }
        else if (customFulfillment.ProductId != null && customFulfillment.BundleId != null)
        {
            ModelState.AddModelError(nameof(ProductCustomFulfillment.ProductId), "Cannot select both a Product and Bundle.");
            ModelState.AddModelError(nameof(ProductCustomFulfillment.BundleId), "Cannot select both a Product and Bundle.");
        }
        else if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customFulfillment);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customFulfillment);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customFulfillment = await _context.ProductCustomFulFillment.Include(x => x.Product).Include(x => x.ShipStationStore)
            .FirstOrDefaultAsync(m => m.ProductCustomFulfillmentId == id);
        if (customFulfillment == null)
        {
            return NotFound();
        }

        return View(customFulfillment);
    }

    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customFulfillment = await _context.ProductCustomFulFillment.FindAsync(id);
        _context.ProductCustomFulFillment.Remove(customFulfillment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
