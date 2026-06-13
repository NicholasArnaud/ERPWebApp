using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory.SkuProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.SkuProperties;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.InventoryBasic + "," + RoleList.ShippingBasic)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class SkuCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    public SkuCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SkuCategories
    public async Task<IActionResult> Index()
    {
        return View(await _context.SkuCategory.ToListAsync());
    }

    // GET: SkuCategories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuCategory = await _context.SkuCategory
            .FirstOrDefaultAsync(m => m.SkuCategoryId == id);
        if (skuCategory == null)
        {
            return NotFound();
        }

        return View(skuCategory);
    }

    // GET: SkuCategories/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: SkuCategories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("SkuCategoryId,Category,Attribute,IsActive")] SkuCategory skuCategory)
    {
        if (ModelState.IsValid)
        {
            skuCategory.LastModified = now;
            _context.Add(skuCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(skuCategory);
    }

    // GET: SkuCategories/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuCategory = await _context.SkuCategory.FindAsync(id);
        if (skuCategory == null)
        {
            return NotFound();
        }
        return View(skuCategory);
    }

    // POST: SkuCategories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("SkuCategoryId,Category,Attribute,IsActive")] SkuCategory skuCategory)
    {
        if (id != skuCategory.SkuCategoryId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                skuCategory.LastModified = now;
                _context.Update(skuCategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkuCategoryExists(skuCategory.SkuCategoryId))
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
        return View(skuCategory);
    }

    // GET: SkuCategories/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuCategory = await _context.SkuCategory
            .FirstOrDefaultAsync(m => m.SkuCategoryId == id);
        if (skuCategory == null)
        {
            return NotFound();
        }

        return View(skuCategory);
    }

    // POST: SkuCategories/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var skuCategory = await _context.SkuCategory.FindAsync(id);
        _context.SkuCategory.Remove(skuCategory);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SkuCategoryExists(int id)
    {
        return _context.SkuCategory.Any(e => e.SkuCategoryId == id);
    }
}
