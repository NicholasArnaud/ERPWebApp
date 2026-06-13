using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company.Security;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class AccessPlansController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    public AccessPlansController(ApplicationDbContext context)
    {
        _context = context;
    }
    // GET: AccessPlans
    public async Task<IActionResult> Index()
    {
        return View(await _context.AccessPlan.ToListAsync());
    }
    // GET: AccessPlans/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlan = await _context.AccessPlan
            .FirstOrDefaultAsync(m => m.AccessPlanId == id);
        if (accessPlan == null)
        {
            return NotFound();
        }

        return View(accessPlan);
    }
    // GET: AccessPlans/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: AccessPlans/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Create([Bind("AccessPlanId,AccessPlanName,IsActive")] AccessPlan accessPlan)
    {
        if (ModelState.IsValid)
        {
            accessPlan.CreationDate = now;
            accessPlan.CreatedBy = User.Identity.Name;
            accessPlan.ModifyDate = now;
            accessPlan.ModifyByUser = User.Identity.Name;
            _context.Add(accessPlan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(accessPlan);
    }
    // GET: AccessPlans/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlan = await _context.AccessPlan.FindAsync(id);
        if (accessPlan == null)
        {
            return NotFound();
        }
        return View(accessPlan);
    }
    // POST: AccessPlans/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("AccessPlanId,AccessPlanName,IsActive")] AccessPlan accessPlan)
    {
        if (id != accessPlan.AccessPlanId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                accessPlan.ModifyDate = now;
                accessPlan.ModifyByUser = User.Identity.Name;
                _context.Update(accessPlan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPlanExists(accessPlan.AccessPlanId))
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
        return View(accessPlan);
    }
    // GET: AccessPlans/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPlan = await _context.AccessPlan
            .FirstOrDefaultAsync(m => m.AccessPlanId == id);
        if (accessPlan == null)
        {
            return NotFound();
        }

        return View(accessPlan);
    }
    // POST: AccessPlans/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessPlan = await _context.AccessPlan.FindAsync(id);
        _context.AccessPlan.Remove(accessPlan);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessPlanExists(int id)
    {
        return _context.AccessPlan.Any(e => e.AccessPlanId == id);
    }
}
