using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
public class BundleItemsController : Controller
{
    private readonly IBundleItemService _bundleItemService;
    private readonly IBundleService _bundleService;
    private readonly IProductService _productService;

    public BundleItemsController(IBundleItemService bundleItemService, IBundleService bundleService, IProductService productService)
    {
        _bundleItemService = bundleItemService;
        _bundleService = bundleService;
        _productService = productService;
    }

    // GET: BundleItems
    public async Task<IActionResult> Index()
    {
        var bundleItems = await _bundleItemService.GetAllAsync(null, [x => x.Bundle, x => x.Product]);
        return View(bundleItems);
    }

    // GET: BundleItems/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var bundleItem = await _bundleItemService.GetAsync(x => x.BundleItemId == id, includes: [x => x.Bundle, x => x.Product]);
        if (bundleItem == null)
        {
            return NotFound();
        }

        return View(bundleItem);
    }

    // GET: BundleItems/Create
    public IActionResult Create()
    {
        BundleSelectList(null);
        ProductSelectList(null);
        return View();
    }

    private void BundleSelectList(int? selectedId)
    {
        var items = _bundleService.GetList(
            (IQueryable<Bundle> b) => b.Select(x=> new SelectListItem
            {
                Value = x.BundleId.ToString(),
                Text = x.BundleName
            })
        );
        ViewData["BundleId"] = new SelectList(items, "Value", "Text", selectedId);
    }

    private void ProductSelectList(int? selectedId)
    {
        var items = _productService.GetList(
            (IQueryable<Product> p) => p.Select(x=> new SelectListItem
            {
                Value = x.ProductId.ToString(),
                Text = x.Sku
            })
        );
        ViewData["ProductId"] = new SelectList(items, "Value", "Text", selectedId);
    }

    // POST: BundleItems/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("BundleItemId,BundleId,ProductId,Quantity")] BundleItem bundleItem)
    {
        if (bundleItem.BundleId == 0)
        {
            ModelState.AddModelError(nameof(BundleItem.BundleId), "The Bundle field is required");
        }
        if (bundleItem.ProductId == 0)
        {
            ModelState.AddModelError(nameof(BundleItem.ProductId), "The Product field is required");
        }

        try
        {
            if (ModelState.IsValid)
            {
                await _bundleItemService.AddAsync(bundleItem);

                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(nameof(BundleItem.ProductId), ex.Message);
        }
        
        BundleSelectList(bundleItem.BundleId);
        ProductSelectList(bundleItem.ProductId);

        return View(bundleItem);
    }

    // GET: BundleItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bundleItem = await _bundleItemService.GetAsync(x => x.BundleItemId == id, includes: [x => x.Bundle, x => x.Product]);
        if (bundleItem == null)
        {
            return NotFound();
        }
        BundleSelectList(bundleItem.BundleId);
        ProductSelectList(bundleItem.ProductId);
        return View(bundleItem);
    }

    // POST: BundleItems/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("BundleItemId,BundleId,ProductId,Quantity")] BundleItem bundleItem)
    {
        if (id != bundleItem.BundleItemId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _bundleItemService.UpdateAsync(bundleItem);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BundleItemExists(bundleItem.BundleItemId))
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
        BundleSelectList(bundleItem.BundleId);
        ProductSelectList(bundleItem.ProductId);
        return View(bundleItem);
    }

    // GET: BundleItems/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bundleItem = await _bundleItemService.GetAsync(x => x.BundleItemId == id, includes: [x => x.Bundle, x => x.Product]);
        if (bundleItem == null)
        {
            return NotFound();
        }

        return View(bundleItem);
    }

    // POST: BundleItems/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bundleItem = await _bundleItemService.GetAsync(x => x.BundleItemId == id);
        if (bundleItem != null)
        {
            _bundleItemService.Remove(id);
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BundleItemExists(int id)
    {
        return _bundleItemService.IsExists(x => x.BundleItemId == id);
    }
}
