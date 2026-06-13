using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.QCManager)]// Administrator,Manager,Financial Basic")]
[AutoValidateAntiforgeryToken]
public class QualityControlCapturesController : Controller
{
    private readonly ApplicationDbContext _context;

    public QualityControlCapturesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: QualityControlCaptures
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.QualityControlCapture.Include(q => q.Departments).Include(q => q.Employees).Include(q => q.Locations).Include(q => q.Diagnoses);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: QualityControlCaptures/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qualityControlCapture = await _context.QualityControlCapture
            .Include(q => q.Departments)
            .Include(q => q.Employees)
            .Include(q => q.Locations)
            .Include(q => q.Diagnoses)
            .FirstOrDefaultAsync(m => m.QualityControlCaptureId == id);
        if (qualityControlCapture == null)
        {
            return NotFound();
        }

        return View(qualityControlCapture);
    }
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    // GET: QualityControlCaptures/Create
    public IActionResult Create()
    {
        var selectListItems = _context.Employee.Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + " - " + c.EmployeeReferenceNumber
        });

        ViewData["QCDiagnosisId"] = new SelectList(_context.QCDiagnosis.Where(x => x.IsActive), "QCDiagnosisId", "QCDiagnosisName");
        ViewData["DepartmentId"] = new SelectList(_context.Department.Where(x => x.IsActive), "DepartmentId", "DepartmentName");
        ViewData["EmployeeId"] = new SelectList(selectListItems, "Value", "Text");
        ViewData["QCStationLocationId"] = new SelectList(_context.QCStationLocation.Where(x => x.IsActive), "QCStationLocationId", "QCStationLocationName");
        return View();
    }

    // POST: QualityControlCaptures/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Create([Bind("QualityControlCaptureId,OrderNumber,QCStationLocationId,DepartmentId,SkuNumber,OrderDate,QCDiagnosisId,EmployeeId,CaptureDate,QCPerson,Quantity,BatchNumber,Notes")] QualityControlCapture qualityControlCapture)
    {
        if (ModelState.IsValid)
        {
            _context.Add(qualityControlCapture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        var selectListItems = _context.Employee.Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + " - " + c.EmployeeReferenceNumber
        });
        ViewData["QCDiagnosisId"] = new SelectList(_context.QCDiagnosis.Where(x => x.IsActive), "QCDiagnosisId", "QCDiagnosisName", qualityControlCapture.QCDiagnosisId);
        ViewData["DepartmentId"] = new SelectList(_context.Department.Where(x => x.IsActive), "DepartmentId", "DepartmentName", qualityControlCapture.DepartmentId);
        ViewData["EmployeeId"] = new SelectList(selectListItems, "Value", "Text", qualityControlCapture.EmployeeId);
        ViewData["QCStationLocationId"] = new SelectList(_context.QCStationLocation.Where(x => x.IsActive), "QCStationLocationId", "QCStationLocationName", qualityControlCapture.QCStationLocationId);
        return View(qualityControlCapture);
    }

    // GET: QualityControlCaptures/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qualityControlCapture = await _context.QualityControlCapture.FindAsync(id);
        if (qualityControlCapture == null)
        {
            return NotFound();
        }
        var selectListItems = _context.Employee.Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + " - " + c.EmployeeReferenceNumber
        });
        ViewData["QCDiagnosisId"] = new SelectList(_context.QCDiagnosis.Where(x => x.IsActive), "QCDiagnosisId", "QCDiagnosisName", qualityControlCapture.QCDiagnosisId);
        ViewData["DepartmentId"] = new SelectList(_context.Department.Where(x => x.IsActive), "DepartmentId", "DepartmentName", qualityControlCapture.DepartmentId);
        ViewData["EmployeeId"] = new SelectList(selectListItems, "Value", "Text", qualityControlCapture.EmployeeId);
        ViewData["QCStationLocationId"] = new SelectList(_context.QCStationLocation.Where(x => x.IsActive), "QCStationLocationId", "QCStationLocationName", qualityControlCapture.QCStationLocationId);
        return View(qualityControlCapture);
    }

    // POST: QualityControlCaptures/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Edit(int id, [Bind("QualityControlCaptureId,OrderNumber,QCStationLocationId,DepartmentId,SkuNumber,OrderDate,QCDiagnosisId,EmployeeId,CaptureDate,QCPerson,Quantity,BatchNumber,Notes")] QualityControlCapture qualityControlCapture)
    {
        if (id != qualityControlCapture.QualityControlCaptureId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(qualityControlCapture);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QualityControlCaptureExists(qualityControlCapture.QualityControlCaptureId))
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
        var selectListItems = _context.Employee.Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + " - " + c.EmployeeReferenceNumber
        });
        ViewData["QCDiagnosisId"] = new SelectList(_context.QCDiagnosis.Where(x => x.IsActive), "QCDiagnosisId", "QCDiagnosisName", qualityControlCapture.QCDiagnosisId);
        ViewData["DepartmentId"] = new SelectList(_context.Department.Where(x => x.IsActive), "DepartmentId", "DepartmentName", qualityControlCapture.DepartmentId);
        ViewData["EmployeeId"] = new SelectList(selectListItems, "Value", "Text", qualityControlCapture.EmployeeId);
        ViewData["QCStationLocationId"] = new SelectList(_context.QCStationLocation.Where(x => x.IsActive), "QCStationLocationId", "QCStationLocationName", qualityControlCapture.QCStationLocationId);
        return View(qualityControlCapture);
    }

    // GET: QualityControlCaptures/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qualityControlCapture = await _context.QualityControlCapture
            .Include(q => q.Departments)
            .Include(q => q.Employees)
            .Include(q => q.Locations)
            .Include(q => q.Diagnoses)
            .FirstOrDefaultAsync(m => m.QualityControlCaptureId == id);
        if (qualityControlCapture == null)
        {
            return NotFound();
        }

        return View(qualityControlCapture);
    }

    // POST: QualityControlCaptures/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var qualityControlCapture = await _context.QualityControlCapture.FindAsync(id);
        _context.QualityControlCapture.Remove(qualityControlCapture);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool QualityControlCaptureExists(int id)
    {
        return _context.QualityControlCapture.Any(e => e.QualityControlCaptureId == id);
    }

    [HttpGet]
    [ProducesResponseType(200,
        Type = typeof(IEnumerable<ShipStationAwaitingOrder>))]
    public async Task<IEnumerable<ShipStationAwaitingOrder>> GetItemsByOrderNumber(string OrderNumber)
    {
        var itemList = await _context.ShipStationAwaitingOrder.Where(x => x.OrderNumber.Equals(OrderNumber)).ToListAsync();
        return itemList;
    }
}
