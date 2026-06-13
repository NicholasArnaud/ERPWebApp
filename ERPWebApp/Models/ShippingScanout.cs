using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Models
{
    [Index(nameof(ScannedTrackingNumber), IsUnique = true)]
    public class ShippingScanout
    {
        [Key]
        public int ShippingScanoutId { get; set; }
        [Display(Name = "Order Shipment")]
        public int? OrderShipmentId { get; set; }
        [ForeignKey("OrderShipmentId")]
        public virtual OrderShipment OrderShipment { get; set; }
        [Display(Name = "Order Fulfillment")]
        public int? OrderFulfillmentId {get;set;}
        [ForeignKey("OrderFulfillmentId")]
        public virtual OrderFulfillment OrderFulfillment { get; set; }
        [Required]
        [Display(Name = "Tracking Number"), MaxLength(45), MinLength(10)]
        public string ScannedTrackingNumber { get; set; } = "";
        [Display(Name = "Create Date")]
        public DateTime CreateDate { get; set; }
        [Display(Name = "Created By"), MaxLength(50)]
        public string CreatedBy { get; set; }
        [Display(Name = "Is Valid Tracking Number")]
        public bool IsValidTrackingNumber { get; set; }
        [Display(Name ="Trailer Number"), MinLength(5),MaxLength(10)]
        public string TrailerNumber { get; set; }
        [Display(Name = "Webhook Batch")]
        public int? WebhookBatchId { get; set; }
        [ForeignKey("WebhookBatchId")]
        public virtual WebHookBatch WebhookBatch { get; set; }
    }
}
