using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.Reports;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Services.IServices
{
    /// <summary>  
    /// Interface for the OrderService, providing methods for managing Order, OrderShipment, and OrderFulfillment entities.  
    /// </summary>  
    public interface IOrderService : IService<Order>
    {
        /// <summary>  
        /// Processes the 'ORDER_NOTIFY' webhook payload.  
        /// </summary>  
        /// <param name="context">The ApplicationDbContext instance.</param>  
        /// <param name="orders">The list of orders to process.</param>
        Task ProcessOrderNotify(List<Order> orders);
        /// <summary>  
        /// Processes the 'ITEM_SHIP_NOTIFY' webhook payload.  
        /// </summary>  
        /// <param name="context">The ApplicationDbContext instance.</param>  
        /// <param name="shipments">The list of shipments to process.</param>
        Task ProcessItemShipNotify(List<OrderShipment> shipments);
        /// <summary>  
        /// Processes the 'FULFILLMENT_SHIPPED' webhook payload.  
        /// </summary>  
        /// <param name="context">The ApplicationDbContext instance.</param>  
        /// <param name="fulfillments">The list of fulfillments to process.</param>
        Task ProcessFulfillmentShipped(List<OrderFulfillment> fulfillments);

        /// <summary>
        /// Grabs the first order in the database by order number
        /// </summary>
        /// <param name="orderNumber">The Order Number to search by</param>
        /// <returns>The base Order object</returns>
        Task<Order> GetOrderByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetOrderUpdates(List<int> ERPOrderIdList);
        Task<Order> FindMissingOrder(string orderNumber, int storeId);
        Order MapSkulabsDtoToOrder(SkulabsDTO skulabsDto, string requiredLocationId);
        Task<List<Order>> ConvertZazzleToOrder(ZazzleDTO zazzleDTO);
        Order SetOrderDimensionsFromItems(Order order);
        Order SetOrderWeightFromItems(Order order);
        Task<Order> ConvertShopifyToOrder(ShopifyDTO shopifyDTO);
        Task<(List<Order>, int)> GetOrdersAsync(
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
        Task<List<Product>> GetOrderProducts(string orderNumber);
        List<Dictionary<string, string>> GetOrderItemsByDate(DateTime fromDate, DateTime toDate);
        List<Dictionary<string, string>> GetOrderItemsSumByDate(DateTime fromDate, DateTime toDate);
        Task<List<YearlyProductShippedReport>> GetYearlyProductCountReport();
        Task<OrderShippingInfo> GetOrderShipToAddressAsync(long orderId, string orderKey);
        Task<List<OrderTag>> GetShipStationTags();
        Task<string> SetShipStationCompletedTag(string orderId, string tagid = "99825");
        Task<List<OrderTag>> GetShipStationTagsByIdsAsync(int[] tagIds);
        Task<List<Order>> GetShipStationOrdersWithTag(int tagId);
        Task<List<Order>> GetShipStationOrderDetails(string orderNumber, int storeId);
        Task<string> AddOrRemoveShipStationTagAsync(long orderId, int tagId, bool addOrRemoveTag);
        Task ForceUpdateOrders(ShipStationOrderFilter shipStationOrderFilter);
        Task<List<OrderShipment>> GetShipStationShipmentsAsync(long orderId);
        Task<List<OrderFulfillment>> GetShipStationFulfillmentsAsync(long orderId);
        Task<Order> SetOrderAsShipped(string orderId, string carrierCode, DateTime shipDate, string trackingNumber);
        Task<List<ShipEngineShippingEstimate>> GetShipEngineEstimatedShipmentRate(List<string> carrierIds, Order order);
        Task<ShipEngineLabel> GetShipEngineOrderLabel(string TrackingNumber);
        Task<ShipStationVoidMessage> VoidShipmentLabel(long shipmentId);
        Task<ShipEngineVoidMessage> VoidFulfillmentLabel(string labelId);
        Task<ShipEngineLabel> CreateShipEngineShipmentLabel(Order order);
        Task<UspsAddressValidationResponse> UspsAddressValidation(string queryString);
    }
}
