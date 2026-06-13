using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;

namespace ERPWebApp.Data.Repositories
{
    public class SkuCategoryRepository : Repository<SkuCategory>, ISkuCategoryRepository
    {
        public SkuCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}