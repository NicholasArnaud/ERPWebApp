using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IProductTagRepository : IRepository<ProductTagsRegistry>
    {
        Task AssignProductTagAsync(ProductTag productTag);
        void AssignProductTag(ProductTag productTag);
        Task UnAssignProductTagAsync(int productId, int tagId);
    }
}