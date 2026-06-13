using ERPWebApp.Models.Mappings;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductVendorMappingFixtures
    {
        public static List<ProductVendorMapping> GetTestList() =>
         [
            new ProductVendorMapping()
            {
                ProductVendorMappingId = 1,
                ProductId = 1001,
                VendorId = 2001,
                isPrimaryVendor = true,
                Cost = 125.50m,
                LeadTime = 5,
                VendorSku = "VEN1001",
                IsActive = true,
                Product= ProductFixtures.GetTestProducts().First(),
                Vendor= VendorFixtures.GetTestList().First(),
            },

            new ProductVendorMapping()
            {
                ProductVendorMappingId = 2,
                ProductId = 1001,
                VendorId = 2002,
                isPrimaryVendor = false,
                Cost = 130.00m,
                LeadTime = 3,
                VendorSku = "VEN1002",
                IsActive = false,
            },

            new ProductVendorMapping()
            {
                ProductVendorMappingId = 3,
                ProductId = 1002,
                VendorId = 2002,
                isPrimaryVendor = true,
                Cost = 50.00m,
                LeadTime = 2,
                VendorSku = "VEN1003",
                IsActive=true
            },

            new ProductVendorMapping()
            {
                ProductVendorMappingId = 4,
                ProductId = 1,
                VendorId = 2001,
                isPrimaryVendor = true,
                Cost = 50.00m,
                LeadTime = 2,
                VendorSku = "VEN1003",
                IsActive = true
            },
         ];
    }
}