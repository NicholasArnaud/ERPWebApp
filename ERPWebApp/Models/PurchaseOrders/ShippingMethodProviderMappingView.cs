using ERPWebApp.Models.Mappings;
namespace ERPWebApp.Models.PurchaseOrders
{
    public class ShippingMethodProviderMappingView
    {
        public IEnumerable<ShippingMethodProviderMapping> ShippingMethodProviderMappings { get; set; }
        public ShippingMethodProviderMapping ShippingMethodProviderMapping { get; set; }

    }
}
