using ERPWebApp.Models.PurchaseOrders;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Mappings
{
    public class ShippingMethodProviderMapping
    {
        [Key]
        public int ShippingMethodProviderMappingId { get; set; }
        [Display(Name = "ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [Display(Name = "ShippingMethodId")]
        public virtual ShippingMethod ShippingMethod { get; set; }
        [Display(Name = "ShippingProvider")]
        public int ShippingProviderId { get; set; }
        [Display(Name = "ShippingProviderId")]
        public virtual ShippingProvider ShippingProvider { get; set; }
    }
}
