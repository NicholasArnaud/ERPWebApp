using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace ERPWebApp.Models.PurchaseOrders
{
    [Index(nameof(ShippingProviderName), IsUnique = true)]
    public class ShippingProvider
    {
        [Key]
        public int ShippingProviderId { get; set; }
        [Required]
        [Display(Name = "Shipping Provider")]
        public string ShippingProviderName { get; set; }
        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }
    }
}
