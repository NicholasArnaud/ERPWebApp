using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Extensions;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.Reports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    /// <summary>  
    /// The current date and time in the Central Standard Time timezone.  
    /// </summary>
    protected readonly DateTime _now;
    public OrderRepository(ApplicationDbContext _context) : base(_context)
    {
        _now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    }
    public async Task<Order> GetOrderByOrderIdCustomSelectAsync(long orderId)
    {
        return await _context.Orders
            .Include(x => x.weight)
            .Include(x => x.dimensions)
            .Include(x => x.advancedOptions)
            .Include(x => x.insuranceOptions)
            .Include(x => x.shipFrom)
            .Include(x => x.shipTo)
            .Include(x => x.billTo)
            .Include(x => x.Sources)
            .Include(x => x.orderShipments)
            .Include(x => x.orderFulfillments)
            .Include(x => x.Tags)
            .Include(x => x.items)
            .ThenInclude(xx => xx.options)
            .FirstOrDefaultAsync(x => x.orderId == orderId);
    }
    public async Task<Order> GetOrderByOrderIdCustomSelectNoTrackingAsync(long orderId)
    {
        return await _context.Orders
            .Include(x => x.weight)
            .Include(x => x.dimensions)
            .Include(x => x.advancedOptions)
            .Include(x => x.insuranceOptions)
            .Include(x => x.shipFrom)
            .Include(x => x.shipTo)
            .Include(x => x.billTo)
            .Include(x => x.Sources)
            .Include(x => x.orderShipments)
            .Include(x => x.orderFulfillments)
            .Include(x => x.Tags)
            .Include(x => x.items)
            .ThenInclude(xx => xx.options)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.orderId == orderId);
    }
    public async Task<Order> GetOrderByOrderIdAndNullKeyCustomSelectAsync(long orderId)
    {
        return await _context.Orders
                .FirstOrDefaultAsync(x => x.orderId == orderId && (x.orderKey == "" || x.orderKey == null));
    }

    public async Task<Order> GetOrderByKeyCustomSelectAsync(string key)
    {
        return await _context.Orders.Include(x => x.weight)
                .Include(x => x.dimensions)
                .Include(x => x.advancedOptions)
                .Include(x => x.insuranceOptions)
                .Include(x => x.shipFrom)
                .Include(x => x.shipTo)
                .Include(x => x.billTo)
                .Include(x => x.Sources)
                .Include(x => x.orderShipments)
                .Include(x => x.orderFulfillments)
                .Include(x => x.Tags)
                .Include(x => x.items)
                .ThenInclude(xx => xx.options)
                .FirstOrDefaultAsync(x => x.orderKey == key);
    }
    public async Task<Order> GetOrderByOrderIdAndKeyCustomSelectAsync(long orderId, string key)
    {
        return await _context.Orders
            .Include(x => x.weight)
            .Include(x => x.dimensions)
            .Include(x => x.advancedOptions)
            .Include(x => x.insuranceOptions)
            .Include(x => x.shipFrom)
            .Include(x => x.shipTo)
            .Include(x => x.billTo)
            .Include(x => x.Sources)
            .Include(x => x.orderShipments)
            .Include(x => x.orderFulfillments)
            .Include(x => x.Tags)
            .Include(x => x.items)
            .ThenInclude(xx => xx.options)
            .FirstOrDefaultAsync(x => x.orderId == orderId && x.orderKey == key);
    }

    public async Task<Order> GetOrderByIdCustomSelectAsync(int id)
    {
        return await _context.Orders
            .Include(order => order.advancedOptions)
            .Include(order => order.Tags)
            .Include(order => order.weight)
            .Include(order => order.dimensions)
            .Include(order => order.insuranceOptions)
            .Include(order => order.shipFrom)
            .Include(order => order.shipTo)
            .Include(order => order.billTo)
            .Include(order => order.orderShipments)
            .Include(order => order.orderFulfillments)
            .Include(order => order.Sources)
            .Include(order => order.items)
                .ThenInclude(orderItem => orderItem.Bundle)
                    .ThenInclude(bundle => bundle.BundleItems)
                        .ThenInclude(bundleItem => bundleItem.Product)
            .Include(order => order.items)
                .ThenInclude(orderItem => orderItem.Product)
            .Include(order => order.items)
                .ThenInclude(orderItem => orderItem.options)
            .FirstOrDefaultAsync(x => x.ERPOrderId == id);
    }

    public List<Report> GetReports(
        string procedure,
        SqlParameter[] parameters,
        Func<DbDataReader, Report> mapResult,
        int timeout = 0
    )
    {
        var conn = _context.Database.GetDbConnection();
        try
        {
            conn.Open();

            var reader = ExecuteStoredProcedure(conn, procedure, timeout, parameters);

            var results = new List<Report>();
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    results.Add(mapResult(reader));
                }

            }

            reader.Close();
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        finally
        {
            conn.Close();
        }
    }


    public List<Dictionary<string, string>> GetOrderAndItemsSumByDate(DateTime fromDate, DateTime toDate)
    {
        var result = (
            from a in (
                from orderItem in _context.OrderItem
                join order in _context.Orders
                    on orderItem.ERPOrderId equals order.ERPOrderId
                where order.shipDate >= fromDate && order.shipDate <= toDate
                join product in _context.Product
                    on orderItem.ERPProductId equals product.ProductId into productGroup
                from product in productGroup.DefaultIfEmpty()
                group new { orderItem, product } by new { order.shipDate, orderItem.ERPOrderId } into g
                select new
                {
                    g.Key.shipDate,
                    g.Key.ERPOrderId,
                    ItemsCount = g.Count(),
                    FulfillmentCost = g.Sum(x => x.product.FulfillmentCost),
                    Cost = g.Sum(x => x.product.Cost),
                    LaborCost = g.Sum(x => x.product.LaborCost)
                }
            )
            join order in _context.Orders
                on a.ERPOrderId equals order.ERPOrderId
            where order.shipDate >= fromDate && order.shipDate <= toDate
            group new { a, order } by a.shipDate into g
            select new Dictionary<string, string>{
                { "Shipped Date", g.Key.ToString()},
                { "Sum of shipped items", g.Sum(x => x.a.ItemsCount).ToString() },
                { "Sum of shipped order totals", g.Sum(x => x.order.orderTotal).ToString() },
                { "Sum of fulfillment from items", g.Sum(x => x.a.FulfillmentCost).ToString() },
                { "Sum of cost from items", g.Sum(x => x.a.Cost).ToString() },
                { "Sum of labor from items", g.Sum(x => x.a.LaborCost).ToString() }
            }
        ).ToList();

        return result;
    }

    public async Task<(List<Order>, int)> GetOrdersAsync(
        int start,
        int length,
        List<string> ordernumbers,
        string itemName,
        OrderStatus[] orderStatus,
        int storeId,
        int[] productIds,
        int[] departmentIds,
        int[] orderTagId,
        string orderStartDate,
        string orderEndDate,
        string shipByDate,
        string orderColumn,
        string orderDir = "asc",
        int? orderBatchId = null,
        List<string> excludeItemNames = null,
        bool includeBatchedOrders = true
    )
    {
        _ = DateTime.TryParse(orderStartDate, out DateTime OrderDateTimeStart);
        _ = DateTime.TryParse(orderEndDate, out DateTime OrderDateTimeEnd);
        _ = DateTime.TryParse(shipByDate, out DateTime ShipByDateTime);
        var today = DateTime.Now;

        List<int> orderIds = orderBatchId.HasValue
            ? await _context.OrderBatchItem
                .Where(x => x.OrderBatchId == orderBatchId.Value && x.ERPOrderId.HasValue)
                .Select(x => x.ERPOrderId.Value)
                .Distinct()
                .ToListAsync()
            : null;

        if (storeId != 0)
        {
            storeId = await _context.ShipStationStore
                .Where(x => x.ShipStationStoreId == storeId)
                .Select(x => x.StoreId)
                .FirstOrDefaultAsync();
        }


        var query = _context.Orders
            .Include(o => o.advancedOptions)
            .Include(o => o.Tags)
            .Include(o => o.items)
            .Where(A =>
                (orderStatus == null || orderStatus.Contains(A.orderStatus))
                && (
                    (string.IsNullOrEmpty(orderStartDate) && string.IsNullOrEmpty(orderEndDate) && A.orderDate >= today.AddMonths(-1))
                    || (A.orderDate >= OrderDateTimeStart && A.orderDate < OrderDateTimeEnd.AddDays(1))
                )
                && (string.IsNullOrEmpty(shipByDate) || A.shipByDate.HasValue && A.shipByDate.Value.Date == ShipByDateTime.Date)
                && (ordernumbers.Count == 0 || ordernumbers.Any(x => A.orderNumber.Contains(x)))
                && (storeId == 0 || A.advancedOptions.storeId == storeId)
                && (productIds == null || productIds.Length == 0 || A.items.Any(i => productIds.Contains(i.ERPProductId ?? 0)))
                && (
                    departmentIds == null
                    || departmentIds.Length == 0
                    || A.items.Any(i => i.Bundle.BundleItems.Any(bi => bi.Product.Departments.Any(d => departmentIds.Contains(d.DepartmentId))))
                    || A.items.Any(i => i.Product.Departments.Any(d => departmentIds.Contains(d.DepartmentId)))
                )
                && (orderTagId == null || orderTagId.Length == 0 || A.Tags.Any(t => orderTagId.Contains(t.tagId)))
                && (string.IsNullOrEmpty(itemName) || A.items.Any(x => x.name.Contains(itemName)))
                && (excludeItemNames == null || !A.items.Any(x => excludeItemNames.Contains(x.name)))
                && (orderIds == null || orderIds.Contains(A.ERPOrderId))
                && (includeBatchedOrders || !_context.OrderBatchItem.Any(b => b.ERPOrderId == A.ERPOrderId))
            );

        var count = await query.CountAsync();

        query = query.SmartSort(orderColumn, !orderDir.Equals("asc"))
            .SmartPaging(start, length);

        var orders = await query.ToListAsync();

        return (orders, count);
    }

    public async Task<List<Product>> GetOrderProducts(string orderNumber)
    {
        var directProductsQuery = from order in _context.Orders
                                  where order.orderNumber == orderNumber
                                  join orderItem in _context.OrderItem on order.ERPOrderId equals orderItem.ERPOrderId
                                  join product in _context.Product on orderItem.ERPProductId equals product.ProductId
                                  select product;

        var bundleProductsQuery = from order in _context.Orders
                                  where order.orderNumber == orderNumber
                                  join orderItem in _context.OrderItem on order.ERPOrderId equals orderItem.ERPOrderId
                                  join bundleItem in _context.BundleItem on orderItem.ERPBundleId equals bundleItem.BundleId
                                  join product in _context.Product on bundleItem.ProductId equals product.ProductId
                                  select product;

        // Combine and remove duplicates
        var productList = await directProductsQuery
            .Union(bundleProductsQuery)
            .Distinct()
            .ToListAsync();
        return productList;
    }

    public async Task<List<TopDepartment>> GetTopDepartmentsByShipment(DateTime startDate, DateTime endDate)
    {
        var result = await (
                from a in _context.OrderItem
                join c in _context.Product on a.ERPProductId equals c.ProductId
                where EF.Functions.DateDiffDay(startDate, a.OrderShipment.shipDate) >= 0
                        && EF.Functions.DateDiffDay(a.OrderShipment.shipDate, endDate) < 0
                select new
                {
                    a.OrderShipment.shipDate,
                    a.sku,
                    c.ProductId,
                    Department = c.Departments
                        .OrderBy(dep => dep.DepartmentName == "UV" ? 0 : 1)
                        .Select(dep => new { dep.DepartmentName, dep.DepartmentColor })
                        .FirstOrDefault()
                }
            ).AsNoTracking()
            .ToListAsync();

        var processedData = result.Select(x => new
        {
            ShipDate = new DateTime(x.shipDate.Year, x.shipDate.Month, 1),
            DepartmentName = x.Department == null ? "unknown"
                : x.sku.EndsWith("UVP") && x.Department.DepartmentName == "UV" ? "UV"
                : x.Department.DepartmentName,
            x.Department?.DepartmentColor,
            TotalItemsShipped = 1
        })
        .GroupBy(d => new { d.ShipDate, d.DepartmentName, d.DepartmentColor })
        .Select(g => new TopDepartment
        (
            g.Key.ShipDate,
            g.Key.DepartmentName,
            g.Key.DepartmentColor,
            g.Count()
        ))
        .OrderByDescending(x => x.TotalItemsShipped)
        .ToList();

        return processedData;
    }

    private static float GetDepartmentGoal(SpeedOMeterGoal speedOMeterGoal, string departmentName)
    {
        // Converting department name to PascalCase and append "Goal" at the end; this is all temporary while I'm working on dynamically adding target goals. 
        var goalPropertyName = departmentName == "UVP" ? "UVGoal" : $"{departmentName.Substring(0, 1).ToUpper()}{departmentName.Substring(1).ToLower()}Goal";

        // Using reflection here to get the PropertyInfo for the goal property  
        var goalProperty = typeof(SpeedOMeterGoal).GetProperty(goalPropertyName);

        // If the goal property exists, return its value, otherwise return 0  
        if (goalProperty != null)
        {
            return Convert.ToSingle(goalProperty.GetValue(speedOMeterGoal));
        }
        return 0;
    }

    private static float CalculateActualTarget(SpeedOMeterGoal speedOMeterGoal, string departmentName, DateTime currentTime)
    {
        //Applying the same target logic that was used previously. Can be changed if desired.
        float departmentGoal = GetDepartmentGoal(speedOMeterGoal, departmentName);
        float currentHourFraction = currentTime.Hour + currentTime.Minute / 60f;
        float target = (currentHourFraction * (departmentGoal / 24f)) == 0f ? 1f : (currentHourFraction * (departmentGoal / 24f));

        return (float)((Math.Round(target) == 0f) ? 1 : (int)Math.Round(target));
    }

    public async Task<List<TallyDto>> GetDailyOrderCompletionCount()
    {
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        DateTime currentDateInCst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
        DateTime startDate = currentDateInCst.Date;

        var speedOMeterGoal = await _context.SpeedOMeterGoal
            .OrderByDescending(x => x.ModifyDate)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var query = (
            from d in _context.Department
            where d.IsProduction && d.IsActive
            join o in _context.Orders
            on true equals true
            where o.orderStatus == OrderStatus.shipped && o.shipDate.Value.Date == startDate
            from oi in o.items
            from pd in oi.Product.Departments
            where pd.IsProduction && pd.IsActive && pd.DepartmentId == d.DepartmentId
            select new
            {
                Department = d,
                ProductSku = oi.Product.Sku,
                Quantity = oi.quantity
            }
        )
        .GroupBy(x => new { x.Department.DepartmentName, x.Department.DepartmentId })
        .Select(
            x => new TallyDto
            {
                DepartmentName = x.Key.DepartmentName,
                DepartmentId = x.Key.DepartmentId,
                Tally = x.Sum(order => order.Quantity),
                Target = speedOMeterGoal != null ? CalculateActualTarget(speedOMeterGoal, x.Key.DepartmentName, currentDateInCst) : default,
                OriginalTarget = speedOMeterGoal != null ? GetDepartmentGoal(speedOMeterGoal, x.Key.DepartmentName) : default,
                DepartmentGoals = x.GroupBy(order => order.ProductSku)
                                        .Select(g => new DepartmentProductInfo
                                        {
                                            DepartmentName = x.Key.DepartmentName,
                                            DepartmentId = x.Key.DepartmentId,
                                            ProductSku = g.Key,
                                            Quantity = g.Sum(order => order.Quantity)
                                        }).ToList()
            }
        );

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<ShipstationOrderDto>> GetDailyShipstationOrdersAll(DateTime startDate, DateTime endDate, int? departmentId = null)
    {
        var results = await _context.Orders
            .Where(
                o => o.orderKey != null &&
                o.orderStatus != OrderStatus.cancelled &&
                (
                    (
                        o.orderDate.Date >= startDate.Date &&
                        o.orderDate.Date <= endDate.Date
                    ) ||
                    (
                        o.shipDate.HasValue &&
                        o.shipDate.Value.Date >= startDate.Date &&
                        o.shipDate.Value.Date <= endDate.Date
                    )
                ) &&
                (
                    !departmentId.HasValue ||
                    o.items.Any(oi => oi.Product.Departments.Any(dm => dm.DepartmentId == departmentId))
                )
        )
        .GroupBy(o => new { o.orderDate.Date, ShipDate = o.shipDate.HasValue ? o.shipDate.Value.Date : default(DateTime?) })
        .Select(g => new ShipstationOrderDto
        {
            OrderDate = g.Key.Date,
            ShipDate = g.Key.ShipDate.HasValue ? g.Key.ShipDate.Value.Date : default,
            OrdersIn = g.Count(),
            OrdersOut = g.Count()
        })
        .OrderBy(o => o.OrderDate)
        .AsNoTracking()
        .ToListAsync();

        return results;
    }

    public async Task<List<YearlyProductShippedReport>> GetYearlyProductCountReport()
    {
        var groupedData = await (
            from o in _context.Orders
                .Where(
                    x => x.orderStatus == OrderStatus.shipped && (x.orderShipments.Any(os => !os.voided) || x.orderFulfillments.Any(of => !of.voided))
                )
            join oi in _context.OrderItem on o.ERPOrderId equals oi.ERPOrderId
            join p in _context.Product on oi.ERPProductId equals p.ProductId
            group new { oi, o } by new { p.ProductId, p.Sku, Year = o.orderDate.Year } into g
            select new
            {
                g.Key.ProductId,
                g.Key.Sku,
                g.Key.Year,
                Quantity = g.Sum(x => x.oi.quantity)
            }
        ).AsNoTracking().ToListAsync();

        var result = groupedData
            .GroupBy(g => new { g.ProductId, g.Sku })
            .SelectMany(group => group
                .OrderBy(g => g.Year)
                .Select((item, index) => new YearlyProductShippedReport(
                    item.ProductId,
                    item.Sku,
                    item.Year.ToString(),
                    GetOrdinalYearLabel(index + 1),
                    item.Quantity
                ))
            )
            .ToList();
        return result;

        static string GetOrdinalYearLabel(int yearIndex) => yearIndex switch
        {
            11 or 12 or 13 => $"{yearIndex}th Year Qty",
            _ when yearIndex % 10 == 1 => $"{yearIndex}st Year Qty",
            _ when yearIndex % 10 == 2 => $"{yearIndex}nd Year Qty",
            _ when yearIndex % 10 == 3 => $"{yearIndex}rd Year Qty",
            _ => $"{yearIndex}th Year Qty"
        };
    }

    public Task<OrderShippingInfo> GetOrderShipToAddressAsync(long orderId, string orderKey)
        => _context.Orders.Where(o => o.orderKey == orderKey && o.orderId == orderId)
            .Select(x => x.shipTo)
            .AsNoTracking()
            .FirstOrDefaultAsync();

}
