using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.InventoryBasic + "," + RoleList.ShippingBasic)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class SubCategoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    private static List<SubCategory> _subcatDbFull = new();

    public SubCategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SubCategories
    public async Task<IActionResult> Index()
    {
        return View(await _context.SubCategory.Where(x => x.IsActive).ToListAsync());
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public List<SubCategory> ToggleActive(bool id)
    {
        if (id)
        {
            _subcatDbFull = _context.SubCategory.Where(x => x.IsActive).OrderBy(x => x.Description).ToList();
            return _subcatDbFull;
        }
        _subcatDbFull = _context.SubCategory.OrderBy(x => x.Description).ToList();
        return _subcatDbFull;

    }

    [HttpGet]
    public IActionResult PartialViewTableShow()
    {
        return PartialView("PartialIndex", _subcatDbFull);
    }


    // GET: SubCategories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subCategory = await _context.SubCategory
            .FirstOrDefaultAsync(m => m.SubCategoryId == id);
        if (subCategory == null)
        {
            return NotFound();
        }

        return View(subCategory);
    }

    // GET: subCategory/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: subCategory/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("Description,IsActive")] SubCategory sub)
    {
        if (ModelState.IsValid)
        {
            _context.Add(sub);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(sub);
    }

    // GET: subCategory/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subCategory = await _context.SubCategory.FindAsync(id);
        if (subCategory == null)
        {
            return NotFound();
        }
        return View(subCategory);
    }

    // POST: subCategory/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("SubCategoryId,Description,IsActive")] SubCategory subCategory)
    {
        if (id != subCategory.SubCategoryId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(subCategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubCategoryExists(subCategory.SubCategoryId))
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
        return View(subCategory);
    }

    // GET: subCategory/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subCategory = await _context.SubCategory
            .FirstOrDefaultAsync(m => m.SubCategoryId == id);
        if (subCategory == null)
        {
            return NotFound();
        }

        return View(subCategory);
    }

    // POST: subCategory/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subCategory = await _context.SubCategory.FindAsync(id);
        _context.SubCategory.Remove(subCategory);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SubCategoryExists(int id)
    {
        return _context.SubCategory.Any(e => e.SubCategoryId == id);
    }
}
