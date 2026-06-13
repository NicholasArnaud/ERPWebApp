using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Shipping;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialBasic + "," + RoleList.ShippingBasic + "," + RoleList.Manager)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ShippingMethodsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShippingMethodsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ShippingMethods
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.ShippingMethod.Include(s => s.ShippingProvider);
        var count = applicationDbContext.Count();
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: ShippingMethods/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.ShippingMethod == null)
        {
            return NotFound();
        }

        var shippingMethod = await _context.ShippingMethod
            .Include(s => s.ShippingProvider)
            .FirstOrDefaultAsync(m => m.ShippingMethodId == id);
        if (shippingMethod == null)
        {
            return NotFound();
        }

        return View(shippingMethod);
    }
    // GET: ShippingMethods/Create
    public IActionResult Create()
    {
        ViewData["ShippingProviderId"] = new SelectList(_context.ShippingProvider.Where(x => x.IsActive), "ShippingProviderId", "ShippingProviderName");
        return View();
    }
    // POST: ShippingMethods/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ShippingMethodId,ShippingProviderId,ShippingMethodName,IsActive,ModifyDate,ModifyByUser")] ShippingMethod shippingMethod)
    {
        if (ModelState.IsValid)
        {
            shippingMethod.ModifyDate = Now();
            shippingMethod.ModifyByUser = User.Identity.Name;
            shippingMethod.IsActive = true;
            _context.Add(shippingMethod);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(shippingMethod);
    }
    // GET: ShippingMethods/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.ShippingMethod == null)
        {
            return NotFound();
        }

        var shippingMethod = await _context.ShippingMethod.FindAsync(id);
        if (shippingMethod == null)
        {
            return NotFound();
        }
        ViewData["ShippingProviderId"] = new SelectList(_context.ShippingProvider.Where(x => x.IsActive), "ShippingProviderId", "ShippingProviderName", shippingMethod.ShippingProviderId);
        return View(shippingMethod);
    }
    // POST: ShippingMethods/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ShippingMethodId,ShippingProviderId,ShippingMethodName,IsActive,ModifyDate,ModifyByUser")] ShippingMethod shippingMethod)
    {
        if (id != shippingMethod.ShippingMethodId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(shippingMethod);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShippingMethodExists(shippingMethod.ShippingMethodId))
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
        ViewData["ShippingProviderId"] = new SelectList(_context.ShippingProvider, "ShippingProviderId", "ShippingProviderName", shippingMethod.ShippingProviderId);
        return View(shippingMethod);
    }
    // GET: ShippingMethods/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.ShippingMethod == null)
        {
            return NotFound();
        }

        var shippingMethod = await _context.ShippingMethod
            .Include(s => s.ShippingProvider)
            .FirstOrDefaultAsync(m => m.ShippingMethodId == id);
        if (shippingMethod == null)
        {
            return NotFound();
        }

        return View(shippingMethod);
    }
    // POST: ShippingMethods/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.ShippingMethod == null)
        {
            return Problem("Entity set 'ApplicationDbContext.ShippingMethod'  is null.");
        }
        var shippingMethod = await _context.ShippingMethod.FindAsync(id);
        if (shippingMethod != null)
        {
            _context.ShippingMethod.Remove(shippingMethod);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ShippingMethodExists(int id)
    {
        return _context.ShippingMethod.Any(e => e.ShippingMethodId == id);
    }
    private DateTime Now() => TimeZoneInfo.ConvertTime(
        DateTime.Now,
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
    );
}
