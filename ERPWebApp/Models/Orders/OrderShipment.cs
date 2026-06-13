using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Models.Orders;

public class OrderShipment
{
    [Key]
    public int OrderShipmentId { get; set; }
    [Display(Name = "Shipment Id")]
    public long shipmentId { get; set; }
    public int ERPOrderId { get; set; }
    [ForeignKey(nameof(ERPOrderId))]
    public Order Order { get; set; }
    [Display(Name = "Order Id")]
    public long orderId { get; set; }
    [Display(Name = "User Id"), MaxLength(100)]
    public string userId { get; set; }
    [ForeignKey("Order Key"), MaxLength(100)]
    public string orderKey { get; set; }
    [Display(Name = "Create Date")]
    public DateTime createDate { get; set; }
    [Display(Name = "Ship Date")]
    public DateTime shipDate { get; set; }
    [Display(Name = "Shipment Cost"), Column(TypeName = "decimal(18,4)")]
    public decimal shipmentCost { get; set; }
    [Display(Name = "Insurance Cost"), Column(TypeName = "decimal(18,4)")]
    public decimal insuranceCost { get; set; }
    [Display(Name = "Tracking Number"), MaxLength(50)]
    public string trackingNumber { get; set; }
    [Display(Name = "Is Return Label")]
    public bool isReturnLabel { get; set; }
    [Display(Name = "Batch Number"), MaxLength(300)]
    public string batchNumber { get; set; }
    [Display(Name = "Carrier Code"), MaxLength(150)]
    public string carrierCode { get; set; }
    [Display(Name = "Service Code"), MaxLength(150)]
    public string serviceCode { get; set; }
    [Display(Name = "Package Code"), MaxLength(150)]
    public string packageCode { get; set; }
    public Confirmation? confirmation { get; set; } = Confirmation.none;
    [Display(Name = "Warehhouse Id")]
    public int? warehouseId { get; set; }
    [Display(Name = "Is Void")]
    public bool voided { get; set; } = false;
    [Display(Name = "Void Date")]
    public DateTime? voidDate { get; set; }
    [Display(Name = "Is Marketplace Notified")]
    public bool marketplaceNotified { get; set; }
    [Display(Name = "Notify Error Message"), MaxLength(300)]
    public string notifyErrorMessage { get; set; }
    [Display(Name = "Ship From")]
    public OrderShippingInfo shipFrom { get; set; } = new OrderShippingInfo();
    [Display(Name = "Ship To")]
    public OrderShippingInfo shipTo { get; set; } = new OrderShippingInfo();
    [Display(Name = "Weight")]
    public OrderWeight weight { get; set; } = new OrderWeight();
    [Display(Name = "Dimensions")]
    public OrderDimensions dimensions { get; set; } = new OrderDimensions();
    [Display(Name = "Advanced Options")]
    public OrderAdvancedOptions advancedOptions { get; set; } = new OrderAdvancedOptions();
    [Display(Name = "Shipment Items")]
    public List<OrderItem> shipmentItems { get; set; } = [];
    public string labelData { get; set; }
    [MaxLength(300)]
    public string formData { get; set; }
    public bool testLabel { get; set; } = false;

    [Display(Name = "Shipping Account Id")]
    public string ShippingAccountId { get; set; }
    [Display(Name = "Is Expedited?")]
    public bool IsExpedited { get; set; } = false;
    [MaxLength(100), Display(Name = "ShipEngine Shipment Id")]
    public string ShipEngineShipmentId { get; set; }
    //[Display(Name = "Is Shipping Received?")]
    //public bool IsShippingReceived { get; set; } = false;
    [Timestamp]
    public byte[] ERPTimestamp { get; internal set; }
    public string ERPModifyByUserId { get; internal set; }
    [Display(Name = "Modify Date")]
    public DateTime ERPModifyDate { get; set; }
}