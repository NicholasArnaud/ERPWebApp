using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic)]
[AutoValidateAntiforgeryToken]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: Departments
    public async Task<IActionResult> Index()
    {
        var result = await _departmentService.GetAllAsync();
        return View(result);
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _departmentService.GetAsync(x => x.DepartmentId == id.Value);
        return department != null ? View(department) : NotFound();
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    // GET: Departments/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Departments/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DepartmentId,DepartmentName,DepartmentColor,IsActive,IsProduction")] Department department)
    {
        if (ModelState.IsValid)
        {
            await _departmentService.AddAsync(department);
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _departmentService.GetAsync(x => x.DepartmentId == id.Value);
        return department != null ? View(department) : NotFound();
    }

    // POST: Departments/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentName,DepartmentColor,IsActive,IsProduction")] Department department)
    {
        if (id != department.DepartmentId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
              await  _departmentService.UpdateAsync(department);
            }
            catch (DbUpdateConcurrencyException)
            {
                var isExists = _departmentService.IsExists(x=>x.DepartmentId == department.DepartmentId);
                if (!isExists)
                    return NotFound();
                else
                    return ValidationProblem();
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }
}
