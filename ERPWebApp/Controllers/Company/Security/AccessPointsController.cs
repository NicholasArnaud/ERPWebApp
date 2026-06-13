using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company.Security;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class AccessPointsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

    public AccessPointsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AccessPoints
    public async Task<IActionResult> Index()
    {
        return View(await _context.AccessPoint.ToListAsync());
    }

    // GET: AccessPoints/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPoint = await _context.AccessPoint
            .FirstOrDefaultAsync(m => m.AccessPointId == id);
        if (accessPoint == null)
        {
            return NotFound();
        }

        return View(accessPoint);
    }

    // GET: AccessPoints/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: AccessPoints/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("AccessPointId,AccessPointLocation,IpAddress,MacAddress,SerialNumber,Status,IsActive")] AccessPoint accessPoint)
    {
        if (ModelState.IsValid)
        {
            accessPoint.CreationDate = now;
            accessPoint.CreatedBy = User.Identity.Name;
            _context.Add(accessPoint);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(accessPoint);
    }

    // GET: AccessPoints/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPoint = await _context.AccessPoint.FindAsync(id);
        if (accessPoint == null)
        {
            return NotFound();
        }
        return View(accessPoint);
    }

    // POST: AccessPoints/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int id, [Bind("AccessPointId,AccessPointLocation,IpAddress,MacAddress,SerialNumber,Status,IsActive")] AccessPoint accessPoint)
    {
        if (id != accessPoint.AccessPointId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(accessPoint);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPointExists(accessPoint.AccessPointId))
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
        return View(accessPoint);
    }

    // GET: AccessPoints/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessPoint = await _context.AccessPoint
            .FirstOrDefaultAsync(m => m.AccessPointId == id);
        if (accessPoint == null)
        {
            return NotFound();
        }

        return View(accessPoint);
    }

    // POST: AccessPoints/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessPoint = await _context.AccessPoint.FindAsync(id);
        _context.AccessPoint.Remove(accessPoint);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessPointExists(int id)
    {
        return _context.AccessPoint.Any(e => e.AccessPointId == id);
    }
}
