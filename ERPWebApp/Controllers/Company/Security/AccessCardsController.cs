using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company.Security;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class AccessCardsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    private const string password = "AccessCardEncryption";

    public AccessCardsController(ApplicationDbContext context)
    {
        _context = context;
    }
    // GET: AccessCards
    public async Task<IActionResult> Index(bool isActive = true)
    {
        if (isActive == true)
        {
            var applicationDbContext = _context.AccessCard.Include(a => a.Employee).Where(a => a.Employee.JobStatus != JobStatus.Terminated).Select(a => new AccessCard
            {
                AccessCardId = a.AccessCardId,
                Employee = a.Employee,
                EmployeeId = a.EmployeeId,
                Key = SecurityEncryption.Decrypt(a.Key, password)
            }).OrderBy(x => x.Employee.FirstName);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AccessCardTablePartial", applicationDbContext);
            }

            ViewBag.IsActive = isActive;

            return View(await applicationDbContext.ToListAsync());
        }
        else
        {
            var applicationDbContext = _context.AccessCard.Include(a => a.Employee).Where(a => a.Employee.JobStatus == JobStatus.Terminated).Select(a => new AccessCard
            {
                AccessCardId = a.AccessCardId,
                Employee = a.Employee,
                EmployeeId = a.EmployeeId,
                Key = SecurityEncryption.Decrypt(a.Key, password)
            }).OrderBy(x => x.Employee.FirstName);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AccessCardTablePartial", applicationDbContext);
            }
            ViewBag.IsActive = isActive;

            return View(await applicationDbContext.ToListAsync());
        }          
    }
    // GET: AccessCards/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessCard = await _context.AccessCard
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(m => m.AccessCardId == id);
        if (accessCard == null)
        {
            return NotFound();
        }
        accessCard.Key = SecurityEncryption.Decrypt(accessCard.Key, password);
        return View(accessCard);
    }
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    // GET: AccessCards/Create
    public IActionResult Create()
    {
        ViewData["EmployeeId"] = new SelectList(_context.Employee.Where(e => e.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FirstName), "EmployeeId", "FullName");
        return View();
    }

    // POST: AccessCards/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Create([Bind("AccessCardId,EmployeeId,Key")] AccessCard accessCard)
    {
        if (ModelState.IsValid)
        {
            accessCard.CreationDate = now;
            accessCard.CreatedBy = User.Identity.Name;
            accessCard.Key = SecurityEncryption.Encrypt(accessCard.Key, password);
            _context.Add(accessCard);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["EmployeeId"] = new SelectList(_context.Employee.Where(e => e.JobStatus != JobStatus.Terminated).OrderBy(x => x.FirstName), "EmployeeId", "FullName", accessCard.EmployeeId);
        return View(accessCard);
    }
    // GET: AccessCards/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessCard = await _context.AccessCard.Include(a => a.Employee).FirstOrDefaultAsync(m => m.AccessCardId == id);
        if (accessCard == null)
        {
            return NotFound();
        }
        accessCard.Key = SecurityEncryption.Decrypt(accessCard.Key, password);
        ViewData["EmployeeId"] = new SelectList(_context.Employee.Where(e => e.JobStatus != JobStatus.Terminated), "EmployeeId", "FullName", accessCard.EmployeeId);
        return View(accessCard);
    }

    // POST: AccessCards/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("AccessCardId,EmployeeId,Key")] AccessCard accessCard)
    {
        if (id != accessCard.AccessCardId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                accessCard.Key = SecurityEncryption.Encrypt(accessCard.Key, password);
                _context.Update(accessCard);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessCardExists(accessCard.AccessCardId))
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
        ViewData["EmployeeId"] = new SelectList(_context.Employee, "EmployeeId", "FullName", accessCard.EmployeeId);
        return View(accessCard);
    }

    // GET: AccessCards/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var accessCard = await _context.AccessCard
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(m => m.AccessCardId == id);
        if (accessCard == null)
        {
            return NotFound();
        }
        accessCard.Key = SecurityEncryption.Decrypt(accessCard.Key, password);
        return View(accessCard);
    }

    // POST: AccessCards/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var accessCard = await _context.AccessCard.FindAsync(id);
        _context.AccessCard.Remove(accessCard);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AccessCardExists(int id)
    {
        return _context.AccessCard.Any(e => e.AccessCardId == id);
    }  
}
