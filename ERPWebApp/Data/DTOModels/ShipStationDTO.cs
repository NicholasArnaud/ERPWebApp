using ERPWebApp.Models.Orders;
using System.Text.Json.Serialization;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.DTOModels
{
    /// <summary>  
    /// Represents a ShipStation webhook payload with the event type and resource URL.  
    /// </summary>  
    public class ShipStationWebhookDTO
    {
        /// <summary>  
        /// The event type that triggered the webhook. Will be one of the following values:  
        /// ORDER_NOTIFY, ITEM_ORDER_NOTIFY, SHIP_NOTIFY, ITEM_SHIP_NOTIFY, FULFILLMENT_SHIPPED, FULFILLMENT_REJECTED  
        /// </summary>  
        public string resource_type { get; set; }

        /// <summary>  
        /// The URL to access the resource associated with the webhook event.  
        /// </summary>  
        public string resource_url { get; set; }
    }

    /// <summary>  
    /// Represents a ShipStation webhook order payload with orders, shipments, and fulfillments.  
    /// </summary>  
    public class ShipStationWebhookOrderDTO
    {
        /// <summary>  
        /// A list of orders associated with the webhook event.  
        /// </summary>  
        [property: JsonPropertyName("orders")]
        public List<Order> Orders { get; set; }

        /// <summary>  
        /// A list of shipments associated with the webhook event.  
        /// </summary>  
        [property: JsonPropertyName("shipments")]
        public List<OrderShipment> Shipments { get; set; }

        /// <summary>  
        /// A list of fulfillments associated with the webhook event.  
        /// </summary>  
        [property: JsonPropertyName("fulfillments")]
        public List<OrderFulfillment> Fulfillments { get; set; }

        /// <summary>  
        /// The total number of records associated with the webhook event.  
        /// </summary>  
        [property: JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>  
        /// The current page of the webhook event data.  
        /// </summary>  
        [property: JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>  
        /// The total number of pages in the webhook event data.  
        /// </summary>  
        [property: JsonPropertyName("pages")]
        public int Pages { get; set; }
    }

    public record ShipStationJson(
        int StoreId,
        string StoreName,
        string Email,
        string PublicEmail,
        bool IsActive
    );

    public record ShipStationOrderFilter
    {
        public const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";
        public string? CustomerName { get; set; }
        public string? ItemKeyword { get; set; }
        public DateTime? CreateDateStart { get; set; }
        public DateTime? CreateDateEnd { get; set; }
        public DateTime? ModifyDateStart { get; set; }
        public DateTime? ModifyDateEnd { get; set; }
        public DateTime? OrderDateStart { get; set; }
        public DateTime? OrderDateEnd { get; set; }
        public string? OrderNumber { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime? PaymentDateStart { get; set; }
        public DateTime? PaymentDateEnd { get; set; }
        public int? StoreId { get; set; }
        public string SortBy { get; set; } = "OrderDate";
        public string SortDir { get; set; } = "DESC";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 2;
    }
}
