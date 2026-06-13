using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class DeputyTimeSheetsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DeputyTimeSheetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DeputyTimeSheets
    public async Task<IActionResult> Index()
    {
        return View(await _context.DeputyTimeSheet.ToListAsync());
    }

    // GET: DeputyTimeSheets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var deputyTimeSheet = await _context.DeputyTimeSheet
            .FirstOrDefaultAsync(m => m.DeputyTimeSheetId == id);
        if (deputyTimeSheet == null)
        {
            return NotFound();
        }

        return View(deputyTimeSheet);
    }

    // GET: DeputyTimeSheets/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DeputyTimeSheets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DeputyTimeSheetId,DeputyId,DeputyEmployeeId,EmployeeHistory,FirstName,LastName,DisplayName,Department,StartTime,StartTimeLocalized,EndTime,EndTimeLocalized,IsInProgress,Date,MealBreak,TotalTime,TotalTimeInv,Created,Modified")] DeputyTimeSheet deputyTimeSheet)
    {
        if (ModelState.IsValid)
        {
            _context.Add(deputyTimeSheet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(deputyTimeSheet);
    }

    // GET: DeputyTimeSheets/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var deputyTimeSheet = await _context.DeputyTimeSheet.FindAsync(id);
        if (deputyTimeSheet == null)
        {
            return NotFound();
        }
        return View(deputyTimeSheet);
    }

    // POST: DeputyTimeSheets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DeputyTimeSheetId,DeputyId,DeputyEmployeeId,EmployeeHistory,FirstName,LastName,DisplayName,Department,StartTime,StartTimeLocalized,EndTime,EndTimeLocalized,IsInProgress,Date,MealBreak,TotalTime,TotalTimeInv,Created,Modified")] DeputyTimeSheet deputyTimeSheet)
    {
        if (id != deputyTimeSheet.DeputyTimeSheetId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(deputyTimeSheet);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeputyTimeSheetExists(deputyTimeSheet.DeputyTimeSheetId))
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
        return View(deputyTimeSheet);
    }

    // GET: DeputyTimeSheets/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var deputyTimeSheet = await _context.DeputyTimeSheet
            .FirstOrDefaultAsync(m => m.DeputyTimeSheetId == id);
        if (deputyTimeSheet == null)
        {
            return NotFound();
        }

        return View(deputyTimeSheet);
    }

    // POST: DeputyTimeSheets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deputyTimeSheet = await _context.DeputyTimeSheet.FindAsync(id);
        _context.DeputyTimeSheet.Remove(deputyTimeSheet);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DeputyTimeSheetExists(int id)
    {
        return _context.DeputyTimeSheet.Any(e => e.DeputyTimeSheetId == id);
    }
}
