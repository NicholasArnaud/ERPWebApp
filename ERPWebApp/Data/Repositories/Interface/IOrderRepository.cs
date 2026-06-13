using System.Data.Common;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.Reports;
using Microsoft.Data.SqlClient;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories.Interface;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetOrderByKeyCustomSelectAsync(string key);
    Task<Order> GetOrderByOrderIdCustomSelectAsync(long orderId);
    Task<Order> GetOrderByOrderIdCustomSelectNoTrackingAsync(long orderId);
    Task<Order> GetOrderByOrderIdAndNullKeyCustomSelectAsync(long orderId);
    Task<Order> GetOrderByOrderIdAndKeyCustomSelectAsync(long orderId, string key);
    Task<Order> GetOrderByIdCustomSelectAsync(int id);

    List<Report> GetReports(
        string procedure,
        SqlParameter[] parameters,
        Func<DbDataReader, Report> mapResult,
        int timeout = 0
    );

    List<Dictionary<string, string>> GetOrderAndItemsSumByDate(DateTime fromDate, DateTime toDate);
    public Task<(List<Order>, int)> GetOrdersAsync(
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
     );
    public Task<List<Product>> GetOrderProducts(string orderNumber);
    Task<List<TopDepartment>> GetTopDepartmentsByShipment(DateTime startDate, DateTime endDate);
    Task<List<TallyDto>> GetDailyOrderCompletionCount();
    Task<List<ShipstationOrderDto>> GetDailyShipstationOrdersAll(DateTime startDate, DateTime endDate, int? departmentId = null);
    public Task<List<YearlyProductShippedReport>> GetYearlyProductCountReport();
    Task<OrderShippingInfo> GetOrderShipToAddressAsync(long orderId, string orderKey);
}
