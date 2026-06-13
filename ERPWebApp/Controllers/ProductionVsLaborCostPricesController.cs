using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class ProductionVsLaborCostPricesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductionVsLaborCostPricesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ProductionVsLaborCostPrices
    public async Task<IActionResult> Index()
    {
        return View(await _context.ProductionVsLaborCostPrice.ToListAsync());
    }

    // GET: ProductionVsLaborCostPrices/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostPrice = await _context.ProductionVsLaborCostPrice
            .FirstOrDefaultAsync(m => m.ProductionVsLaborCostPriceId == id);
        if (productionVsLaborCostPrice == null)
        {
            return NotFound();
        }

        return View(productionVsLaborCostPrice);
    }

    // GET: ProductionVsLaborCostPrices/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ProductionVsLaborCostPrices/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("ProductionVsLaborCostPriceId,EmbroideryItemCost,EngravingItemCost,MetalItemCost,UVItemCost")] ProductionVsLaborCostPrice productionVsLaborCostPrice)
    {
        if (ModelState.IsValid)
        {
            _context.Add(productionVsLaborCostPrice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(productionVsLaborCostPrice);
    }

    // GET: ProductionVsLaborCostPrices/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostPrice = await _context.ProductionVsLaborCostPrice.FindAsync(id);
        if (productionVsLaborCostPrice == null)
        {
            return NotFound();
        }
        return View(productionVsLaborCostPrice);
    }

    // POST: ProductionVsLaborCostPrices/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("ProductionVsLaborCostPriceId,EmbroideryItemCost,EngravingItemCost,MetalItemCost,UVItemCost,ModifyDate,ModifyByUser")] ProductionVsLaborCostPrice productionVsLaborCostPrice)
    {
        if (id != productionVsLaborCostPrice.ProductionVsLaborCostPriceId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(productionVsLaborCostPrice);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductionVsLaborCostPriceExists(productionVsLaborCostPrice.ProductionVsLaborCostPriceId))
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
        return View(productionVsLaborCostPrice);
    }

    // GET: ProductionVsLaborCostPrices/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostPrice = await _context.ProductionVsLaborCostPrice
            .FirstOrDefaultAsync(m => m.ProductionVsLaborCostPriceId == id);
        if (productionVsLaborCostPrice == null)
        {
            return NotFound();
        }

        return View(productionVsLaborCostPrice);
    }

    // POST: ProductionVsLaborCostPrices/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var productionVsLaborCostPrice = await _context.ProductionVsLaborCostPrice.FindAsync(id);
        _context.ProductionVsLaborCostPrice.Remove(productionVsLaborCostPrice);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductionVsLaborCostPriceExists(int id)
    {
        return _context.ProductionVsLaborCostPrice.Any(e => e.ProductionVsLaborCostPriceId == id);
    }
}
