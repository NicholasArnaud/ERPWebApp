using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
[AutoValidateAntiforgeryToken]
public class MessageEmployeesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebhooks _webhooks;
    private static MessageEmployeeFilter _messageEmployeeFilter = new MessageEmployeeFilter();

    private readonly DateTime now =
        TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));


    public MessageEmployeesController(ApplicationDbContext context, IWebhooks webhooks, IConfiguration configuration)
    {
        _webhooks = webhooks;
        _context = context;
    }

    // GET: MessageEmployees
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Employee.Where(
            employee => employee.JobStatus != JobStatus.Terminated &&
                        employee.JobStatus != JobStatus.Seasonal &&
                        employee.PhoneNumber != "").Include(employee => employee.Department);

        _messageEmployeeFilter.Employees = await applicationDbContext.ToListAsync();
        _messageEmployeeFilter.MessageEmployeeList = await _context.MessageEmployee.ToListAsync();

        return View(_messageEmployeeFilter);
    }

    [HttpGet]
    public ActionResult MessageHistoryView()
    {
        _messageEmployeeFilter.MessageEmployeeList = _context.MessageEmployee.ToList();

        return PartialView("MessageHistoryView", _messageEmployeeFilter);
    }

    public async Task PopulateDatabase(String message)
    {
        var messageEmployee = new MessageEmployee();

        var eID = await _context.Employee.Where(employee =>
            employee.FirstName + employee.LastName.Substring(0, 1) == User.Identity.Name).FirstOrDefaultAsync();

        messageEmployee.EmployeeId = eID.EmployeeId;

        messageEmployee.Message = message;

        messageEmployee.SentTime = now;

        messageEmployee.SentFromEmployee = eID.FullName;

        _context.Add(messageEmployee);

        await _context.SaveChangesAsync();
    }



    [HttpGet]
    [ProducesResponseType(200)]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> SendMessage(string[] EmployeeIds, string[] DepartmentIds, string Message)
    {
        //If whats needed not here then stop. What are you doing? Get some help
        if (Message == null || (EmployeeIds[0].Length <= 2 && DepartmentIds[0].Length <= 2))
            return BadRequest();
        //people dictionary to message
        var people = new Dictionary<string, string>();

        //Departments first since it covers most people
        if (DepartmentIds[0].Length > 2)
        {
            string[] items = DepartmentIds[0].Replace("[", "").Replace("]", "").Replace("\\s", "").Split(",");
            int[] DepartmentIdIntArray = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                DepartmentIdIntArray[i] = int.Parse(items[i]);
            }

            if (DepartmentIdIntArray.Length > 0)
                foreach (var id in DepartmentIdIntArray)
                {
                    var peopleWithinDepartment = (from employee in _messageEmployeeFilter.Employees
                                                  where employee.DepartmentId == id
                                                  select employee);
                    if (peopleWithinDepartment.Count() > 0)
                        peopleWithinDepartment.ToList().ForEach(p => people.Add(p.PhoneNumber, p.FullName));
                }
        }

        if (EmployeeIds[0].Length > 2)
        {
            //Parse string because I couldn't get js to send over a basic int array
            string[] items = EmployeeIds[0].Replace("[", "").Replace("]", "").Replace("\\s", "").Split(",");
            int[] EmployeeIdIntArray = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                EmployeeIdIntArray[i] = int.Parse(items[i]);
            }

            //Individual Employees Selected Next
            //Ignore employees that fall in the same department selected. Lets not send them duplicate messages
            if (EmployeeIdIntArray.Length > 0)
                foreach (var id in EmployeeIdIntArray)
                {
                    var personIndividuallySelected = (from employee in _messageEmployeeFilter.Employees
                                                      where employee.EmployeeId == id
                                                      select employee);
                    if (!people.ContainsKey(personIndividuallySelected.First().PhoneNumber))
                        personIndividuallySelected.ToList().ForEach(p => people.Add(p.PhoneNumber, p.FullName));
                }
        }

        if (people.Count() > 0)
        {
            _webhooks.SendMessageToEmployee(Message, people, User.Identity.Name);

            //insert the message and other criteria into the DB
            await PopulateDatabase(Message);
        }

        return Ok(people);
    }
}