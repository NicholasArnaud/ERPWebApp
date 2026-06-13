using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class BundleItemRepository : Repository<BundleItem>, IBundleItemRepository
    {
        public BundleItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
