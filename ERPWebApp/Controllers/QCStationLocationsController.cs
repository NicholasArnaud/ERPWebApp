using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.QCManager)]
[AutoValidateAntiforgeryToken]
public class QCStationLocationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public QCStationLocationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: QCStationLocations
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.QCStationLocation.Include(q => q.Departments);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: QCStationLocations/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qCStationLocation = await _context.QCStationLocation
            .Include(q => q.Departments)
            .FirstOrDefaultAsync(m => m.QCStationLocationId == id);
        if (qCStationLocation == null)
        {
            return NotFound();
        }

        return View(qCStationLocation);
    }

    // GET: QCStationLocations/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager + "," + RoleList.QCManager)]
    public IActionResult Create()
    {
        ViewData["Department"] = new SelectList(_context.Department, "DepartmentId", "DepartmentName");
        return View();
    }

    // POST: QCStationLocations/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Create([Bind("QCStationLocationId,QCStationLocationName,DepartmentId,DepartmentName,IsActive")] QCStationLocation qCStationLocation)
    {
        if (ModelState.IsValid)
        {
            _context.Add(qCStationLocation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["Department"] = new SelectList(_context.Department, "DepartmentId", "DepartmentName", qCStationLocation.DepartmentId);
        return View(qCStationLocation);
    }

    // GET: QCStationLocations/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qCStationLocation = await _context.QCStationLocation.FindAsync(id);
        if (qCStationLocation == null)
        {
            return NotFound();
        }
        ViewData["Department"] = new SelectList(_context.Department, "DepartmentId", "DepartmentName", qCStationLocation.DepartmentId);
        return View(qCStationLocation);
    }

    // POST: QCStationLocations/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("QCStationLocationId,QCStationLocationName,DepartmentId,DepartmentName,IsActive")] QCStationLocation qCStationLocation)
    {
        if (id != qCStationLocation.QCStationLocationId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(qCStationLocation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QCStationLocationExists(qCStationLocation.QCStationLocationId))
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
        ViewData["Department"] = new SelectList(_context.Department, "DepartmentId", "DepartmentName", qCStationLocation.DepartmentId);
        return View(qCStationLocation);
    }

    private bool QCStationLocationExists(int id)
    {
        return _context.QCStationLocation.Any(e => e.QCStationLocationId == id);
    }
}
