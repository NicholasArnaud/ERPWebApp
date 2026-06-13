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
public class SkuUnitOfMeasuresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    public SkuUnitOfMeasuresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SkuUnitOfMeasures
    public async Task<IActionResult> Index()
    {
        return View(await _context.SkuUnitOfMeasure.ToListAsync());
    }

    // GET: SkuUnitOfMeasures/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuUnitOfMeasure = await _context.SkuUnitOfMeasure
            .FirstOrDefaultAsync(m => m.SkuUnitOfMeasureId == id);
        if (skuUnitOfMeasure == null)
        {
            return NotFound();
        }

        return View(skuUnitOfMeasure);
    }

    // GET: SkuUnitOfMeasures/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: SkuUnitOfMeasures/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Create([Bind("SkuUnitOfMeasureId,UnitOfMeasure,Attribute,IsActive")] SkuUnitOfMeasure skuUnitOfMeasure)
    {
        if (ModelState.IsValid)
        {
            skuUnitOfMeasure.LastModified = now;
            _context.Add(skuUnitOfMeasure);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(skuUnitOfMeasure);
    }

    // GET: SkuUnitOfMeasures/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuUnitOfMeasure = await _context.SkuUnitOfMeasure.FindAsync(id);
        if (skuUnitOfMeasure == null)
        {
            return NotFound();
        }
        return View(skuUnitOfMeasure);
    }

    // POST: SkuUnitOfMeasures/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("SkuUnitOfMeasureId,UnitOfMeasure,Attribute,IsActive")] SkuUnitOfMeasure skuUnitOfMeasure)
    {
        if (id != skuUnitOfMeasure.SkuUnitOfMeasureId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                skuUnitOfMeasure.LastModified = now;
                _context.Update(skuUnitOfMeasure);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkuUnitOfMeasureExists(skuUnitOfMeasure.SkuUnitOfMeasureId))
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
        return View(skuUnitOfMeasure);
    }

    // GET: SkuUnitOfMeasures/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuUnitOfMeasure = await _context.SkuUnitOfMeasure
            .FirstOrDefaultAsync(m => m.SkuUnitOfMeasureId == id);
        if (skuUnitOfMeasure == null)
        {
            return NotFound();
        }

        return View(skuUnitOfMeasure);
    }

    // POST: SkuUnitOfMeasures/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var skuUnitOfMeasure = await _context.SkuUnitOfMeasure.FindAsync(id);
        _context.SkuUnitOfMeasure.Remove(skuUnitOfMeasure);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SkuUnitOfMeasureExists(int id)
    {
        return _context.SkuUnitOfMeasure.Any(e => e.SkuUnitOfMeasureId == id);
    }
}
