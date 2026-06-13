using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductTagsRegistryFixtures
    {
        public static List<ProductTagsRegistry> GetTestProductTags() =>
        [
            new ProductTagsRegistry { TagId = 1, Description = "Tag 1", Color = "#fff" },
            new ProductTagsRegistry { TagId = 2, Description = "Tag 2", Color = "#fff" },
            new ProductTagsRegistry { TagId = 3, Description = "Tag 3", Color = "#fff" },
            new ProductTagsRegistry { TagId = 4, Description = "Tag 4", Color = "#fff" },
        ];
    }
}