using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ShippingProviderFixtures
    {
        public static List<ShippingProvider> GetTestList() => [
            new ShippingProvider
            {
                ShippingProviderId = 1,
                ShippingProviderName = "FedEx",
                IsActive = true,
                ModifyDate = DateTime.Now,
                ModifyByUser = "John Smith"
            }
         ];
    }
}