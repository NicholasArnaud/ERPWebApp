using System.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Data.DTOModels;
using Microsoft.EntityFrameworkCore;
using static ERPWebApp.Models.Orders.Order;
using ERPWebApp.Models.Reports;
using DocumentFormat.OpenXml.Bibliography;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class FinancialsRepository : Repository<FinancialsViewModel>, IFinancialsRepository
    {
        private DateTime _currentDateTime;
        public FinancialsRepository(ApplicationDbContext context) : base(context)
        {
            _currentDateTime = DateTime.Now;
        }
        public async Task<List<FulfillmentInfoDto>> FulfillmentTable()
        {
            var joinedData = await (from orderItem in _context.OrderItem
                                    join order in _context.Orders on orderItem.ERPOrderId equals order.ERPOrderId
                                    from store in _context.ShipStationStore
                                    let product = _context.Product.FirstOrDefault(p => p.ProductId == orderItem.ERPProductId)
                                    join productCustomFulfillment in _context.ProductCustomFulFillment on product.ProductId equals productCustomFulfillment.ProductId into productCustomFulfillment
                                    from department in product.Departments.Where(d => d.IsProduction && d.IsActive).DefaultIfEmpty()
                                    let departmentName = product == null || !product.Departments.Any() ? "Unknown" : department.DepartmentName
                                    let departmentColor = product == null || !product.Departments.Any() ? "Unknown" : department.DepartmentColor
                                    where orderItem.sku != null && orderItem.Order.orderDate.Date >= DateTime.Today && orderItem.Order.advancedOptions.storeId == store.StoreId
                                          && (departmentName != "Unknown" || !string.IsNullOrEmpty(orderItem.sku))
                                    select new FulfillmentInfoDto
                                    {
                                        OrderDate = orderItem.Order.orderDate,
                                        HasIncreasedPricing = store.HasIncreasedPricing,
                                        StoreName = store.StoreName,
                                        ProductSku = product.Sku,
                                        ProductCost = product != null ? product.Cost : 0,
                                        ProductFulfillmentCost =
                                                    productCustomFulfillment.SingleOrDefault() == default ? product.FulfillmentCost :
                                                    productCustomFulfillment.Single().CustomFulfillmentCost,
                                        DepartmentName = departmentName,
                                        DepartmentColor = departmentColor
                                    }).ToListAsync();

            foreach (var row in joinedData)
            {
                if (row.HasIncreasedPricing)
                {
                    row.ProductFulfillmentCost *= 1.15m;
                }
                row.ProductProfit = (row.ProductFulfillmentCost ?? 0) - row.ProductCost;
            }

            var groupedData = joinedData.GroupBy(row => new { row.DepartmentName, row.DepartmentColor })
                                 .Select(group => new FulfillmentInfoDto
                                 {
                                     DepartmentName = group.Key.DepartmentName,
                                     DepartmentColor = group.Key.DepartmentColor,
                                     ProductFulfillmentCost = group.Sum(row => row.ProductFulfillmentCost),
                                     ProductCost = group.Sum(row => row.ProductCost),
                                     ProductProfit = group.Sum(row => row.ProductProfit),
                                     StoreFulfillmentCost = group.GroupBy(row => row.StoreName)
                                                                 .ToDictionary(g => g.Key, g => g.Sum(row => row.ProductFulfillmentCost ?? 0))

                                 }).ToList();
            return groupedData;
        }

        public async Task<List<TrendsInfoDto>> TrendsTable(DateTime startDate, DateTime endDate)
        {
            var joinedData = await _context.OrderItem
                .Where(orderItem =>
                    orderItem.sku != null &&
                    orderItem.Order.orderDate.Date >= startDate &&
                    orderItem.Order.orderDate.Date <= endDate)
                .Join(
                    _context.ShipStationStore,
                    orderItem => orderItem.Order.advancedOptions.storeId,
                    store => store.StoreId,
                    (orderItem, store) => new { orderItem, store }
                )
                .SelectMany(
                    x => _context.Product
                        .Where(p => p.ProductId == x.orderItem.ERPProductId)
                        .DefaultIfEmpty(),
                    (x, product) => new { x.orderItem, x.store, product }
                )
                .GroupJoin(
                    _context.ProductCustomFulFillment,
                    x => x.product.ProductId,
                    pcf => pcf.ProductId,
                    (x, productCustomFulfillment) => new { x.orderItem, x.store, x.product, productCustomFulfillment = productCustomFulfillment.FirstOrDefault() }
                )
                .SelectMany(
                    x => x.product.Departments
                        .Where(d => d.IsProduction && d.IsActive)
                        .DefaultIfEmpty(),
                    (x, department) => new
                    {
                        x.orderItem,
                        x.store,
                        x.product,
                        x.productCustomFulfillment,
                        department,
                        departmentName = x.product == null ? "Unknown" :
                            !x.product.Departments.Any() ? "Unknown" :
                            (x.product.Departments.Count == 1 ? x.product.Departments.First().DepartmentName :
                            (x.orderItem.sku.EndsWith("UVP") && x.product.Departments.Any(department => department.DepartmentName == "UV")) ? "UV" :
                            department.DepartmentName)
                    }
                )
                .Where(x => x.departmentName == "Unknown" || x.product.Departments.Any(d => d.IsProduction && d.IsActive))
                .Select(x => new TrendsInfoDto
                {
                    date = x.orderItem.Order.orderDate,
                    hasIncreasedPricing = x.store.HasIncreasedPricing,
                    productCost = x.product != null ? x.product.Cost : 0,
                    fulfillmentCost = x.product.FulfillmentCost == default ? 1 :
                                      x.productCustomFulfillment == null ? x.product.FulfillmentCost :
                                      x.productCustomFulfillment.CustomFulfillmentCost,
                    departmentName = x.departmentName,
                    orderId = x.orderItem.Order.ERPOrderId,
                    orderSku = x.orderItem.sku,
                    productProfit = 0
                })
                .AsNoTracking()
                .ToListAsync();


            foreach (var row in joinedData)
            {
                if (row.hasIncreasedPricing)
                {
                    row.fulfillmentCost = row.fulfillmentCost * 1.15m;
                }
                row.productProfit = row.fulfillmentCost - row.productCost;
            }
            var groupedData = joinedData
                .Where(f => f.departmentName != null && f.productProfit.HasValue && f.date.HasValue)
                .GroupBy(f => new { f.date.Value.Date, f.departmentName })
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.departmentName,
                    FulfillmentCost = g.Sum(f => f.fulfillmentCost.Value),
                    ProductProfit = g.Sum(f => f.productProfit.Value),
                    NumberOfOrders = g.Where(f => !string.IsNullOrEmpty(f.orderSku)).Select(f => f.orderId).Distinct().Count(),
                    ProductCost = g.Sum(f => f.productCost)

                })
                .GroupBy(g => g.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Departments = g.ToDictionary(d => d.departmentName, d => new
                    {
                        d.FulfillmentCost,
                        d.ProductProfit,
                        d.NumberOfOrders,
                        d.ProductCost
                    })
                })
                .OrderBy(g => g.Date)
                .ToList();

            var trendsInfo = groupedData.Select(g => new TrendsInfoDto
            {
                date = g.Date,
                departmentsOrders = g.Departments.ToDictionary(d => d.Key, d => d.Value.NumberOfOrders),
                departmentsFulfillmentCost = g.Departments.ToDictionary(d => d.Key, d => d.Value.FulfillmentCost),
                departmentsProductProfit = g.Departments.ToDictionary(d => d.Key, d => d.Value.ProductProfit),
                departmentsProductCost = g.Departments.ToDictionary(d => d.Key, d => d.Value.ProductCost)
            }).ToList();

            return trendsInfo;
        }

        public async Task<List<ProductSalesInfoDto>> ProductSalesTable(int days)
        {
            var startDate = DateTime.Today.AddDays(days).Date;

            var results = await _context.Orders
                .Where(order => order.orderDate.Date >= startDate)
                .Join(
                    _context.OrderItem,
                    order => order.ERPOrderId,
                    item => item.ERPOrderId,
                    (order, item) => new { order, item }
                )
                .Join(
                    _context.Product,
                    combined => combined.item.ERPProductId,
                    product => product.ProductId,
                    (combined, product) => new { combined.order, combined.item, product }
                )
                .Where(x => x.product.Departments.Any(d => d.IsProduction && d.IsActive))
                .GroupBy(x => new { x.product.Sku, x.product.Description })
                .Select(group => new ProductSalesInfoDto
                {
                    ProductSku = group.Key.Sku,
                    Description = group.Key.Description,
                    TotalQuantity = group.Sum(g => g.item.quantity),
                })
                .OrderByDescending(dto => dto.TotalQuantity)
                .Take(50)
                .ToListAsync();

            return results;
        }

        public async Task<List<YearlyProfitInfoDto>> GetYearlyProfits(DateTime startDate, DateTime endDate)
        {
            var yearlyProf = from o in _context.Orders
                             join oi in _context.OrderItem on new { o.ERPOrderId, adjustment = false } equals new { oi.ERPOrderId, oi.adjustment }
                             join b in _context.Bundle on oi.ERPBundleId equals b.BundleId into bJoin
                             from b in bJoin.DefaultIfEmpty()
                             join p in _context.Product on oi.ERPProductId equals p.ProductId into pJoin
                             from p in pJoin.DefaultIfEmpty()
                             where o.orderDate >= startDate && o.orderDate < endDate
                             select new
                             {
                                 Date = o.orderDate,
                                 o.orderDate.Year,
                                 Quantity = oi.quantity,
                                 FulfillmentCost = b != null && b.FulfillmentCost != 0 ? 
                                                  b.FulfillmentCost : 
                                                  (p != null ? p.FulfillmentCost : 0),
                                 ProductCost = p != null ? p.Cost : 0,
                                 ShipStationSales = o.orderTotal
                             };
            
            var results = await yearlyProf.ToListAsync();
            
            return [.. results
                .GroupBy(x => new {x.Year,x.Date.Date})
                .Select(g => new YearlyProfitInfoDto
                {
                    Year = g.Key.Year,
                    Date = g.Key.Date,
                    ItemsSold = g.Sum(x => x.Quantity),
                    Profits = g.Sum(x => (x.FulfillmentCost - x.ProductCost) * x.Quantity),
                    ShipStationSales = g.Sum(x => x.ShipStationSales)
                })];
        }

        public async Task<List<WeeklyProfit>> GetWeeklyProfits(DateTime startDate, DateTime endDate)
        {
            var query =
            from orderItem in _context.OrderItem
            join order in _context.Orders
                on orderItem.ERPOrderId equals order.ERPOrderId
            join product in _context.Product
                on orderItem.ERPProductId equals product.ProductId

            where order.orderStatus == OrderStatus.shipped
                  && order.orderDate >= startDate
                  && order.orderDate <= endDate

                  && (_context.OrderShipments.Any(sh => sh.ERPOrderId == order.ERPOrderId && !sh.voided)
                      || _context.OrderFulfillments.Any(f => f.ERPOrderId == order.ERPOrderId && !f.voided))

            group orderItem by new
            {
                product.ProductId,
                product.Sku,
                product.FulfillmentCost,
                product.Cost
            }
            into grouped
            select new WeeklyProfit
            (
                grouped.Key.ProductId,
                grouped.Sum(x => x.quantity) * grouped.Key.FulfillmentCost
                              - grouped.Sum(x => x.quantity) * grouped.Key.Cost,
                grouped.Sum(x => x.quantity),
                grouped.Key.Sku

            );
            var result = await query.AsNoTracking().ToListAsync();
            return result;
        }
    }
}
