using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using ERPWebApp.Services.IServices;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Controllers.Inventory;

[Authorize(
    Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.InventoryBasic
            + ","
            + RoleList.ShippingBasic
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class BundlesController : Controller
{
    private readonly IBundleService _bundleService;

    public BundlesController(IBundleService bundleService)
    {
        _bundleService = bundleService;
    }

    // GET: Bundles
    public async Task<IActionResult> Index()
    {
        var bundles = await _bundleService.GetAllAsync();
        return View(bundles);
    }

    // GET: Bundles/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var bundle = await _bundleService.GetAsync(x => x.BundleId == id);

        if (bundle == null)
        {
            return NotFound();
        }

        return View(bundle);
    }

    // GET: Bundles/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Bundles/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("BundleId,BundleName,FulfillmentCost")] Bundle bundle)
    {
        if (ModelState.IsValid)
        {
            bundle.BundleName = bundle.BundleName.ToUpperInvariant().Trim();
            await _bundleService.AddAsync(bundle);

            return RedirectToAction(nameof(Index));
        }
        return View(bundle);
    }

    // GET: Bundles/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var bundle = await _bundleService.GetAsync(x=>x.BundleId == id);
        if (bundle == null)
        {
            return NotFound();
        }
        return View(bundle);
    }

    // POST: Bundles/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("BundleId,BundleName,FulfillmentCost")] Bundle bundle)
    {
        if (id != bundle.BundleId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _bundleService.UpdateAsync(bundle);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BundleExists(bundle.BundleId))
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
        return View(bundle);
    }

    // GET: Bundles/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var bundle = await _bundleService.GetAsync(x => x.BundleId == id);

        if (bundle == null)
        {
            return NotFound();
        }

        return View(bundle);
    }

    // POST: Bundles/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bundle = await _bundleService.GetAsync(x => x.BundleId == id);
        if (bundle != null)
        {
            _bundleService.Remove(id);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duplicate(int id)
    {
        var bundle = await _bundleService.GetBundleWithItemsAsync(id);
        if (bundle == null)
        {
            return NotFound();
        }

        var newBundle = new Bundle
        {
            BundleName = bundle.BundleName + " - Copy",
            FulfillmentCost = bundle.FulfillmentCost,
            BundleItems = new List<BundleItem>()
        };

        foreach (var item in bundle.BundleItems)
        {
            var newItem = new BundleItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            };
            newBundle.BundleItems.Add(newItem);
        }

        await _bundleService.AddAsync(newBundle);
        return RedirectToAction(nameof(Index));
    }

    private bool BundleExists(int id)
    {
        return _bundleService.IsExists(x => x.BundleId == id);
    }
}
