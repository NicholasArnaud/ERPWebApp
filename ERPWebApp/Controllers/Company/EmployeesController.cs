using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ERPWebApp.Controllers.Company;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic)]
[AutoValidateAntiforgeryToken]
public class EmployeesController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserService _userService;

    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    private static List<Employee> _employeeDbFull = new();


    public EmployeesController(
        UserManager<IdentityUser> userManager,
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IUserService userService
    )
    {
        _userManager = userManager;
        _employeeService = employeeService;
        _departmentService = departmentService;
        _userService = userService;
    }

    // GET: Employees
    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetListAsync(e => e.JobStatus != JobStatus.Terminated, includes: [ x => x.Department ]);
        return View(employees);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public List<Employee> ToggleActive(bool id)
    {
        if (id)
        {
            var query = (IQueryable<Employee> employees) => employees
                .Where(x => x.JobStatus != JobStatus.Terminated)
                .Include(x => x.Department)
                .OrderBy(x => x.FirstName);

            _employeeDbFull = _employeeService.GetList(query);
            return _employeeDbFull;
        }

        var queryAll = (IQueryable<Employee> employees) => employees
            .Include(x => x.Department)
            .OrderBy(x => x.FirstName);

        _employeeDbFull = _employeeService.GetList(queryAll);
        return _employeeDbFull;
    }

    [HttpGet]
    public IActionResult PartialViewTableShow()
    {
        return PartialView("PartialIndex", _employeeDbFull);
    }

    // GET: Employees/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _employeeService.GetAsync(e => e.EmployeeId ==  id, includes: [ e => e.Department ]);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // GET: Employees/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public IActionResult Create()
    {
        var manusers = _userManager.Users.ToList();
        var userRolesViewModel = new List<UserRolesViewModel>();

        var thisViewModel = new UserRolesViewModel();
        thisViewModel.UserId = "Reset";
        thisViewModel.Email = " ";
        thisViewModel.UserName = " ";
        userRolesViewModel.Add(thisViewModel);

        foreach (IdentityUser user in manusers)
        {
            if (!_employeeService.IsExists(e => e.ApsuId == user.Id))
            {
                thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.UserName = user.UserName + " | " + user.Email;// + " | " + user.Id;
                userRolesViewModel.Add(thisViewModel);
            }
        }

        ViewData["UserRolesViewModel"] = new SelectList(userRolesViewModel.OrderBy(x => x.UserName), "UserId", "UserName", ViewBag.UserRolesViewModel);

        GetDepartments(null);

        return View();
    }

    private void GetDepartments(int? departmentId)
    {
        var departments = _departmentService.GetList(
            (IQueryable<Department> d) => d.Where(x=>x.IsActive)
                .Select(x=> new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                })
        );
        ViewData["Department"] = new SelectList(departments, "Value", "Text", departmentId); 
    }

    // POST: Employees/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,MiddleName,LastName,apsuId,UserRolesViewModelId,Position,DepartmentId,PhoneNumber,CompanyEmail,PersonalEmail,JobStatus,IncomePerHour,EmployeeReferenceNumber,IsActive")] Employee employee)
    {
        if (ModelState.IsValid)
        {
            string MiddleNameForFull = employee.MiddleName;
            if (MiddleNameForFull == "")
                MiddleNameForFull = " ";
            else
                MiddleNameForFull = " " + employee.MiddleName + " ";
            employee.FullName = employee.FirstName.Trim() + MiddleNameForFull.Trim() + employee.LastName.Trim();
            employee.ModifyDate = now;
            employee.ModifyBy = User.Identity.Name;

            if (_employeeService.IsExists(e => e.FullName == employee.FullName))
            {
                ModelState.AddModelError("", "Employee with the same name already exists.");
                return View(employee);
            }

            if (employee.CompanyEmail != null)
            {
                Regex re = new Regex(@"^[a-zA-Z0-9._%+-]+(@completeful.com|@ERP.com)$");
                bool IsERPEmail = re.IsMatch(employee.CompanyEmail);
                if (!IsERPEmail)
                {
                    ModelState.AddModelError("", "The Email needs to be part of the @ERP.com domain.");
                    return View(employee);
                }
            }
            if (employee.UserRolesViewModelId != "Reset" && employee.UserRolesViewModelId != null)
            {
                employee.ApsuId = employee.UserRolesViewModelId;
            }
            else
            {
                if ((employee.UserRolesViewModelId == "Reset" || employee.UserRolesViewModelId == null) && employee.CompanyEmail == null)
                {
                    ModelState.AddModelError("", "You are required to either add an existing user or provide a company email to create a new user.");
                    return View(employee);
                }

                if ((employee.UserRolesViewModelId == "Reset" || employee.UserRolesViewModelId == null) && employee.CompanyEmail != null)
                {
                    bool isExistUser = _employeeService.IsExists(e => e.CompanyEmail == employee.CompanyEmail);

                    if (isExistUser)
                    {
                        ModelState.AddModelError("", "This company email is already registered. You cannot create a new user with this email. Please use the existing user or verify the company email again.");
                        return View(employee);
                    }
                    else
                    {
                        var userResult = await _userService.CreateUserForEmployee(employee.CompanyEmail, employee.DepartmentId);
                        if (!userResult.Succeeded)
                        {
                            ModelState.AddModelError("", string.Join(", ", userResult.Errors.Select(e => e.Description)));
                            return View(employee);
                        }
                        var user = await _userManager.FindByEmailAsync(employee.CompanyEmail);
                        employee.ApsuId = user.Id;
                    }
                }
            }          
            employee.UserRolesViewModelId = null;

            _ = await _employeeService.AddAsync(employee);

            return RedirectToAction(nameof(Index));
        }

        GetDepartments(employee.DepartmentId);
        return View(employee);
    }

    // GET: Employees/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _employeeService.GetAsync(e => e.EmployeeId == id);
        if (employee == null)
        {
            return NotFound();
        }

        var manusers = _userManager.Users.ToList();
        var userRolesViewModel = new List<UserRolesViewModel>();


        var thisViewModel = new UserRolesViewModel();
        thisViewModel.UserId = "Reset";
        thisViewModel.Email = " ";
        thisViewModel.UserName = " ";
        userRolesViewModel.Add(thisViewModel);

        var currentUserName = thisViewModel.UserId;

        foreach (IdentityUser user in manusers)
        {
            if (!_employeeService.IsExists(e => e.ApsuId == user.Id) || user.Id == employee.ApsuId)
            {
                thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.UserName = user.UserName + " | " + user.Email;// + " | " + user.Id;
                userRolesViewModel.Add(thisViewModel);
            }
            if (user.Id == employee.ApsuId)
            {
                currentUserName = thisViewModel.UserId;
            }
        }

        await Task.Run(() => GetDepartments(employee.DepartmentId));
        ViewData["UserRolesViewModel"] = new SelectList(userRolesViewModel.OrderBy(x => x.UserName), "UserId", "UserName", currentUserName);
        return View(employee);
    }

    // POST: Employees/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,MiddleName,LastName,apsuId,UserRolesViewModelId,Position,DepartmentId,PhoneNumber,CompanyEmail,PersonalEmail,JobStatus,IncomePerHour,EmployeeReferenceNumber,IsActive")] Employee employee)
    {
        if (id != employee.EmployeeId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                string MiddleNameForFull = employee.MiddleName;
                if (MiddleNameForFull == "")
                    MiddleNameForFull = " ";
                else
                    MiddleNameForFull = " " + employee.MiddleName + " ";
                employee.FullName = employee.FirstName + MiddleNameForFull + employee.LastName;
                employee.ModifyDate = now;
                employee.ModifyBy = User.Identity.Name;

                if (employee.UserRolesViewModelId == "Reset")
                {
                    employee.ApsuId = null;
                }
                else if (employee.UserRolesViewModelId != null)
                {
                    employee.ApsuId = employee.UserRolesViewModelId;
                }

                employee.UserRolesViewModelId = null;

                _ = await _employeeService.UpdateAsync(employee);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_employeeService.IsExists(e => e.EmployeeId == employee.EmployeeId))
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

        return View(employee);
    }

    // GET: Employees/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _employeeService.GetAsync(e => e.EmployeeId == id, includes: [ e=>  e.Department ]);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // POST: Employees/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _ = await _employeeService.RemoveAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
