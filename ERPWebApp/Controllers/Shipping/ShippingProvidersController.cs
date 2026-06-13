using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Shipping;

[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ShippingProvidersController : Controller
{
    private readonly ApplicationDbContext _context;
    public static ShippingMethodProviderView _shipMethodProvider = new ShippingMethodProviderView();
    public ShippingProvidersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        _shipMethodProvider.Providers = await _context.ShippingProvider.OrderBy(x => x.ShippingProviderName).ToListAsync();
        return View(_shipMethodProvider);
    }
    public class MethodProviderView
    {
        public IEnumerable<ShippingMethod> Method { get; set; }
        public IEnumerable<ShippingProvider> Provider { get; set; }
    }
    // GET: ShippingProviders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.ShippingProvider == null)
        {
            return NotFound();
        }

        var shippingProvider = await _context.ShippingProvider
            .FirstOrDefaultAsync(m => m.ShippingProviderId == id);
        if (shippingProvider == null)
        {
            return NotFound();
        }

        return View(shippingProvider);
    }

    // GET: ShippingProviders/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ShippingProviders/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> CreateProvider([Bind("Provider, Provider.ShippingProviderId,Provider.ShippingProviderName,Provider.IsActive,Provider.ModifyDate,Provider.ModifyByUser")] ShippingMethodProviderView shippingMethodProviderView)
    {
        if (ModelState.IsValid)
        {
            shippingMethodProviderView.Provider.ModifyDate = DateTime.Now;
            shippingMethodProviderView.Provider.ModifyByUser = this.User.Identity.Name;
            shippingMethodProviderView.Provider.IsActive = true;
            _context.Add(shippingMethodProviderView.Provider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: ShippingProviders/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.ShippingProvider == null)
        {
            return NotFound();
        }

        var shippingProvider = await _context.ShippingProvider.FindAsync(id);
        if (shippingProvider == null)
        {
            return NotFound();
        }
        return View(shippingProvider);
    }

    // POST: ShippingProviders/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("ShippingProviderId,ShippingProviderName,IsActive,ModifyDate,ModifyByUser")] ShippingProvider shippingProvider)
    {
        if (id != shippingProvider.ShippingProviderId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(shippingProvider);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShippingProviderExists(shippingProvider.ShippingProviderId))
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
        return View(shippingProvider);
    }

    // GET: ShippingProviders/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.ShippingProvider == null)
        {
            return NotFound();
        }

        var shippingProvider = await _context.ShippingProvider
            .FirstOrDefaultAsync(m => m.ShippingProviderId == id);
        if (shippingProvider == null)
        {
            return NotFound();
        }

        return View(shippingProvider);
    }

    // POST: ShippingProviders/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.ShippingProvider == null)
        {
            return Problem("Entity set 'ApplicationDbContext.ShippingProvider'  is null.");
        }
        var shippingProvider = await _context.ShippingProvider.FindAsync(id);
        if (shippingProvider != null)
        {
            _context.ShippingProvider.Remove(shippingProvider);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ShippingProviderExists(int id)
    {
        return _context.ShippingProvider.Any(e => e.ShippingProviderId == id);
    }
}
