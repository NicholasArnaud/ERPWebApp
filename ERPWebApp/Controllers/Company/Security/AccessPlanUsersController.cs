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
public class AccessPlanUsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccessPlanUsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AccessPlanUsers
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.AccessPlanUser.Include(a => a.AccessCard).Include(a => a.AccessPlan);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: AccessPlanUsers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanUser = await _context.AccessPlanUser
            .Include(a => a.AccessCard)
            .Include(a => a.AccessPlan)
            .FirstOrDefaultAsync(m => m.AccessPlanUserId == id);
        if (accessPlanUser == null)
        {
            return NotFound();
        }

        return View(accessPlanUser);
    }

    // GET: AccessPlanUsers/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public IActionResult Create()
    {
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key");
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName");
        return View();
    }

    // POST: AccessPlanUsers/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Create([Bind("AccessPlanUserId,AccessCardId,AccessPlanId")] AccessPlanUser accessPlanUser)
    {
        if (ModelState.IsValid)
        {
            _context.Add(accessPlanUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPlanUser.AccessCardId);
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanUser.AccessPlanId);
        return View(accessPlanUser);
    }
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    // GET: AccessPlanUsers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanUser = await _context.AccessPlanUser.FindAsync(id);
        if (accessPlanUser == null)
        {
            return NotFound();
        }
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPlanUser.AccessCardId);
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanUser.AccessPlanId);
        return View(accessPlanUser);
    }

    // POST: AccessPlanUsers/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("AccessPlanUserId,AccessCardId,AccessPlanId")] AccessPlanUser accessPlanUser)
    {
        if (id != accessPlanUser.AccessPlanUserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(accessPlanUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPlanUserExists(accessPlanUser.AccessPlanUserId))
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
        ViewData["AccessCardId"] = new SelectList(_context.AccessCard, "AccessCardId", "Key", accessPlanUser.AccessCardId);
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanUser.AccessPlanId);
        return View(accessPlanUser);
    }

    // GET: AccessPlanUsers/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanUser = await _context.AccessPlanUser
            .Include(a => a.AccessCard)
            .Include(a => a.AccessPlan)
            .FirstOrDefaultAsync(m => m.AccessPlanUserId == id);
        if (accessPlanUser == null)
        {
            return NotFound();
        }

        return View(accessPlanUser);
    }

    // POST: AccessPlanUsers/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessPlanUser = await _context.AccessPlanUser.FindAsync(id);
        _context.AccessPlanUser.Remove(accessPlanUser);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessPlanUserExists(int id)
    {
        return _context.AccessPlanUser.Any(e => e.AccessPlanUserId == id);
    }
}
