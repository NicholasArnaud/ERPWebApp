using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace ERPWebApp.Controllers;


[Authorize(Roles = RoleList.Administrator + "," + RoleList.Developer)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class SalesReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalesReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SalesReports
    public async Task<IActionResult> Index()
    {
        RemoveAllPreviousData();
        var applicationDbContext = _context.SalesReport;
        return View(await applicationDbContext.ToListAsync());
    }


    [HttpGet]
    [ProducesResponseType(200,
        Type = typeof(IEnumerable<SalesReport>))]
    public async Task<IEnumerable<SalesReport>> PullStoreSalesReports(int? StoreId, DateTime? StartDate, DateTime? EndDate)
    {
        RemoveAllPreviousData();
        var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();
        DbDataReader reader;

        try
        {
            using var command = conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetShopOrderInvoicesByShopIdAndDateRange";
            List<SqlParameter> param = new()
            {
                new SqlParameter("@StoreId", SqlDbType.Int){ Value = StoreId},
                new SqlParameter("@StartDate", SqlDbType.DateTime){ Value = StartDate},
                new SqlParameter("@EndDate", SqlDbType.DateTime){ Value = EndDate},
            };
            command.Parameters.AddRange(param.ToArray());

            reader = await command.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    var reportItem = new SalesReport
                    {
                        Sku = reader.GetString(0),
                        QuantitySold = reader.GetInt32(1),
                        CostPerItem = reader.GetDecimal(4),
                        TotalCost = reader.GetDecimal(5),
                        ShippingCost = reader.GetDecimal(3)
                    };
                    _context.SalesReport.Add(reportItem);
                }
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
        await _context.SaveChangesAsync();
        return await _context.SalesReport.ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> PartialViewInvoice()
    {
        return PartialView("PartialIndex", await _context.SalesReport.OrderBy(x => x.Sku).ToListAsync());
    }


    [HttpGet]
    [ProducesResponseType(200,
        Type = typeof(IEnumerable<ShipStationStore>))]
    public async Task<IEnumerable<ShipStationStore>> GetStores()
    {
        return await _context.ShipStationStore.Where(x => x.IsActive).ToListAsync();
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
