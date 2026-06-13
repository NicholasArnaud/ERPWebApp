using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class ProductTagRepository : Repository<ProductTagsRegistry>, IProductTagRepository
    {
        public ProductTagRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void AssignProductTag(ProductTag productTag)
        {
            var isExists = _context.productTag.Any(x => x.ProductId == productTag.ProductId && x.TagId == productTag.TagId);
            if (!isExists) _context.productTag.Add(productTag);
        }

        public async Task AssignProductTagAsync(ProductTag productTag)
        {
            var isExists = _context.productTag.Any(x => x.ProductId == productTag.ProductId && x.TagId == productTag.TagId);
            if (!isExists) await _context.productTag.AddAsync(productTag);
        }

        public async Task UnAssignProductTagAsync(int productId, int tagId)
        {
            var productTag = await _context.productTag.FirstAsync(x => x.ProductId == productId && x.TagId == tagId);
            if (productTag != null)
                _context.productTag.Remove(productTag);
        }
    }
}