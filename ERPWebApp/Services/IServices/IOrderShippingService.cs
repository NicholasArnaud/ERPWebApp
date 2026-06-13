using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Orders;
using System.Linq.Expressions;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;

namespace ERPWebApp.Services.IServices
{
    public interface IOrderShippingService : IService<OrderShipment>
    {
        Task<Order> GetOrderByOrderIdAndKeyCustomSelectAsync(long orderId, string orderKey);
        Task<Order> GetRateEstimate(Order ssosData);
        Task<List<AvailableShipmentCarrier>> GetBestRates(Order ssosData);
        Task<Order> GenerateLabelShipEngine(Order ssosData, string shippedByUsername);
        Task<Order> GenerateLabelZazzle(Order ssosData, string shippedByUsername);
        Task<List<DepartmentShippedTotalDTO>> GetDepartmentShippedTotalsListAsync();
        Task<List<DepartmentShippedTotalByDateDTO>> GetDepartmentShippedTotalsByDateList();
        Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsInRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order> VoidAsync(int orderId, OrderShipment row);
        Task<bool> VoidUspsLabel(string trackingNumber);
        Task<OrderShipment> GetROWAsync(int id, string timestamp);
        Task<OrderShipment> GetShipmentAsync(
            Expression<Func<OrderShipment, bool>> expression,
            params Expression<Func<OrderShipment, object>>[] includes
        );
        Task<List<OrderShipment>> GetShipmentListAsync(
            Expression<Func<OrderShipment, bool>> expression,
            Expression<Func<OrderShipment, string>>[] orderSelectors = null,
            params Expression<Func<OrderShipment, object>>[] includes
        );
        List<OrderShipment> GetShipmentList(
           Expression<Func<OrderShipment, bool>> expression,
           Expression<Func<OrderShipment, string>>[] orderSelectors = null,
           Expression<Func<OrderShipment, object>>[] includes = null
       );
        void OnUpdateShipment(OrderShipment orderShipment);
        void OnBulkUpdateShipments(List<OrderShipment> orderShipmentList);
        List<Report> GetAvgShippingCostInDateRangeBySku(DateTime StartDate, DateTime EndDate);
        List<Report> GetAvgShippingCostInDateRangeByService(DateTime StartDate, DateTime EndDate);
        List<Report> GetAmountItemsShippedByDateRange(DateTime StartDate, DateTime EndDate);
        List<Report> GetAmountShippedByDateRangeSkuFilter(int? ProductId, DateTime StartDate, DateTime EndDate);
        Task<Order> GenerateLabelShopify(Order ssosData, string shippedByUsername);

    }
}