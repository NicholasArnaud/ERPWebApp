using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using Microsoft.EntityFrameworkCore;
using ERPWebApp.Models.Orders;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories;

public class OrderShipmentRepository : Repository<OrderShipment>, IOrderShipmentRepository
{
    protected readonly DateTime _now;
    public OrderShipmentRepository(ApplicationDbContext context) : base(context)
    {
        _now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    }
    private async Task<List<DepartmentShippedTotalDTO>> DepartmentShippedTotalAsync()
    {
        var shippedOrders = new List<DepartmentShippedTotalDTO>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = @"  
                WITH OrderShipmentsCTE AS (  
                    SELECT ERPOrderId, MIN(CreateDate) AS CreateDate  
                    FROM OrderShipments  
                    WHERE voided = 0  
                    GROUP BY ERPOrderId  
                ),  
                OrderFulfillmentsCTE AS (  
                    SELECT ERPOrderId, MIN(CreateDate) AS CreateDate  
                    FROM OrderFulfillments  
                    WHERE voided = 0  
                    GROUP BY ERPOrderId  
                ),  
                OrderItemsWithDepartments AS (  
                    SELECT  
                        COALESCE(OS.CreateDate, OOF.CreateDate, O.shipDate) AS ShipDate,  
                        COALESCE(PD.DepartmentsDepartmentId, -1) AS DepartmentId,  
                        OI.quantity  
                    FROM  
                        OrderItem AS OI  
                    INNER JOIN Orders AS O ON OI.ERPOrderId = O.ERPOrderId  
                    LEFT JOIN OrderShipmentsCTE AS OS ON O.ERPOrderId = OS.ERPOrderId  
                    LEFT JOIN OrderFulfillmentsCTE AS OOF ON O.ERPOrderId = OOF.ERPOrderId  
                    LEFT JOIN DepartmentProduct AS PD ON OI.ERPProductId = PD.ProductsProductId  
                    LEFT JOIN Department AS D ON PD.DepartmentsDepartmentId = D.DepartmentId  
                    WHERE  
                        O.orderStatus = 2 AND O.shipDate IS NOT NULL AND OI.sku <> ''  
                        AND (D.DepartmentId IS NULL OR (D.IsProduction = 1 AND D.IsActive = 1  
                        --MultiDepartment Conflict Checks  
                        AND ((PD.DepartmentsDepartmentId = 3 AND (UPPER(OI.sku) LIKE '%UVP%'))  
                        OR (PD.DepartmentsDepartmentId = 2 AND NOT (UPPER(OI.sku) LIKE '%UVP%'))  
                        OR (PD.DepartmentsDepartmentId NOT IN (2,3)))))    
                )  
                SELECT  
                    CAST(ShipDate AS DATE) AS Date,  
                    DepartmentId,  
                    SUM(quantity) AS ShipCount  
                FROM  
                    OrderItemsWithDepartments  
                GROUP BY  
                    CAST(ShipDate AS DATE), DepartmentId";

            _context.Database.OpenConnection();

            using (var result = await command.ExecuteReaderAsync())
            {
                while (await result.ReadAsync())
                {
                    shippedOrders.Add(new DepartmentShippedTotalDTO
                    {
                        Date = result.GetDateTime(0),
                        DepartmentId = result.GetInt32(1),
                        ShipCount = result.GetInt32(2)
                    });
                }
            }

            _context.Database.CloseConnection();
        }
        return shippedOrders;
    }


    public IQueryable<DepartmentShippedTotalDTO> GetAllDepartmentShippedTotals()
    {
        var departmentShippedTotalsList = _context.Orders
            .Where(o => o.orderStatus == OrderStatus.shipped)
            .Include(o => o.items)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Departments)
            .SelectMany(o => o.items, (order, item) => new { order, item })
            .SelectMany(x => x.item.Product.Departments
                .Where(d => d.IsProduction && d.IsActive)
                .Select(d => new { x.order.shipDate.Value.Date, Department = d, x.item.quantity }))
            .GroupBy(x => new { x.Date, x.Department })
            .Select(x => new DepartmentShippedTotalDTO
            {
                Date = x.Key.Date,
                DepartmentId = x.Key.Department.DepartmentId,
                ShipCount = x.Sum(order => order.quantity)
            });
        return departmentShippedTotalsList;
    }

    public async Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsByDateAsync()
    {
        var departments = await _context.Department
            .Where(d => d.IsProduction && d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        // Add the "Unknown" department  
        departments.Add(new Department
        {
            DepartmentId = -1,
            DepartmentName = "Unknown",
            IsProduction = true,
            IsActive = true
        });

        var shippedOrders = await DepartmentShippedTotalAsync();
        //var shippedOrders = await DepartmentShippedTotal().ToListAsync();

        // If there are no shipped orders within the specified date range, return an empty result.  
        if (!shippedOrders.Any())
        {
            return new List<DepartmentShippedTotalByDateDTO>();
        }

        var result = shippedOrders
            .GroupBy(x => x.Date)
            .Select(x => new DepartmentShippedTotalByDateDTO
            {
                Date = x.Key,
                DepartmentTotals = departments.ToDictionary(d => d.DepartmentName, d => x.Where(y => y.DepartmentId == d.DepartmentId).Sum(y => y.ShipCount))
            }).OrderByDescending(x => x.Date).ToList();

        return result;
    }

    public async Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsInRangeAsync(DateTime startDate, DateTime endDate)
    {
        var departments = await _context.Department
            .Where(d => d.IsProduction && d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        // Add the "Unknown" department  
        departments.Add(new Department
        {
            DepartmentId = -1,
            DepartmentName = "Unknown",
            IsProduction = true,
            IsActive = true
        });

        var pullDepartmentOrders = await DepartmentShippedTotalAsync();
        var shippedOrders = pullDepartmentOrders.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
        // If there are no shipped orders within the specified date range, return an empty result.  
        if (!shippedOrders.Any())
        {
            return new List<DepartmentShippedTotalByDateDTO>();
        }
        //Find the minimum and maximum shipped dates
        DateTime minDate = shippedOrders.Where(x => x.Date >= startDate).Min(x => x.Date);
        DateTime maxDate = shippedOrders.Where(x => x.Date <= endDate).Max(x => x.Date);

        //Generate a list of all dates between the minimum and maximum shipped dates
        List<DateTime> allDates = Enumerable.Range(0, (maxDate - minDate).Days + 1)
            .Select(offset => minDate.AddDays(offset)).ToList();

        var result = allDates
            .Select(date => new DepartmentShippedTotalByDateDTO
            {
                Date = date,
                DepartmentTotals = departments.ToDictionary(d => d.DepartmentName, d => 0)
            })
            .Union(shippedOrders.GroupBy(x => x.Date)
            .Select(x => new DepartmentShippedTotalByDateDTO
            {
                Date = x.Key,
                DepartmentTotals = departments.ToDictionary(d => d.DepartmentName, d => x.Where(y => y.DepartmentId == d.DepartmentId).Sum(y => y.ShipCount))
            })).GroupBy(x => x.Date)
            .Select(x => new DepartmentShippedTotalByDateDTO
            {
                Date = x.Key,
                DepartmentTotals = departments.ToDictionary(d => d.DepartmentName, d => x.SelectMany(y => y.DepartmentTotals).Where(y => y.Key == d.DepartmentName).Sum(y => y.Value))
            }).OrderByDescending(x => x.Date).ToList();

        return result;
    }

    public async Task<List<OrderShipmentsByServiceDTO>> GetDaysShipmentsByServiceCode()
    {
        var result = await _context.OrderShipments.Where(x => x.shipDate.Date == _now.Date && !x.voided)
            .GroupBy(x => new { x.carrierCode, x.serviceCode })
            .Select(g => new OrderShipmentsByServiceDTO
            {
                CarrierCode = g.Key.carrierCode,
                ServiceCode = g.Key.serviceCode,
                TotalShipments = g.Count()
            })
            .AsNoTracking()
            .ToListAsync();
        return result;
    }
}