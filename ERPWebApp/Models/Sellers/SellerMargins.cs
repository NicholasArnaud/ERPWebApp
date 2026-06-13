using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ERPWebApp.Models.Sellers
{
    public class SellerMargins
    {
        [Key]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }
        [Display(Name = "Ship Date")]
        public DateTime ShipDate { get; set; }
        [Display(Name = "Store Name")]
        public string StoreName { get; set; }
        [Display(Name = "Service Code")]
        public string ServiceCode { get; set; }
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Store Items Cost")]
        public decimal StoreItemsCost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Customer Items Cost")]
        public decimal CustomerItemsCost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Shipping Cost")]
        public decimal ShippingCost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Shipment Cost")]
        public decimal ShipmentCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Store Cost With Etsy")]
        public decimal StoreCostWithEtsy { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Store Cost Diff Subfulfillment And Shipping")]
        public decimal StoreCostDiffSubfulfillmentAndShipping { get; set; }
        [NotMapped]
        [Display(Name = "Ship Date")]
        public string StringShipDate { get; set; }
    }
}
