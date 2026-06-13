using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Config;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.HELP_REQUEST)]
[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.BasicUser)]
[AutoValidateAntiforgeryToken]
public class HelpRequestFormsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IGraphAPIService _graphAPIService;
    private readonly ExternalEndpoints _endpoints;

    public HelpRequestFormsController(ApplicationDbContext context, IGraphAPIService graphAPIService, IOptions<ExternalEndpoints> endpoints)
    {
        _context = context;
        _graphAPIService = graphAPIService;
        _endpoints = endpoints.Value;
    }

    // GET: HelpRequestForms
    public async Task<IActionResult> Index()
    {
        var currentUser = User.Identity.Name;

        var applicationDbContext = _context.HelpRequestForm
            .Include(h => h.HelperEmployee)
            .Include(h => h.RequestingEmployee).OrderBy(h => h.HelpRequestFormId);
        var individualizedIndex = applicationDbContext.Where(h => h.HelperEmployee.CompanyEmail.Contains(currentUser) || h.RequestedByUser == currentUser);

        if (User.IsInRole(RoleList.Administrator))
        {
            return View(await applicationDbContext.ToListAsync());
        }
        else
        {
            return View(await individualizedIndex.ToListAsync());
        }

    }

    // GET: HelpRequestForms/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var helpRequestForm = await _context.HelpRequestForm
            .Include(h => h.HelperEmployee)
            .Include(h => h.RequestingEmployee)
            .FirstOrDefaultAsync(m => m.HelpRequestFormId == id);
        if (helpRequestForm == null)
        {
            return NotFound();
        }

        return View(helpRequestForm);
    }

    // GET: HelpRequestForms/Create
    public IActionResult Create()
    {

        var selectListEmployees = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName
        });
        var selectListHelpers = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + "-" + c.Department.DepartmentName
        });

        ViewData["HelperEmployeeId"] = new SelectList(selectListHelpers, "Value", "Text");
        ViewData["RequestingEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text");
        return View();
    }

    // POST: HelpRequestForms/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("HelpRequestFormId,Subject,RequestingEmployeeId,Description,Urgency,HelperEmployeeId")] HelpRequestForm helpRequestForm)
    {
        if (ModelState.IsValid)
        {
            helpRequestForm.CreatedDate = DateTime.Now;
            helpRequestForm.RequestedByUser = User.Identity.Name;
            _context.Add(helpRequestForm);
            await _context.SaveChangesAsync();
            await Emailer(helpRequestForm, 1);

            return RedirectToAction(nameof(Index));
        }

        var selectListEmployees = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName
        });
        var selectListHelpers = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + "-" + c.Department.DepartmentName
        });

        ViewData["HelperEmployeeId"] = new SelectList(selectListHelpers, "Value", "Text", helpRequestForm.HelperEmployeeId);
        ViewData["RequestingEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text", helpRequestForm.RequestingEmployeeId);

        return View(helpRequestForm);
    }

    // GET: HelpRequestForms/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var currentUser = User.Identity.Name;

        if (id == null)
        {
            return NotFound();
        }

        var helpRequestForm = await _context.HelpRequestForm.FindAsync(id);
        if (helpRequestForm == null)
        {
            return NotFound();
        }
        if (helpRequestForm.IsComplete)
        {
            return Redirect("~/Identity/Account/AccessDenied");
        }

        var employees = await _context.Employee.Where(i => i.EmployeeId == helpRequestForm.RequestingEmployeeId || i.EmployeeId == helpRequestForm.HelperEmployeeId).ToListAsync();
        helpRequestForm.HelperEmployee = employees.Where(i => i.EmployeeId == helpRequestForm.HelperEmployeeId).Single();
        helpRequestForm.RequestingEmployee = employees.Where(i => i.EmployeeId == helpRequestForm.RequestingEmployeeId).Single();

        if (helpRequestForm.HelperEmployee.CompanyEmail.Split('@')[0].ToLower() == currentUser.ToLower() || this.User.IsInRole(RoleList.Administrator))
        {
            var selectListEmployees = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
            {
                Value = c.EmployeeId.ToString(),
                Text = c.FullName
            });
            var selectListHelpers = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
            {
                Value = c.EmployeeId.ToString(),
                Text = c.FullName + "-" + c.Department.DepartmentName
            });

            ViewBag.CurrentUser = currentUser;
            ViewBag.Emp = helpRequestForm.RequestingEmployee.FullName;
            ViewData["HelperEmployeeId"] = new SelectList(selectListHelpers, "Value", "Text", helpRequestForm.HelperEmployeeId);
            ViewData["RequestingEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text", helpRequestForm.RequestingEmployeeId);
            return View(helpRequestForm);
        }
        else
        {
            return Redirect("~/Identity/Account/AccessDenied");
        }

    }
    // POST: HelpRequestForms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("HelpRequestFormId,Subject,RequestedByUser,RequestingEmployeeId,Description,Urgency,HelperEmployeeId,CreatedDate,Priority,IsDenied,IsComplete,CompletedDate")] HelpRequestForm helpRequestForm)
    {
        //normal saving will save the information and email the requesting employee with updates on the status of their ticket
        if (id != helpRequestForm.HelpRequestFormId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (helpRequestForm.IsDenied)
                {
                    helpRequestForm.Priority = null;
                }

                if (helpRequestForm.IsComplete)
                {
                    helpRequestForm.CompletedDate = DateTime.Now;
                }


                _context.Update(helpRequestForm);
                await _context.SaveChangesAsync();
                await Emailer(helpRequestForm, 2);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HelpRequestFormExists(helpRequestForm.HelpRequestFormId))
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
        var selectListEmployees = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName
        });
        var selectListHelpers = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + "-" + c.Department.DepartmentName
        });

        ViewData["HelperEmployeeId"] = new SelectList(selectListHelpers, "Value", "Text", helpRequestForm.HelperEmployeeId);
        ViewData["RequestingEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text", helpRequestForm.RequestingEmployeeId);
        return View(helpRequestForm);
    }


    // POST: HelpRequestForms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> SaveAndSend(int id, [Bind("HelpRequestFormId,Subject,RequestedByUser,RequestingEmployeeId,Description,Urgency,HelperEmployeeId,CreatedDate,Priority,IsDenied,IsComplete,CompletedDate")] HelpRequestForm helpRequestForm)
    {
        //save & send will save info and email new "helper" to let them know that they have a new ticket
        if (id != helpRequestForm.HelpRequestFormId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                helpRequestForm.Priority = null;
                helpRequestForm.IsComplete = false;
                helpRequestForm.IsDenied = false;


                _context.Update(helpRequestForm);
                await _context.SaveChangesAsync();
                await Emailer(helpRequestForm, 1);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HelpRequestFormExists(helpRequestForm.HelpRequestFormId))
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
        var selectListEmployees = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName
        });
        var selectListHelpers = _context.Employee.Where(i => i.JobStatus != Models.Company.JobStatus.Terminated).OrderBy(x => x.FullName).Select(c => new SelectListItem
        {
            Value = c.EmployeeId.ToString(),
            Text = c.FullName + "-" + c.Department.DepartmentName
        });

        ViewData["HelperEmployeeId"] = new SelectList(selectListHelpers, "Value", "Text", helpRequestForm.HelperEmployeeId);
        ViewData["RequestingEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text", helpRequestForm.RequestingEmployeeId);
        return View(helpRequestForm);
    }

    [Authorize(Roles = RoleList.Administrator)]
    // GET: HelpRequestForms/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var helpRequestForm = await _context.HelpRequestForm
            .Include(h => h.HelperEmployee)
            .Include(h => h.RequestingEmployee)
            .FirstOrDefaultAsync(m => m.HelpRequestFormId == id);
        if (helpRequestForm == null)
        {
            return NotFound();
        }

        return View(helpRequestForm);
    }

    [Authorize(Roles = RoleList.Administrator)]
    // POST: HelpRequestForms/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var helpRequestForm = await _context.HelpRequestForm.FindAsync(id);
        _context.HelpRequestForm.Remove(helpRequestForm);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool HelpRequestFormExists(int id)
    {
        return _context.HelpRequestForm.Any(e => e.HelpRequestFormId == id);
    }

    //takes in a help request form and identifier to decide what kind of email should be sent
    public async Task<IActionResult> Emailer(HelpRequestForm hrf, int identifier)
    {

        var employees = await _context.Employee.Where(i => i.EmployeeId == hrf.RequestingEmployeeId || i.EmployeeId == hrf.HelperEmployeeId).ToListAsync();
        hrf.HelperEmployee = employees.Where(i => i.EmployeeId == hrf.HelperEmployeeId).Single();
        hrf.RequestingEmployee = employees.Where(i => i.EmployeeId == hrf.RequestingEmployeeId).Single();
        var link = $"{_endpoints.Helpdesk}/HelpRequestForms/Edit/" + hrf.HelpRequestFormId;
        string email = "management@completeful.com";
        string subject = "Something went wrong";
        string message = $"Something went wrong with the help ticket emailer. Please check it out!";
        switch (identifier)
        {
            case 1:
                email = hrf.HelperEmployee.CompanyEmail;
                subject = "Help Request Ticket Received";
                message = $"A help request ticket has been submitted with your name on it! Please review and respond promptly<br>"
                    + link + $"<br>Ticket Information: <hr>"
                    + $"Requesting Employee: " + hrf.RequestingEmployee.FullName
                    + $"<br>Requesting User/Station: " + hrf.RequestedByUser
                    + $"<br>Subject: " + hrf.Subject
                    + $"<br>Description: " + hrf.Description
                    + $"<br>Urgency: " + hrf.Urgency;
                break;
            case 2:
                if (hrf.RequestingEmployee.CompanyEmail != null)
                {
                    email = hrf.RequestingEmployee.CompanyEmail;
                }
                else
                {
                    email = hrf.RequestedByUser + "@completeful.com";
                }

                subject = "Help request response";
                if (hrf.IsDenied)
                {
                    message = $"Your submitted help request has been denied. If this is unsatisfactory, please speak with your supervisor.<br>"
                        + $"<br>Ticket Information: <hr>"
                        + $"Requesting Employee: " + hrf.RequestingEmployee.FullName
                        + $"<br>Requesting User/Station: " + hrf.RequestedByUser
                        + $"<br>Subject: " + hrf.Subject
                        + $"<br>Description: " + hrf.Description
                        + $"<br>Urgency: " + hrf.Urgency; ;
                }
                if (hrf.Priority != null && !hrf.IsComplete)
                {
                    message = $"Your submitted help request has been viewed & given a priority of " + hrf.Priority + $" out of 6 <br>"
                        + $"<br>Ticket Information: <hr>"
                        + $"Requesting Employee: " + hrf.RequestingEmployee.FullName
                        + $"<br>Requesting User/Station: " + hrf.RequestedByUser
                        + $"<br>Subject: " + hrf.Subject
                        + $"<br>Description: " + hrf.Description
                        + $"<br>Urgency: " + hrf.Urgency; ;
                }
                if (hrf.IsComplete && !hrf.IsDenied)
                {
                    message = $"Your submitted help request has been marked complete.<br>"
                        + $"<br>Ticket Information: <hr>"
                        + $"Requesting Employee: " + hrf.RequestingEmployee.FullName
                        + $"<br>Requesting User/Station: " + hrf.RequestedByUser
                        + $"<br>Subject: " + hrf.Subject
                        + $"<br>Description: " + hrf.Description
                        + $"<br>Urgency: " + hrf.Urgency; ;
                }
                break;
            default:
                break;

        }

        await _graphAPIService.SendEmailAlert(subject, message, email, null);

        return RedirectToAction(nameof(Index));

    }
}