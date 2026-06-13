using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.PurchaseOrders;

public class MiscProduct
{
    [Key]
    public int MiscProductId { get; set; }
    [ForeignKey("PurchaseOrderId")]
    public int PurchaseOrderId { get; set; }
    public virtual PurchaseOrder PurchaseOrder { get; set; }
    [StringLength(60, MinimumLength = 3)]
    public string Sku { get; set; }
    [MaxLength(400)]
    public string Description { get; set; }
    [Display(Name = "Product Cost")]
    [Column(TypeName = "decimal(16,4)")]
    public decimal ProductCost { get; set; }
    [Display(Name = "Custom Cost")]
    [Column(TypeName = "decimal(16,4)")]
    public decimal CustomCost { get; set; }
    [Display(Name = "Custom Cost")]
    [Column(TypeName = "decimal(16,4)"),Range(0,1)]
    public decimal DiscountPercentage { get; set; }
    public int Quantity { get; set; }
    [Display(Name = "Total Cost")]
    [Column(TypeName = "decimal(16,4)")]
    public decimal TotalCost { get; set; }
    [Display(Name = "Expected Date")]
    public DateTime ExpectedDate { get; set; } = DateTime.Now.Date;
    public bool IsActive { get; set; } = true;
    [Display(Name = "Modify Date")]
    public DateTime ModifyDate { get; set; } = DateTime.Now.Date;
    [Display(Name = "Modify By User")]
    [MaxLength(50)]
    public string ModifyByUser { get; set; }
}
