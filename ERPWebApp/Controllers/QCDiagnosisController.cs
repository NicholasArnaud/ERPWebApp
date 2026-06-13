using ERPWebApp.Data;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.QCBasic)]
[AutoValidateAntiforgeryToken]
public class QCDiagnosisController : Controller
{
    private readonly ApplicationDbContext _context;

    public QCDiagnosisController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: QCDiagnosis
    public async Task<IActionResult> Index()
    {
        return View(await _context.QCDiagnosis.ToListAsync());
    }

    // GET: QCDiagnosis/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qCDiagnosis = await _context.QCDiagnosis
            .FirstOrDefaultAsync(m => m.QCDiagnosisId == id);
        if (qCDiagnosis == null)
        {
            return NotFound();
        }

        return View(qCDiagnosis);
    }

    // GET: QCDiagnosis/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public IActionResult Create()
    {
        return View();
    }

    // POST: QCDiagnosis/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("QCDiagnosisId,QCDiagnosisName,IsActive")] QCDiagnosis qCDiagnosis)
    {
        if (ModelState.IsValid)
        {
            _context.Add(qCDiagnosis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(qCDiagnosis);
    }

    // GET: QCDiagnosis/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var qCDiagnosis = await _context.QCDiagnosis.FindAsync(id);
        if (qCDiagnosis == null)
        {
            return NotFound();
        }
        return View(qCDiagnosis);
    }

    // POST: QCDiagnosis/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.QCManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("QCDiagnosisId,QCDiagnosisName,IsActive")] QCDiagnosis qCDiagnosis)
    {
        if (id != qCDiagnosis.QCDiagnosisId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(qCDiagnosis);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QCDiagnosisExists(qCDiagnosis.QCDiagnosisId))
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
        return View(qCDiagnosis);
    }

    private bool QCDiagnosisExists(int id)
    {
        return _context.QCDiagnosis.Any(e => e.QCDiagnosisId == id);
    }
}
