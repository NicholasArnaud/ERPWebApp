using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories
{
    public class ShipStationOrderedHistoryRepository : Repository<ShipStationOrderedHistory>, IShipStationOrderedHistoryRepository
    {
        public ShipStationOrderedHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// First delete the old ShipStationOrderedHistory, then insert the new data.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public async Task UpdateShipStationOrderedHistory()
        {
            try
            {
                _context.Database.SetCommandTimeout(200);
                var allRecords = await _context.ShipStationOrderedHistory.ToListAsync();
                _context.ShipStationOrderedHistory.RemoveRange(allRecords);
                await _context.SaveChangesAsync();
                var currentTime = DateTime.Now;

                var ninetyDaysAgo = currentTime.AddDays(-90);

                var query = (
                    from x in _context.Orders
                    join io in _context.OrderItem on x.ERPOrderId equals io.ERPOrderId
                    join a in _context.Product on io.sku.Substring(0, io.sku.Length) equals a.Sku
                    let b = (
                        from stock in _context.Stock
                        join location in _context.Location on stock.LocationId equals location.LocationId
                        join site in _context.Site on location.SiteId equals site.SiteId
                        where stock.ProductId == a.ProductId
                              && location.IsActive
                              && site.IsActive
                              && !new[] { "Production", "QC Holding", "Operator Error", "Red Bucket" }.Contains(site.SiteName)
                        select stock.TotalAvailable
                    ).DefaultIfEmpty().Sum()
                    where x.orderStatus != OrderStatus.cancelled
                          && x.orderDate >= ninetyDaysAgo
                    select new
                    {
                        x.orderDate,
                        io.quantity,
                        a.Sku,
                        a.Description,
                        a.OnOrder,
                        a.LeadTime,
                        b
                    }
                ).AsEnumerable();

                var result = query
                    .GroupBy(g => new { g.Sku, g.Description, g.OnOrder, g.LeadTime, g.b })
                    .OrderBy(g => g.Key.Sku)
                    .Select(g => new
                    {
                        g.Key.Sku,
                        g.Key.Description,
                        TotalAvailable = g.Key.b,
                        g.Key.OnOrder,
                        g.Key.LeadTime,
                        OrderedIn24Hours = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days == 1 ? x.quantity : 0),
                        OrderedIn3Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 3 ? x.quantity : 0),
                        OrderedIn7Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 7 ? x.quantity : 0),
                        OrderedIn15Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 15 ? x.quantity : 0),
                        OrderedIn30Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 30 ? x.quantity : 0),
                        OrderedIn90Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 90 ? x.quantity : 0)
                    })
                    .ToList();



                var orderedHistoryList = new List<ShipStationOrderedHistory>();

                foreach (var item in result)
                {
                    var orderedHistoryItem = new ShipStationOrderedHistory
                    {
                        Sku = item.Sku,
                        Description = item.Description,
                        TotalFromAllLocations = item.TotalAvailable,
                        OnOrder = item.OnOrder,
                        LeadTime = item.LeadTime,
                        OrderedIn24Hours = item.OrderedIn24Hours,
                        OrderedIn3Days = item.OrderedIn3Days,
                        OrderedIn7Days = item.OrderedIn7Days,
                        OrderedIn15Days = item.OrderedIn15Days,
                        OrderedIn30Days = item.OrderedIn30Days,
                        OrderedIn90Days = item.OrderedIn90Days
                    };

                    orderedHistoryList.Add(orderedHistoryItem);
                }

                _context.ShipStationOrderedHistory.AddRange(orderedHistoryList);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }

        }
        public List<ShipStationOrderedHistory> GetShipStationOrderedHistory(int DepartmentId)
        {
            var currentTime = DateTime.Now;
            var qOrders = from order in _context.Orders
                          join orderItem in _context.OrderItem on order.ERPOrderId equals orderItem.ERPOrderId
                          join product in _context.Product on orderItem.ERPProductId equals product.ProductId into ProductJoin
                          from joinOnProduct in ProductJoin.DefaultIfEmpty()
                          join bundle in _context.Bundle on orderItem.ERPBundleId equals bundle.BundleId into BundleJoin
                          from joinOnBundle in BundleJoin.DefaultIfEmpty()
                          join bundleItem in _context.BundleItem on joinOnBundle.BundleId equals bundleItem.BundleId into BundleItemJoin
                          from joinOnBundleItem in BundleItemJoin.DefaultIfEmpty()
                          join productFromBundle in _context.Product on joinOnBundleItem.ProductId equals productFromBundle.ProductId into ProductFromBundleJoin
                          from productFromBundle in ProductFromBundleJoin.DefaultIfEmpty()
                          where order.orderStatus != OrderStatus.cancelled
                          where order.orderDate >= currentTime.AddDays(-90)
                          select new
                          {
                              order.orderDate,
                              orderItem.quantity,
                              ProductId = productFromBundle != null ? productFromBundle.ProductId : joinOnProduct != null ? joinOnProduct.ProductId : 0,
                              Sku = productFromBundle.Sku ?? joinOnProduct.Sku ?? string.Empty,
                              LeadTime = productFromBundle != null ? productFromBundle.LeadTime : joinOnProduct != null ? joinOnProduct.LeadTime : 0,
                              OnOrder = productFromBundle != null ? productFromBundle.OnOrder : joinOnProduct != null ? joinOnProduct.OnOrder : 0,
                              Description = productFromBundle.Description ?? joinOnProduct.Description ?? string.Empty,
                              bundleQty = joinOnBundleItem != null ? joinOnBundleItem.Quantity : 1,
                              DepartmentIds = (
                              from productDepartment in _context.Department
                              where productDepartment.Products.Select(z => z.ProductId).Contains(productFromBundle != null ? productFromBundle.ProductId : joinOnProduct != null ? joinOnProduct.ProductId : 0)
                              select productDepartment.DepartmentId).ToList()
                          };
            var qStock = qOrders
                .Where(x => x.Sku != string.Empty && (x.DepartmentIds.Contains(DepartmentId) || DepartmentId == 0))
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Sku,
                    x.OnOrder,
                    x.LeadTime,
                    x.Description,
                    x.orderDate,
                    x.bundleQty
                })
                .Select(g => new
                {
                    g.Key.Sku,
                    g.Key.OnOrder,
                    g.Key.LeadTime,
                    g.Key.Description,
                    g.Key.orderDate,
                    TotalQuantity = g.Sum(x => x.quantity * g.Key.bundleQty),
                    TotalAvailable = (
                        from stock in _context.Stock
                        join location in _context.Location on stock.LocationId equals location.LocationId
                        join site in _context.Site on location.SiteId equals site.SiteId
                        where stock.ProductId == g.Key.ProductId
                              && location.IsActive
                              && site.IsActive
                              && location.Type != LocationType.ReceiveOnly
                              && !location.IsExternal
                        select stock.TotalAvailable
                    ).Sum()
                }).ToList();

            var result = qStock
                .GroupBy(g => new
                {
                    g.Sku
                })
                .OrderBy(g => g.Key.Sku)
                .Select(g => new
                {
                    g.Key.Sku,
                    OnOrder = g.Max(x => x.OnOrder),
                    TotalAvailable = g.Max(x => x.TotalAvailable),
                    g.First().Description,
                    LeadTime = g.Max(x => x.LeadTime),
                    OrderedIn24Hours = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days == 1 ? x.TotalQuantity : 0),
                    OrderedIn3Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 3 ? x.TotalQuantity : 0),
                    OrderedIn7Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 7 ? x.TotalQuantity : 0),
                    OrderedIn15Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 15 ? x.TotalQuantity : 0),
                    OrderedIn30Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 30 ? x.TotalQuantity : 0),
                    OrderedIn90Days = g.Sum(x => (currentTime.Date - x.orderDate.Date).Days <= 90 ? x.TotalQuantity : 0),
                }).ToList();

            var orderedHistoryList = new List<ShipStationOrderedHistory>();

            foreach (var item in result)
            {
                var orderedHistoryItem = new ShipStationOrderedHistory
                {
                    Sku = item.Sku,
                    Description = item.Description,
                    TotalFromAllLocations = item.TotalAvailable,
                    OnOrder = item.OnOrder,
                    LeadTime = item.LeadTime,
                    OrderedIn24Hours = item.OrderedIn24Hours,
                    OrderedIn3Days = item.OrderedIn3Days,
                    OrderedIn7Days = item.OrderedIn7Days,
                    OrderedIn15Days = item.OrderedIn15Days,
                    OrderedIn30Days = item.OrderedIn30Days,
                    OrderedIn90Days = item.OrderedIn90Days,
                };

                orderedHistoryList.Add(orderedHistoryItem);
            }
            return orderedHistoryList;
        }
    }
}
