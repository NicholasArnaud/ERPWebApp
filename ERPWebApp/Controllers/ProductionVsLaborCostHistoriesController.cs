using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class ProductionVsLaborCostHistoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductionVsLaborCostHistoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ProductionVsLaborCostHistories
    public async Task<IActionResult> Index()
    {
        return View(await _context.ProductionVsLaborCostHistory.ToListAsync());
    }

    // GET: ProductionVsLaborCostHistories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostHistory = await _context.ProductionVsLaborCostHistory
            .FirstOrDefaultAsync(m => m.ProductionVsLaborCostHistoryId == id);
        if (productionVsLaborCostHistory == null)
        {
            return NotFound();
        }

        return View(productionVsLaborCostHistory);
    }

    // GET: ProductionVsLaborCostHistories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ProductionVsLaborCostHistories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("ProductionVsLaborCostHistoryId,Date,EmbroideryItemCost,EngravingItemCost,MetalTotalItemCost,UVPTotalItemCost,EmbroideryProdCost,EngravingProdCost,MetalTotalProdCost,UVPTotalProdCost")] ProductionVsLaborCostHistory productionVsLaborCostHistory)
    {
        if (ModelState.IsValid)
        {
            _context.Add(productionVsLaborCostHistory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(productionVsLaborCostHistory);
    }

    // GET: ProductionVsLaborCostHistories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostHistory = await _context.ProductionVsLaborCostHistory.FindAsync(id);
        if (productionVsLaborCostHistory == null)
        {
            return NotFound();
        }
        return View(productionVsLaborCostHistory);
    }

    // POST: ProductionVsLaborCostHistories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("ProductionVsLaborCostHistoryId,Date,EmbroideryItemCost,EngravingItemCost,MetalTotalItemCost,UVPTotalItemCost,EmbroideryProdCost,EngravingProdCost,MetalTotalProdCost,UVPTotalProdCost")] ProductionVsLaborCostHistory productionVsLaborCostHistory)
    {
        if (id != productionVsLaborCostHistory.ProductionVsLaborCostHistoryId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(productionVsLaborCostHistory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductionVsLaborCostHistoryExists(productionVsLaborCostHistory.ProductionVsLaborCostHistoryId))
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
        return View(productionVsLaborCostHistory);
    }

    // GET: ProductionVsLaborCostHistories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productionVsLaborCostHistory = await _context.ProductionVsLaborCostHistory
            .FirstOrDefaultAsync(m => m.ProductionVsLaborCostHistoryId == id);
        if (productionVsLaborCostHistory == null)
        {
            return NotFound();
        }

        return View(productionVsLaborCostHistory);
    }

    // POST: ProductionVsLaborCostHistories/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var productionVsLaborCostHistory = await _context.ProductionVsLaborCostHistory.FindAsync(id);
        _context.ProductionVsLaborCostHistory.Remove(productionVsLaborCostHistory);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductionVsLaborCostHistoryExists(int id)
    {
        return _context.ProductionVsLaborCostHistory.Any(e => e.ProductionVsLaborCostHistoryId == id);
    }
}
