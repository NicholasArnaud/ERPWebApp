using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company.Security;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class AccessPointLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccessPointLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AccessPointLogs
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.AccessPointLog.Include(a => a.AccessCard).Include(a => a.AccessPoint);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: AccessPointLogs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPointLog = await _context.AccessPointLog
            .Include(a => a.AccessCard)
            .Include(a => a.AccessPoint)
            .FirstOrDefaultAsync(m => m.AccessPointLogId == id);
        if (accessPointLog == null)
        {
            return NotFound();
        }

        return View(accessPointLog);
    }

    // GET: AccessPointLogs/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public IActionResult Create()
    {
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key");
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation");
        return View();
    }

    // POST: AccessPointLogs/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Create([Bind("AccessPointLogId,AccessCardId,AccessPointId,CreationDate")] AccessPointLog accessPointLog)
    {
        if (ModelState.IsValid)
        {
            _context.Add(accessPointLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPointLog.AccessCardId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPointLog.AccessPointId);
        return View(accessPointLog);
    }

    // GET: AccessPointLogs/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPointLog = await _context.AccessPointLog.FindAsync(id);
        if (accessPointLog == null)
        {
            return NotFound();
        }
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPointLog.AccessCardId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPointLog.AccessPointId);
        return View(accessPointLog);
    }

    // POST: AccessPointLogs/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("AccessPointLogId,AccessCardId,AccessPointId,CreationDate")] AccessPointLog accessPointLog)
    {
        if (id != accessPointLog.AccessPointLogId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(accessPointLog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPointLogExists(accessPointLog.AccessPointLogId))
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
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPointLog.AccessCardId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPointLog.AccessPointId);
        return View(accessPointLog);
    }

    // GET: AccessPointLogs/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPointLog = await _context.AccessPointLog
            .Include(a => a.AccessCard)
            .Include(a => a.AccessPoint)
            .FirstOrDefaultAsync(m => m.AccessPointLogId == id);
        if (accessPointLog == null)
        {
            return NotFound();
        }

        return View(accessPointLog);
    }

    // POST: AccessPointLogs/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessPointLog = await _context.AccessPointLog.FindAsync(id);
        _context.AccessPointLog.Remove(accessPointLog);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessPointLogExists(int id)
    {
        return _context.AccessPointLog.Any(e => e.AccessPointLogId == id);
    }
}
