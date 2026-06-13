using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories
{
    public class ProductPurchaseOrderStockMappingRepository : Repository<ProductPurchaseOrderStockMapping>, IProductPurchaseOrderStockMappingRepository
    {
        public ProductPurchaseOrderStockMappingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}