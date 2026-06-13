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
public class SkuColorsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    public SkuColorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SkuColors
    public async Task<IActionResult> Index()
    {
        return View(await _context.SkuColor.ToListAsync());
    }

    // GET: SkuColors/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuColor = await _context.SkuColor
            .FirstOrDefaultAsync(m => m.SkuColorId == id);
        if (skuColor == null)
        {
            return NotFound();
        }

        return View(skuColor);
    }

    // GET: SkuColors/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: SkuColors/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("SkuColorId,Color,Attribute,IsActive")] SkuColor skuColor)
    {
        if (ModelState.IsValid)
        {
            skuColor.LastModified = now;
            _context.Add(skuColor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(skuColor);
    }

    // GET: SkuColors/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuColor = await _context.SkuColor.FindAsync(id);
        if (skuColor == null)
        {
            return NotFound();
        }
        return View(skuColor);
    }

    // POST: SkuColors/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("SkuColorId,Color,Attribute,IsActive")] SkuColor skuColor)
    {
        if (id != skuColor.SkuColorId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                skuColor.LastModified = now;
                _context.Update(skuColor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkuColorExists(skuColor.SkuColorId))
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
        return View(skuColor);
    }

    // GET: SkuColors/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var skuColor = await _context.SkuColor
            .FirstOrDefaultAsync(m => m.SkuColorId == id);
        if (skuColor == null)
        {
            return NotFound();
        }

        return View(skuColor);
    }

    // POST: SkuColors/Delete/5
    [HttpPost, ActionName("Delete")]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var skuColor = await _context.SkuColor.FindAsync(id);
        _context.SkuColor.Remove(skuColor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SkuColorExists(int id)
    {
        return _context.SkuColor.Any(e => e.SkuColorId == id);
    }
}
