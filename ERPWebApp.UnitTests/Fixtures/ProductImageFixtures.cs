using ERPWebApp.Models.Mappings;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class ProductImageFixtures
    {
        public static List<ProductImage> GetProductImageFixtures() =>
         [
            new ProductImage(){
                ProductImageId = 1,
                ProductId = 1,
                FileId = 1,
                FileUrl = "",
                ThumbnailUrl = "",
                IsDefault = true
            },
            new ProductImage(){
                ProductImageId = 2,
                ProductId = 2,
                FileId = 2,
                FileUrl = "",
                ThumbnailUrl = "",
                IsDefault = true
            }
         ];
    }
}