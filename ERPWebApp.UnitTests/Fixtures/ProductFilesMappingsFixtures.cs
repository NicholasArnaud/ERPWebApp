using ERPWebApp.Models.Mappings;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductFilesMappingsFixtures
    {
        public static List<ProductFilesMappings> GetTestProductFilesMappings() =>
        [
            new ProductFilesMappings{
                ProductFilesMappingId = 1,
                ProductId = 1,
                FileId = 1,
                IsDetailedImage = true,
                IsThumbnail = false
            },
            new ProductFilesMappings{
                ProductFilesMappingId = 2,
                ProductId = 2,
                FileId = 2,
                IsDetailedImage = false,
                IsThumbnail = true
            }
        ];
    }
}