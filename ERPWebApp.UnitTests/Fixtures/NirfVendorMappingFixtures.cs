using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfVendorMappingFixtures
    {
        public static List<NirfVendorMapping> GetTestList() => [
            new NirfVendorMapping
            {
                NirfVendorMappingId = 1,
                VendorId = 1001,
                Vendor = new Vendor { VendorId = 1001, VendorName = "ABC Inc.", VendorNumber = "1" },
                SignedBy = "John Smith",
                SignedOn = new DateTime(2022, 04, 23),
                AspUserId = "user123",
                NirfFormId = 101,
                NirfForm = NirfFormFixtures.GetTestList().First()
            },
            new NirfVendorMapping
            {
                NirfVendorMappingId = 2,
                VendorId = 2,
                Vendor = new Vendor { VendorId = 2, VendorName = "XYZ Inc", VendorNumber = "2" },
                SignedBy = "Jane Smith",
                SignedOn = new DateTime(2023, 4, 25),
                AspUserId = "janesmith456",
                NirfFormId = 2,
                NirfForm = NirfFormFixtures.GetTestList().First()
            }
        ];
    }
}