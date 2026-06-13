using ERPWebApp.Models.Inventory;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
        public class ProductVendorMapping
        {
                [Key]
                public int ProductVendorMappingId { get; set; }

                [Display(Name = "Product")]
                [Required]
                public int ProductId { get; set; }

                [ForeignKey("ProductId")]
                public virtual Product Product { get; set; }

                [Display(Name = "Vendor")]
                [Required]
                public int VendorId { get; set; }

                [ForeignKey("VendorId")]
                public virtual Vendor Vendor { get; set; }

                [Display(Name = "Primary Vendor")]
                public bool isPrimaryVendor { get; set; }

                [Display(Name = "Cost")]
                [DataType(DataType.Currency)]
                [Column(TypeName = "decimal(16,4)")]
                public decimal Cost { get; set; }

                [Display(Name = "Lead Time")]
                public int LeadTime { get; set; }

                [Display(Name = "Vendor Sku")]
                [Required]
                public string VendorSku { get; set; }

                [Display(Name = "Active")]
                [DefaultValue(true)]
                public bool IsActive { get; set; } = true;

                [Display(Name = "Notes")]
                public string Notes { get; set; }

                [Display(Name = "Minimum Order Quantity")]
                [DefaultValue(1)]
                public int MOQ { get; set; }

                [Display(Name = "Order Multiples")]
                [DefaultValue(1)]
                public int OrderMultiples { get; set; }

                [Display(Name = "Unit of Measure")]
                [StringLength(10)]
                public string UnitofMeasure { get; set; }

                [Display(Name = "Terms")]
                [StringLength(10)]
                public string Term { get; set; }

                [Display(Name = "Raw Material")]
                public bool IsRawMaterial { get; set; }

                [NotMapped]
                public string Permission { get; set; }
        }
}