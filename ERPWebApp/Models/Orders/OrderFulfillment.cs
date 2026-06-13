using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Models.Orders;

public class OrderFulfillment
{
    [Key]
    public int OrderFulfillmentId { get; set; }
    public int ERPOrderId { get; set; }
    [ForeignKey(nameof(ERPOrderId))]
    public Order Order { get; set; }
    public int orderId { get; set; }
    [Display(Name = "Order Number"), MaxLength(35)]
    public string orderNumber { get; set; }
    [Display(Name = "ShipStation User Id")]
    public string userId { get; set; }
    [Display(Name = "Customer Email"), MaxLength(50)]
    public string customerEmail { get; set; }
    [Display(Name = "Tracking Number"), MaxLength(50)]
    public string trackingNumber { get; set; }
    [Display(Name = "Create Date")]
    public DateTime createDate { get; set; }
    [Display(Name = "Ship Date")]
    public DateTime shipDate { get; set; }
    [Display(Name = "Void Date")]
    public DateTime? voidDate { get; set; }
    [Display(Name = "Delivery Date")]
    public DateTime? deliveryDate { get; set; }
    [Display(Name = "Carrier Code"), MaxLength(150)]
    public string carrierCode { get; set; }
    [Display(Name = "Fulfillment Provider Code"), MaxLength(150)]
    public string fulfillmentProviderCode { get; set; }
    [Display(Name = "Fulfillment Service Code"), MaxLength(150)]
    public string fulfillmentServiceCode { get; set; }
    [Display(Name = "Fulfillment Fee"), Column(TypeName = "decimal(18,2)")]
    public decimal? fulfillmentFee { get; set; }
    [Display(Name = "Is Void Requested?")]
    public bool? voidRequested { get; set; }
    [Display(Name = "Is Void")]
    public bool voided { get; set; }
    [Display(Name = "Is Marketplace Notified?")]
    public bool? marketplaceNotified { get; set; }
    [Display(Name = "Notify Error Message"), MaxLength(200)]
    public string notifyErrorMessage { get; set; }
    [Display(Name = "Ship To")]
    public OrderShippingInfo shipTo { get; set; }
    [Display(Name = "Seller Fill Provider Id")]
    public int? sellerFillProviderId { get; set; }
    [Display(Name = "Seller Fill Provider Name"), MaxLength(200)]
    public string sellerFillProviderName { get; set; }
    [Timestamp]
    public byte[] ERPTimestamp { get; internal set; }
    public string ERPModifyByUserId { get; internal set; }
    [Display(Name = "Modify Date")]
    public DateTime ERPModifyDate { get; set; }
    //[Display(Name = "Is Shipping Received?")]
    //public bool IsShippingReceived { get; set; } = false;
}
