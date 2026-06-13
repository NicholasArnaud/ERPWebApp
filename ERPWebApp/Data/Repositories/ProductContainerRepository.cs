using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class ProductContainerRepository : Repository<ProductContainer>, IProductContainerRepository
    {
        public ProductContainerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}