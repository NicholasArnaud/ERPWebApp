using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Models.Orders;

[Index(nameof(orderItemId), IsUnique = false)]
public class OrderItem
{
    [Key]
    public int ERPOrderItemId { get; set; }
    public int ERPOrderId { get; set; }
    [ForeignKey(nameof(ERPOrderId))]
    public Order Order { get; set; }
    public OrderShipment OrderShipment { get; set; }
    /// <summary>
    /// Shipstation system generated identifier for the OrderItem. This is a read-only field.
    /// </summary>
    [Display(Name = "Order Item Id")]
    public long orderItemId { get; set; }
    /// <summary>
    /// An identifier for the OrderItem in the originating system.
    /// </summary>
    [Display(Name = "Line Item Key")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string lineItemKey { get; set; }
    [Display(Name = "Sku"), MaxLength(60)]
    public string sku { get; set; }
    /// <summary>
    /// The name of the product associated with this line item. Cannot be null
    /// </summary>
    [Required]
    [Display(Name = "Name"), MaxLength(200)]
    public string name { get; set; }
    [Display(Name = "Image URL"), MaxLength(300)]
    public string imageUrl { get; set; }
    /// <summary>
    /// The weight of a single item.
    /// </summary>
    [Display(Name = "Weight")]
    public OrderWeight weight { get; set; }
    [Display(Name = "Quantity")]
    public int quantity { get; set; }
    /// <summary>
    /// The sell price of a single item specified by the order source.
    /// </summary>
    [Display(Name = "Unit Price"), Column(TypeName = "decimal(18,4)")]
    public decimal unitPrice { get; set; }
    /// <summary>
    /// The tax price of a single item specified by the order source.
    /// </summary>
    [Display(Name = "Tax Amount"), Column(TypeName = "decimal(18,4)")]
    public decimal? taxAmount { get; set; } = 0.0m;
    /// <summary>
    /// The shipping amount or price of a single item specified by the order source.
    /// </summary>
    [Display(Name = "Shipping Amount"), Column(TypeName = "decimal(18,4)")]
    public decimal? shippingAmount { get; set; } = 0.0m;
    [Display(Name = "Warehouse Location"), MaxLength(50)]
    public string warehouseLocation { get; set; }
    public List<OrderItemOption> options { get; set; } = [];
    /// <summary>
    /// The identifier for the Product Resource associated with this OrderItem.
    /// </summary>
    [Display(Name = "Product Id")]
    public long? productId { get; set; }
    public int? ERPProductId { get; set; }
    [ForeignKey(nameof(ERPProductId))]
    public virtual Product Product { get; set; }
    [Display(Name = "Bundle")]
    public int? ERPBundleId { get; set; }
    [ForeignKey(nameof(ERPBundleId))]
    public virtual Bundle Bundle { get; set; }
    /// <summary>
    /// The fulfillment SKU associated with this OrderItem if the fulfillment provider requires an identifier other then the SKU.
    /// </summary>
    [Display(Name = "Fulfillment Sku"), MaxLength(150)]
    public string fulfillmentSku { get; set; }
    /// <summary>
    /// Indicates that the OrderItem is a non-physical adjustment to the order (e.g. a discount or promotional code)
    /// </summary>
    [Display(Name = "Is Adjustment")]
    public bool adjustment { get; set; }
    /// <summary>
    /// The Universal Product Code associated with this OrderItem.
    /// </summary>
    [Display(Name = "UPC"), MaxLength(150)]
    public string upc { get; set; }
    /// <summary>
    /// The timestamp the orderItem was created in ShipStation's database. Read-Only.
    /// </summary>
    [Display(Name = "Date Created")]
    public DateTime createDate { get; set; }
    /// <summary>
    /// The timestamp the orderItem was modified in ShipStation. modifyDate will equal createDate until a modification is made. Read-Only.
    /// </summary>
    [Display(Name = "Date Last Modified")]
    public DateTime modifyDate { get; set; }

    [Timestamp]
    public byte[] ERPTimestamp { get; internal set; }

    public string ERPModifyByUserId { get; internal set; }

    public class OrderItemOption
    {
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string value { get; set; }
    }
}
