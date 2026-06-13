using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Data.Extensions;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Extensions;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories;

public class MoveStockHistoryRepository(ApplicationDbContext context, ILogger<MoveStockHistoryRepository> logger)
    : Repository<MoveStockHistory>(context), IMoveStockHistoryRepository
{
    public async Task<IEnumerable<MoveStockHistory>> GetStockHistoriesCustomSelectionAsync(int id)
    {
        return await _context.MoveStockHistory
            .Where(x => x.FromStockId == id || x.ToStockId == id)
            .ToListAsync();
    }

    public async Task<(IEnumerable<StockMovementHistory>, int)> GetStockMovementHistoryAsync(
        SearchParameters search,
        bool? isExternal,
        string sku
    )
    {
        try
        {
            var stockTransaction = context.MoveStockHistory
                .WhereIf(!string.IsNullOrWhiteSpace(sku), x => x.Sku.Equals(sku))
                .Select(a => new
                {
                    a.Sku,
                    a.Type,
                    a.DateTime,
                    a.EmployeeName,
                    a.FromStockId,
                    FromStockQty = a.Quantity,
                    FromStockRunnigBalance = 0,
                    a.ToStockId,
                   ToStockQty = a.Quantity,
                    ToStockRunnigBalance = 0
                });

            var fromStockHistory = from a in stockTransaction
                join b in context.Stock.TemporalAll()
                    on a.FromStockId equals b.StockId into fromStock
                from b in fromStock.DefaultIfEmpty()
                join c in context.Location on b.LocationId equals c.LocationId into fromLocation
                from c in fromLocation.DefaultIfEmpty()
                where (a.DateTime >= EF.Property<DateTime>(b, "PeriodStart") &&a.DateTime <= EF.Property<DateTime>(b, "PeriodEnd"))
                    && (isExternal == null || b.IsExternal == isExternal)
                select new
                {
                    a.Sku,
                    a.Type,
                    a.DateTime,
                    a.EmployeeName,
                    FromLocation = c.LocationName,
                    a.FromStockQty,
                    FromStockRunnigBalance = b != null ? b.TotalAvailable : 0,
                    a.ToStockId,
                    a.ToStockQty,
                    ToStockRunnigBalance = 0
                };
            
            

            var finalQuery = from a in fromStockHistory
                join b in context.Stock.TemporalAll()
                    on a.ToStockId equals b.StockId into toStock
                from b in toStock.DefaultIfEmpty()
                join c in context.Location on b.LocationId equals c.LocationId into tolocation
                from c in tolocation.DefaultIfEmpty()
                where a.DateTime >= EF.Property<DateTime>(b, "PeriodStart") &&
                      a.DateTime <= EF.Property<DateTime>(b, "PeriodEnd")
                      && (
                          string.IsNullOrEmpty(search.SearchValue) ||
                          (
                              c.LocationName.Contains(search.SearchValue)
                              || a.EmployeeName.Contains(search.SearchValue)
                              || a.Sku.Contains(search.SearchValue)
                              || a.FromLocation.Contains(search.SearchValue)
                          )
                      )
                      && (isExternal == null || b.IsExternal == isExternal)
                select new
                {
                    a.Sku,
                    a.Type,
                    a.DateTime,
                    a.EmployeeName,
                    a.FromLocation,
                    a.FromStockQty,
                    a.FromStockRunnigBalance,
                    ToLocation = c.LocationName,
                    a.ToStockQty,
                    ToStockRunnigBalance = b != null ? b.TotalAvailable : 0
                };
        
            var count = await finalQuery.CountAsync();
            
            search.PageSize = search.PageSize is null or < 0 ? count : search.PageSize;
            
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                if (search.IsDescending)
                {
                    finalQuery = search.SortBy switch
                    {
                        "SKU" => finalQuery.OrderByDescending(x => x.Sku).ThenByDescending(x => x.DateTime),
                        "FromLocation" => finalQuery.OrderByDescending(x => x.FromLocation),
                        "ToLocation" => finalQuery.OrderByDescending(x => x.ToLocation),
                        "Action" or "MovementType" => finalQuery.OrderByDescending(x => x.Type),
                        "DateTime" => finalQuery.OrderByDescending(x => x.DateTime),
                        "EmployeeName" => finalQuery.OrderByDescending(x => x.EmployeeName)
                            .ThenByDescending(x => x.DateTime),
                        "Quantity" => finalQuery.OrderByDescending(x => x.ToStockQty),
                        _ => finalQuery.OrderByDescending(x => x.DateTime).ThenByDescending(x=>x.Sku)
                    };
                }
                else
                {
                    finalQuery = search.SortBy switch
                    {
                        "SKU" => finalQuery.OrderBy(x => x.Sku).ThenByDescending(x => x.DateTime),
                        "FromLocation" => finalQuery.OrderBy(x => x.FromLocation),
                        "ToLocation" => finalQuery.OrderBy(x => x.ToLocation),
                        "Action" or "MovementType" => finalQuery.OrderBy(x => x.Type),
                        "DateTime" => finalQuery.OrderBy(x => x.DateTime),
                        "EmployeeName" => finalQuery.OrderBy(x => x.EmployeeName).ThenByDescending(x => x.DateTime),
                        "Quantity" => finalQuery.OrderBy(x => x.ToStockQty),
                        _ => finalQuery.OrderBy(x => x.DateTime).ThenBy(x=>x.Sku)
                    };
                }
            }


            var result = await finalQuery
                .AsNoTracking()
                .SmartPaging(search.Start, search.PageSize)
                .ToListAsync();

            var stockHistory = result.Select(x => new StockMovementHistory(
                x.EmployeeName,
                x.DateTime.ToLocalTimeIfUtc(search.UserTimeZone),
                x.Sku,
                x.Type.GetDisplayName(),
                x.FromLocation,
                x.FromStockRunnigBalance,
                x.ToLocation,
                x.ToStockRunnigBalance,
                ActionQty(x.Type, x.FromStockQty, x.ToStockQty),
                StockAction(x.Type,(x.FromLocation, x.FromStockQty), (x.ToLocation, x.ToStockQty))
            )).ToList();

            return (stockHistory, count);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to retrieve the MoveStockHistory");
            throw;
        }
        
        int ActionQty(ActionType action, int fromStockQty, int toStockQty) => action switch
        {
            ActionType.Add => toStockQty,
            ActionType.Remove => fromStockQty,
            ActionType.Transfer => fromStockQty,
            ActionType.Received => toStockQty,
            _ => fromStockQty
        };

        string StockAction(ActionType action,(string location, int qty) from, (string location, int qty) to) => action switch
        {
            ActionType.Add => $"Qty of {to.qty} Added to location {to.location}",
            ActionType.Remove => $"Qty of {from.qty} Removed from {from.location}",
            ActionType.Transfer => $"Qty of {from.qty} Transferred from {from.location} to {to.location}",
            ActionType.Received => $"Qty of {to.qty} Received to Location {to.location}",
            ActionType.CycleCount when from.qty != 0 => $"Qty of {from.qty} Adjusted on Location {from.location}",
            ActionType.CycleCount when from.qty == 0 => $"Cycle Count performed without any modification on Location {from.location}",
            _ => "Unknown Action"
        };
    }
}