using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductPurchaseOrderFixtures
    {
        public static List<ProductPurchaseOrder> GetTestList() =>
         [
            new ProductPurchaseOrder
            {
                ProductPurchaseOrderId = 1,
                PurchaseOrderId = 1001,
                PurchaseOrder = new PurchaseOrder { PurchaseOrderId = 1001 },
                ProductVendorMappingId = 123,
                ProductVendorMapping = new ProductVendorMapping { ProductVendorMappingId = 123 },
                CustomCost = 12.3456M,
                AverageCost = 10.1234M,
                TotalRecieved = 50,
                TotalOrdered = 100,
                ModifyDate = DateTime.Now,
                ModifyByUser = "John Doe"
            },
            new ProductPurchaseOrder
            {
                ProductPurchaseOrderId = 2,
                PurchaseOrderId = 1001,
                PurchaseOrder = new PurchaseOrder { PurchaseOrderId = 1001 },
                ProductVendorMappingId = 123,
                ProductVendorMapping = new ProductVendorMapping { ProductVendorMappingId = 123 },
                CustomCost = 12.3456M,
                AverageCost = 10.1234M,
                TotalRecieved = 50,
                TotalOrdered = 100,
                ModifyDate = DateTime.Now,
                ModifyByUser = "John Doe"
            }
         ];
    }
}