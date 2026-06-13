using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.NirfForms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
[AutoValidateAntiforgeryToken]
public class FontsController : Controller
{
    private readonly ApplicationDbContext _context;
    private static DateTime _date = TimeZoneInfo.ConvertTime(
   DateTime.Now,
   TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
);
    public FontsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Fonts
    public async Task<IActionResult> Index()
    {
        return View(await _context.Fonts.ToListAsync());
    }

    // GET: Fonts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Fonts == null)
        {
            return NotFound();
        }

        var fonts = await _context.Fonts
            .FirstOrDefaultAsync(m => m.FontId == id);
        if (fonts == null)
        {
            return NotFound();
        }

        return View(fonts);
    }

    // GET: Fonts/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Fonts/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("FontId,FontTitle,ModifyDate,ModifyByUser,IsActive")] Fonts fonts)
    {
        if (ModelState.IsValid)
        {
            fonts.ModifyDate = _date;
            fonts.ModifyByUser = this.User.Identity.Name;
            _context.Add(fonts);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(fonts);
    }

    // GET: Fonts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Fonts == null)
        {
            return NotFound();
        }

        var fonts = await _context.Fonts.FindAsync(id);
        if (fonts == null)
        {
            return NotFound();
        }
        return View(fonts);
    }

    // POST: Fonts/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("FontId,FontTitle,ModifyDate,ModifyByUser,IsActive")] Fonts fonts)
    {
        if (id != fonts.FontId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                fonts.ModifyDate = _date;
                fonts.ModifyByUser = this.User.Identity?.Name;
                _context.Update(fonts);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FontsExists(fonts.FontId))
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
        return View(fonts);
    }

    // GET: Fonts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Fonts == null)
        {
            return NotFound();
        }

        var fonts = await _context.Fonts
            .FirstOrDefaultAsync(m => m.FontId == id);
        if (fonts == null)
        {
            return NotFound();
        }

        return View(fonts);
    }

    // POST: Fonts/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Fonts == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Fonts'  is null.");
        }
        var fonts = await _context.Fonts.FindAsync(id);
        if (fonts != null)
        {
            _context.Fonts.Remove(fonts);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FontsExists(int id)
    {
        return _context.Fonts.Any(e => e.FontId == id);
    }
}
