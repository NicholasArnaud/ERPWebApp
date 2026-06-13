using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ERPWebApp.Controllers.Inventory;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.FinancialBasic + "," + RoleList.SellerBasic)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class StockHistoryController : Controller
{

    private readonly ApplicationDbContext _context;
    public static StockHistory _stockHistory = new();
    public static StockHistory _stockHistoryFilter = new();
    public StockHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        StockHistory _stockHistory = new();
        ViewData["SkuData"] = new SelectList(_context.Stock.Include(x => x.Products).ToList().GroupBy(z => z.Products).Select(x => x.First()), "ProductId", "Products.Sku");
        ViewData["LocationData"] = new SelectList(_context.Stock.Include(x => x.Location).ToList().GroupBy(z => z.Location).Select(x => x.First()), "LocationId", "Location.LocationName");
        return View();
    }
    [HttpPost("GetStockHistory")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetStockHistory(int Sku, int Location, string DateRange)
    {
        StockHistory _stockHistory = new();
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = 0;
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }
        //initial loading of the table
        var stockHistoryDb = _context.Stock.TemporalAll().ToList();
        var stockHistoryJoin = (from s in stockHistoryDb
                                join p in _context.Product on s.ProductId equals p.ProductId
                                join l in _context.Location on s.LocationId equals l.LocationId
                                select new
                                {
                                    s.ModifyDate,
                                    s.ModifyByUser,
                                    s.StockId,
                                    p.ProductId,
                                    p.Sku,
                                    p.Description,
                                    l.LocationId,
                                    Location = l.LocationName,
                                    s.TotalAvailable
                                }).Distinct();
        //loading of the filtered table
        var modelFiltered = stockHistoryJoin;
        //dropdown menu filters
        if (DateRange != null)
        {
            string[] dates = DateRange.Split(" - ", 2);
            DateTime first = DateTime.Parse(dates[0]);
            DateTime second = DateTime.Parse(dates[1]);

            //xyy
            if (Sku == -10 && Location != -10)
            {
                modelFiltered = modelFiltered
                .Where(x => x.ModifyDate >= first && x.ModifyDate <= second && x.LocationId == Location).OrderByDescending(m => m.ModifyDate).ToList();
            }
            //yxy
            else if (Sku != -10 && Location == -10)
            {
                modelFiltered = modelFiltered
                .Where(x => x.ProductId == Sku && x.ModifyDate >= first && x.ModifyDate <= second).OrderByDescending(m => m.ModifyDate).ToList();
            }
            //yyy
            else if (Sku != -10 && Location != -10)
            {
                modelFiltered = modelFiltered
                .Where(x => x.ProductId == Sku && x.LocationId == Location && x.ModifyDate >= first && x.ModifyDate <= second).OrderByDescending(m => m.ModifyDate).ToList();
            }
            //xxy
            else if (Sku == -10 && Location == -10 && DateRange != null)
            {
                modelFiltered = modelFiltered
                    .Where(x => x.ModifyDate >= first && x.ModifyDate <= second).OrderByDescending(m => m.ModifyDate).ToList();
            }
        }
        //xyx
        else if (Sku == -10 && Location != -10 && DateRange == null)
        {
            modelFiltered = modelFiltered
                .Where(x => x.LocationId == Location).OrderByDescending(m => m.ModifyDate).ToList();
        }
        //yxx
        else if (Sku != -10 && Location == -10 && DateRange == null)
        {
            modelFiltered = modelFiltered
                .Where(x => x.ProductId == Sku).OrderByDescending(m => m.ModifyDate).ToList();
        }
        //yyx
        else if (Sku != -10 && Location != -10 && DateRange == null)
        {
            modelFiltered = modelFiltered
                .Where(x => x.ProductId == Sku && x.LocationId == Location).OrderByDescending(m => m.ModifyDate).ToList();
        }


        if (length != null)
        {
            if (length == "-10")
            {
                pageSize = stockHistoryJoin.Count();
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }
        //Column Sort Direction
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Modify Date":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.ModifyDate);
                        break;
                    case "Modified By User":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.ModifyByUser);
                        break;
                    case "Product Sku":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.Sku);
                        break;
                    case "Product Name":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.Description);
                        break;
                    case "Location":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.Location);
                        break;
                    case "Total Available":
                        stockHistoryJoin = stockHistoryJoin.OrderBy(x => x.TotalAvailable);
                        break;
                }
            }
            else if (sortColumnDirection == "desc")
            {
                switch (sortColumn)
                {
                    case "Modify Date":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.ModifyDate);
                        break;
                    case "Modified By User":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.ModifyByUser).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Product Sku":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.Sku).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Product Name":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.Description).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Location":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.Location).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Total Available":
                        stockHistoryJoin = stockHistoryJoin.OrderByDescending(x => x.TotalAvailable).ThenByDescending(x => x.ModifyDate);
                        break;
                }
            }
        }
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Modify Date":
                        modelFiltered = modelFiltered.OrderBy(x => x.ModifyDate);
                        break;
                    case "Modified By User":
                        modelFiltered = modelFiltered.OrderBy(x => x.ModifyByUser);
                        break;
                    case "Product Sku":
                        modelFiltered = modelFiltered.OrderBy(x => x.Sku);
                        break;
                    case "Product Name":
                        modelFiltered = modelFiltered.OrderBy(x => x.Description);
                        break;
                    case "Location":
                        modelFiltered = modelFiltered.OrderBy(x => x.Location);
                        break;
                    case "Total Available":
                        modelFiltered = modelFiltered.OrderBy(x => x.TotalAvailable);
                        break;
                }
            }
            else if (sortColumnDirection == "desc")
            {
                switch (sortColumn)
                {
                    case "Modify Date":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.ModifyDate);
                        break;
                    case "Modified By User":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.ModifyByUser).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Product Sku":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.Sku).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Product Name":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.Description).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Location":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.Location).ThenByDescending(x => x.ModifyDate);
                        break;
                    case "Total Available":
                        modelFiltered = modelFiltered.OrderByDescending(x => x.TotalAvailable).ThenByDescending(x => x.ModifyDate);
                        break;
                }
            }
        }
        var columndir = 0;
        if (columndir == 1)
        {
            _stockHistory.Stock =
                (from fullStock in stockHistoryJoin.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).OrderBy(x => x.ModifyDate).ThenBy(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else if (columndir == 2)
        {
            _stockHistory.Stock =
                (from fullStock in stockHistoryJoin.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).OrderByDescending(x => x.ModifyDate).ThenByDescending(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            _stockHistory.Stock =
                (from fullStock in stockHistoryJoin.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).ToList();
        }
        if (columndir == 1)
        {
            _stockHistory.Stock =
                (from fullStock in modelFiltered.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).OrderBy(x => x.ModifyDate).ThenBy(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else if (columndir == 2)
        {
            _stockHistory.Stock =
                (from fullStock in modelFiltered.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).OrderByDescending(x => x.ModifyDate).ThenByDescending(x => x.Sku).Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            _stockHistory.Stock =
                (from fullStock in modelFiltered.ToList()
                 select new StockHistory
                 {
                     ModifyDate = fullStock.ModifyDate,
                     ModifyByUser = fullStock.ModifyByUser,
                     Sku = fullStock.Sku,
                     Description = fullStock.Description,
                     Location = fullStock.Location,
                     TotalAvailable = fullStock.TotalAvailable
                 }).ToList();
        }
        //Filters through the search string
        if (!string.IsNullOrEmpty(searchValue))
        {
            stockHistoryJoin = stockHistoryJoin.Where(d => d.Sku.ToLower().Contains(searchValue)
                               || d.ModifyDate.ToString().ToLower().Contains(searchValue)
                               || d.ModifyByUser != null && d.ModifyByUser.ToLower().Contains(searchValue)
                               || d.Description.ToLower().Contains(searchValue)
                               || d.Location.ToLower().Contains(searchValue)
                               || d.TotalAvailable.Equals(searchValue));
        }
        if (!string.IsNullOrEmpty(searchValue))
        {
            modelFiltered = modelFiltered.Where(d => d.Sku.ToLower().Contains(searchValue)
                               || d.ModifyDate.ToString().ToLower().Contains(searchValue)
                               || d.ModifyByUser != null && d.ModifyByUser.ToLower().Contains(searchValue)
                               || d.Description.ToLower().Contains(searchValue)
                               || d.Location.ToLower().Contains(searchValue)
                               || d.TotalAvailable.Equals(searchValue));
        }
        int recordsTotal = 0;
        int recordsFilteredTotal = 0;
        recordsTotal = stockHistoryJoin.Count();
        recordsFilteredTotal = modelFiltered.Count();
        //initial return
        if (Sku == -10 && Location == -10 && DateRange == null)
        {
            stockHistoryJoin = stockHistoryJoin.Skip(skip).Take(pageSize);
            var data = stockHistoryJoin.ToList();
            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };

            return Ok(jsonData);
        }
        //filtered return
        else
        {
            modelFiltered = modelFiltered.Skip(skip).Take(pageSize);
            var filteredData = modelFiltered.ToList();
            var jsonFilteredData = new { draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsFilteredTotal, data = filteredData };

            return Ok(jsonFilteredData);
        }
    }
}
