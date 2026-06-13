using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductTagFixtures
    {
        public static List<ProductTag> GetTestProductTags() {
            return
            [
                new() { ProductId = 1, TagId = 1 },
                new() { ProductId = 2, TagId = 2 },
                new() { ProductId = 3, TagId = 3 }
            ];
        }
    }
}
