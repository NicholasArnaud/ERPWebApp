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
public class AccessPlanDoorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccessPlanDoorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AccessPlanDoors
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.AccessPlanDoor.Include(a => a.AccessPlan).Include(a => a.AccessPoint);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: AccessPlanDoors/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanDoor = await _context.AccessPlanDoor
            .Include(a => a.AccessPlan)
            .Include(a => a.AccessPoint)
            .FirstOrDefaultAsync(m => m.AccessPlanDoorId == id);
        if (accessPlanDoor == null)
        {
            return NotFound();
        }

        return View(accessPlanDoor);
    }

    // GET: AccessPlanDoors/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public IActionResult Create()
    {
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName");
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation");
        return View();
    }

    // POST: AccessPlanDoors/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("AccessPlanDoorId,AccessPlanId,AccessPointId")] AccessPlanDoor accessPlanDoor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(accessPlanDoor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanDoor.AccessPlanId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPlanDoor.AccessPointId);
        return View(accessPlanDoor);
    }

    // GET: AccessPlanDoors/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanDoor = await _context.AccessPlanDoor.FindAsync(id);
        if (accessPlanDoor == null)
        {
            return NotFound();
        }
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanDoor.AccessPlanId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPlanDoor.AccessPointId);
        return View(accessPlanDoor);
    }

    // POST: AccessPlanDoors/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("AccessPlanDoorId,AccessPlanId,AccessPointId")] AccessPlanDoor accessPlanDoor)
    {
        if (id != accessPlanDoor.AccessPlanDoorId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(accessPlanDoor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPlanDoorExists(accessPlanDoor.AccessPlanDoorId))
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
        ViewData["AccessPlanId"] = new SelectList(_context.AccessPlan, "AccessPlanId", "AccessPlanName", accessPlanDoor.AccessPlanId);
        ViewData["AccessPointId"] = new SelectList(_context.AccessPoint, "AccessPointId", "AccessPointLocation", accessPlanDoor.AccessPointId);
        return View(accessPlanDoor);
    }

    // GET: AccessPlanDoors/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlanDoor = await _context.AccessPlanDoor
            .Include(a => a.AccessPlan)
            .Include(a => a.AccessPoint)
            .FirstOrDefaultAsync(m => m.AccessPlanDoorId == id);
        if (accessPlanDoor == null)
        {
            return NotFound();
        }

        return View(accessPlanDoor);
    }

    // POST: AccessPlanDoors/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessPlanDoor = await _context.AccessPlanDoor.FindAsync(id);
        _context.AccessPlanDoor.Remove(accessPlanDoor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessPlanDoorExists(int id)
    {
        return _context.AccessPlanDoor.Any(e => e.AccessPlanDoorId == id);
    }
}
