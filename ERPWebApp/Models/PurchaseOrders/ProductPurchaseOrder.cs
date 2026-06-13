using ERPWebApp.Models.Mappings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.PurchaseOrders
{
    public class ProductPurchaseOrder
    {
        [Key]
        public int ProductPurchaseOrderId { get; set; }
        [Display(Name = "PurchaseOrder")]
        public int PurchaseOrderId { get; set; }
        [Display(Name = "PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        [Display(Name = "ProductVendorMapping")]
        public int ProductVendorMappingId { get; set; }
        public virtual ProductVendorMapping ProductVendorMapping { get; set; }
        [Required]
        [Display(Name = "Custom Cost")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CustomCost { get; set; }
        [Required]
        [Display(Name = "Average Cost")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal AverageCost { get; set; }
        [Required]
        [Display(Name = "Total Quantity Recieved")]
        public int TotalRecieved { get; set; }
        [Required]
        [Display(Name = "Total Quantity Ordered")]
        public int TotalOrdered { get; set; }
        [Display(Name = "Discount %")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountPercentage { get; set; }
        [Display(Name ="Discount Amount")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountAmount { get; set;}
        [Display(Name = "Total Product Cost")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalProductCost { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }
        [Display(Name = "Expected Delivery Date")]
        public DateTime ExpectedDate { get; set; }
    }
}
