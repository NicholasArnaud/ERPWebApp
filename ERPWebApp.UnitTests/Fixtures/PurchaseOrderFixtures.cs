using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class PurchaseOrderFixtures
    {
        public static List<PurchaseOrder> GetTestList() =>
        [
            new PurchaseOrder
            {
                PurchaseOrderId = 1,
                ShippingMethodId = 1,
                ShippingMethod = new ShippingMethod { ShippingMethodName = "Standard" },
                ShippingProviderId = 1,
                ShippingProvider = new ShippingProvider { ShippingProviderName = "UPS" },
                VendorId = 1,
                Vendor = new Vendor { VendorName = "ABC Company", VendorNumber = "1" },
                PurchaseOrderNumber = "PO-10001",
                OrderDate = DateTime.Parse("2023-04-09"),
                EstimatedDate = DateTime.Parse("2023-04-16"),
                POStatus = Status.Draft,
                ReferenceNumber = "REF-10001",
                Notes = "This is a test note",
                ShippingCost = 10.50m,
                GrandTotal = 100.00m,
                Discount = 5.00m,
                ShippingTax = 7.50m,
                OtherCost = 2.00m,
                IsActive = true,
                ModifyDate = DateTime.Parse("2023-04-09"),
                ModifyByUser = "John Doe",
                Attachments = 2,
                totalQty = 10
            },
            new PurchaseOrder
            {
                PurchaseOrderId = 2,
                ShippingMethodId = 1,
                ShippingMethod = new ShippingMethod { ShippingMethodName = "Standard" },
                ShippingProviderId = 1,
                ShippingProvider = new ShippingProvider { ShippingProviderName = "UPS" },
                VendorId = 1,
                Vendor = new Vendor { VendorName = "ABC Company", VendorNumber = "2" },
                PurchaseOrderNumber = "PO-10001",
                OrderDate = DateTime.Parse("2023-04-09"),
                EstimatedDate = DateTime.Parse("2023-04-16"),
                POStatus = Status.Draft,
                ReferenceNumber = "REF-10001",
                Notes = "This is a test note",
                ShippingCost = 10.50m,
                GrandTotal = 100.00m,
                Discount = 5.00m,
                ShippingTax = 7.50m,
                OtherCost = 2.00m,
                IsActive = true,
                ModifyDate = DateTime.Parse("2023-04-09"),
                ModifyByUser = "John Doe",
                Attachments = 2,
                totalQty = 10
            }
        ];
    }
}