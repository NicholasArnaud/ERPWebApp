using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace ERPWebApp.Models.PurchaseOrders
{
    public class ShippingMethod
    {
        [Key]
        public int ShippingMethodId { get; set; }
        [Display(Name = "ShippingProvider")]
        public int ShippingProviderId { get; set; }
        [Display(Name = "ShippingProviderId")]
        public virtual ShippingProvider ShippingProvider { get; set; }
        [Required]
        [Display(Name = "Shipping Method")]
        public string ShippingMethodName { get; set; }
        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }
    }
}
