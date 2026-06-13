using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.PurchaseOrders
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }
        [Display(Name = "ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [Display(Name = "ShippingMethodId")]
        public virtual ShippingMethod ShippingMethod { get; set; }
        [Display(Name = "ShippingProvider")]
        public int ShippingProviderId { get; set; }
        [Display(Name = "ShippingProviderId")]
        public virtual ShippingProvider ShippingProvider { get; set; }
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        [Display(Name = "VendorId")]
        public virtual Vendor Vendor { get; set; }
        [StringLength(60, MinimumLength = 5)]
        [Required]
        [Display(Name = "Purchase Order Number")]
        public string PurchaseOrderNumber { get; set; }
        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }
        [Required]
        [Display(Name = "Estimated Date")]
        public DateTime EstimatedDate { get; set; }
        [Required]
        [Display(Name = "Status")]
        public Status POStatus { get; set; }
        [Required]
        [Display(Name = "Reference Number")]
        [StringLength(60)]
        public string ReferenceNumber { get; set; }
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }
        [Required]
        [Display(Name = "Shipping Cost")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ShippingCost { get; set; }
        [Required]
        [Display(Name = "Grand Total")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrandTotal { get; set; }
        [Required]
        [Display(Name = "Discount(%)")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Discount { get; set; }
        [Required]
        [Display(Name = "Tax(%)")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ShippingTax { get; set; }
        [Required]
        [Display(Name = "Other Cost")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal OtherCost { get; set; }
        public bool IsActive { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }

        [NotMapped]
        public int Attachments { get; set; }
        [NotMapped]
        public int totalQty { get; set; }
        [NotMapped]
        public decimal TotalProdutsCost { get; set; }
        public virtual List<PurchaseOrderFilesMapping> OrderFiles { get; set; }
        [NotMapped]
        public string Permission { get; set; }
    }
    public enum Status
    {
        Draft = 0,
        OpenIssued = 1,
        InProgress = 2,
        Close = 3,
        Cancelled = 4,
        FullyReceived = 5
    }
}
