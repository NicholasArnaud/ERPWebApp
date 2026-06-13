namespace ERPWebApp.Models.PurchaseOrders
{
    public class ShippingMethodProviderView
    {

        public IEnumerable<ShippingMethod> Methods { get; set; }
        public ShippingMethod Method { get; set; }
        public IEnumerable<ShippingProvider> Providers { get; set; }
        public ShippingProvider Provider { get; set; }

    }
}
