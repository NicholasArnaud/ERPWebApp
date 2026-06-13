using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfShippingProdivderFixtures
    {
        public static List<NirfShippingProdivder> GetTestList() => [
            new NirfShippingProdivder
            {
                ShippingProvider = ShippingProviderFixtures.GetTestList().First(),
                ShippingWeight = 10.5m,
                ShippingSize = "Medium",
                ShippingCost = 25.99m
            }
         ];
    }
}