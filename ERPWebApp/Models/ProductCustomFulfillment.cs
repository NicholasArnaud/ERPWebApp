using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class ProductCustomFulfillment
    {
        [Key]
        public int ProductCustomFulfillmentId { get; set; }

        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Inventory.Product Product { get; set; }
        [Display(Name = "Bundle")]
        public int? BundleId { get; set; }

        [ForeignKey("BundleId")]
        public virtual Inventory.Bundle Bundle{ get; set; }

        [Display(Name = "Ship Station Store")]
        [Required]
        public int ShipStationStoreId { get; set; }

        [ForeignKey("ShipStationStoreId")]
        public virtual ShipStationStore ShipStationStore { get; set; }

        [Display(Name = "Custom Fulfillment Cost")]
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal CustomFulfillmentCost { get; set; }

        [Display(Name = "Effective Date")]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}