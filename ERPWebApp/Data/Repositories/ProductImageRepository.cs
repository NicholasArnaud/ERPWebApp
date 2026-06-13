using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}