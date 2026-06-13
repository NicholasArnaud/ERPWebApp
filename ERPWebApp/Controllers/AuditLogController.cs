using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class AuditLogController : Controller
{
    private readonly IAuditLogService _auditLogService;
    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public IActionResult Index()
    {


        return View();
    }

    [HttpPost("GetAuditLogs")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetAuditLogs()
    {
        int recordsTotal = 0;
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault().ToLower();
        int pageSize = 0;
        recordsTotal = await _auditLogService.GetCountAsync(null);
        if (length != null)
        {
            if (length == "-1")
            {
                pageSize = recordsTotal;
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        IQueryable<AuditLogDTO> query(IQueryable<AuditLog> v)
        {
            return v.Select(x => new AuditLogDTO
            {
                Id = x.Id,
                User = x.UserName,
                Timestamp = x.Timestamp,
                BusinessEntity = x.BusinessEntity,
                PropertyName = x.PropertyName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
            }).OrderByDescending(x=>x.Timestamp).Skip(skip).Take(pageSize);
        }

        var result = await _auditLogService.GetListAsync(query);
        var jsonData = new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data = result
        };


        return Ok(jsonData);
    }
}
