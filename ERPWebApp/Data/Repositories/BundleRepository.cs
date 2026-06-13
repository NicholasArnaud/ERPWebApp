using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class BundleRepository : Repository<Bundle>, IBundleRepository
    {
        public BundleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
