using ERPWebApp.Models.PurchaseOrders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfShippingProdivder
    {
        [Key]
        [Display(Name = "NirfShippingMappingId")]
        public int NirfShippingProviderId { get; set; }
        [Display(Name = "Shipping Provider")]
        public int ShippingProviderId { get; set; }
        [ForeignKey("ShippingProviderId")]
        public virtual ShippingProvider ShippingProvider { get; set; }
        [Display(Name = "Shipping Weight (lb)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingWeight { get; set; }
        [Display(Name = "Shipping Size")]
        [StringLength(50)]
        public string ShippingSize { get; set; }
        [Display(Name = "Shipping Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }
        [Display(Name = "Error Message")]
        [NotMapped]
        public string ErrorMessage { get; set; }
    }
}