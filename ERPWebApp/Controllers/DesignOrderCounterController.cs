using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ProductionBasic)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class DesignOrderCounterController : Controller
{
    private readonly ApplicationDbContext _context;
    private static List<Employee> _employeeList;
    private static List<DesignOrderCounterViewModel> _designOrderDesignList;

    public DesignOrderCounterController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DesignCounter
    public async Task<ActionResult> Index()
    {
        _employeeList = await _context.Employee.ToListAsync();
        _designOrderDesignList = new List<DesignOrderCounterViewModel>();
        return View(_designOrderDesignList);
    }


    [HttpGet]
    [ProducesResponseType(200,
        Type = typeof(List<DesignOrderCounterViewModel>))]
    public async Task<List<DesignOrderCounterViewModel>> PullDesignOrdersByDateRange(int? EmployeeId, DateTime StartDate, DateTime EndDate)
    {

        Employee selectedEmployee = new();
        List<Employee> EmployeeList = await _context.Employee.ToListAsync();
        if (EmployeeId != null)
        {
            selectedEmployee = _employeeList.Single(e => e.EmployeeId == EmployeeId);
        }
        else
        {

        }
        var conn = _context.Database.GetDbConnection();
        conn.Open();
        try
        {
            using var command = conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetDesignerOrdersByDesignDateRange";
            List<SqlParameter> param = new()
                {
                    new SqlParameter("@DesignStartDate", SqlDbType.DateTime){ Value = StartDate},
                    new SqlParameter("@DesignEndDate", SqlDbType.DateTime){ Value = EndDate}
                };
            command.Parameters.AddRange(param.ToArray());
            DbDataReader reader = await command.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                _designOrderDesignList = new List<DesignOrderCounterViewModel>();
                while (await reader.ReadAsync())
                {
                    try
                    {
                        //CustomField3 Designer Information
                        string[] cfSplit = reader.GetString(6).Split(',');
                        if (selectedEmployee != null && selectedEmployee.FirstName != null && EmployeeId != null)
                        {
                            if (selectedEmployee.FirstName.First() == cfSplit[1].First() &&
                            selectedEmployee.LastName.First() == cfSplit[1].Last())
                            {
                                //Checks for middle name and verifies correct employee
                                if (cfSplit[1].Length == 3 && selectedEmployee.MiddleName.First() != cfSplit[1][1])
                                    continue;
                                DesignOrderCounterViewModel currentDesignerOrderInfo = new()
                                {
                                    OrderNumber = reader.GetString(0),
                                    Sku = reader.GetString(1),
                                    ItemName = reader.GetString(2),
                                    Quantity = reader.GetInt32(3)
                                };
                                if (!reader.IsDBNull(4))
                                    currentDesignerOrderInfo.ShipDate = reader.GetDateTime(4);
                                currentDesignerOrderInfo.OrderDate = reader.GetDateTime(5);
                                currentDesignerOrderInfo.CustomField3 = reader.GetString(6);

                                currentDesignerOrderInfo.DesignDate = Convert.ToDateTime(cfSplit[0]);
                                currentDesignerOrderInfo.DesignerName = selectedEmployee.FullName;
                                _designOrderDesignList.Add(currentDesignerOrderInfo);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            selectedEmployee = EmployeeList.Where
                                (x => x.JobStatus != JobStatus.Terminated &&
                                x.FirstName.StartsWith((char)cfSplit[1].First()) &&
                                x.LastName.StartsWith((char)cfSplit[1].Last()) && !string.IsNullOrEmpty(x.MiddleName) &&
                                x.MiddleName.StartsWith((char)cfSplit[1][1])).FirstOrDefault();

                            DesignOrderCounterViewModel currentDesignerOrderInfo = new()
                            {
                                OrderNumber = reader.GetString(0),
                                Sku = reader.GetString(1),
                                ItemName = reader.GetString(2),
                                Quantity = reader.GetInt32(3)
                            };
                            if (!reader.IsDBNull(4))
                                currentDesignerOrderInfo.ShipDate = reader.GetDateTime(4);
                            currentDesignerOrderInfo.OrderDate = reader.GetDateTime(5);
                            currentDesignerOrderInfo.CustomField3 = reader.GetString(6);

                            currentDesignerOrderInfo.DesignDate = Convert.ToDateTime(cfSplit[0]);
                            currentDesignerOrderInfo.DesignerName = (selectedEmployee == null) ? cfSplit[1] : selectedEmployee.FullName;
                            _designOrderDesignList.Add(currentDesignerOrderInfo);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An issue has arisen:" + ex.Message);
                    }

                }
            }
            reader.Close();
        }
        finally
        {
            conn.Close();
        }
        int DesignCount = 0;
        _designOrderDesignList.ForEach(Design => DesignCount += Design.Quantity);
        ViewBag.DesignCount = DesignCount;
        return _designOrderDesignList;
    }

    [HttpGet]
    public IActionResult PartialViewIndex()
    {
        return PartialView("PartialIndex", _designOrderDesignList.OrderBy(x => x.Sku));
    }


    public async Task<IEnumerable<Employee>> GetEmployees()
    {
        return await _context.Employee.Where(x => x.JobStatus != JobStatus.Terminated)
            .OrderBy(emp => emp.FullName).ToListAsync();
    }


    private void RemoveAllPreviousData()
    {
        foreach (var id in _context.SalesReport.Select(e => e.SalesReportId))
        {
            var entity = new SalesReport { SalesReportId = id };
            _context.SalesReport.Attach(entity);
            _context.SalesReport.Remove(entity);
        }
        _context.SaveChanges();
    }
}
