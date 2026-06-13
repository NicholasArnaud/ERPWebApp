using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IProductTagService : IService<ProductTagsRegistry>
    {
        Task AssignProductTagRangeAsync(List<ProductTagsRegistry> tags, int productId);
        void AssignProductTagRange(List<ProductTagsRegistry> tags, int productId);
        Task AssignProductTagAsync(ProductTagsRegistry tag, int productId);
        void AssignProductTag(ProductTagsRegistry tag, int productId);

        Task UnAssignProductTagAsync(int productId, int tagId);
    }
}