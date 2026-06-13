using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class VendorFixtures
    {
        public static List<Vendor> GetTestList() => [
            new Vendor
            {
                VendorNumber = "V001",
                VendorName = "Acme Corporation",
                Notes = "Some notes about Acme Corporation",
                ContactName = "John Smith",
                PhoneNumber = "555-1234",
                BusinessEmail = "info@acmecorp.com",
                Fax = "555-5678",
                Website = "https://www.acmecorp.com",
                Address1 = "123 Main St",
                Address2 = "Suite 456",
                City = "Anytown",
                State = "CA",
                PostalCode = "12345",
                Country = "USA",
                LastModified = DateTime.Now,
                IsActive = true,
                IsExternal = false
            },
            new Vendor
            {
                VendorNumber = "V002",
                VendorName = "Globex Corporation",
                Notes = "Some notes about Globex Corporation",
                ContactName = "Jane Doe",
                PhoneNumber = "555-5678",
                BusinessEmail = "info@globexcorp.com",
                Fax = "555-1234",
                Website = "https://www.globexcorp.com",
                Address1 = "456 Main St",
                Address2 = "Suite 789",
                City = "Othertown",
                State = "NY",
                PostalCode = "67890",
                Country = "USA",
                LastModified = DateTime.Now,
                IsActive = true,
                IsExternal = false
            }
         ];
    }
}