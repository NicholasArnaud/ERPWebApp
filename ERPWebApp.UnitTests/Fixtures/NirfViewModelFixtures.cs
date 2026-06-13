using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfViewModelFixtures
    {
        public static NirfViewModel GetTestData() => new()
        {
            NirfForms = NirfFormFixtures.GetTestList().First(),
            NirfProductMapping = NirfProductMappingFixtures.GetTestList().First(),
            NirfForecastings = NirfForecastingFixtures.GetTestList().First(),
            NirfInventories = NirfInventoryFixtures.GetTestList().First(),
            NirfPackagings = NirfPackagingFixtures.GetTestList().First(),
            NirfShippings = NirfShippingFixtures.GetTestList().First(),
            NirfParameters = NirfParametersFixtures.GetTestList().First(),
            NirfVendorMapping = NirfVendorMappingFixtures.GetTestList().First(),
            NirfImageMapping = NirfImageMappingFixtures.GetTestList(),
            ShippingProviders = ShippingProviderFixtures.GetTestList(),
            Vendor = VendorFixtures.GetTestList().First(),
            NirfShippingProvider = NirfShippingProdivderFixtures.GetTestList()
        };
    }
}