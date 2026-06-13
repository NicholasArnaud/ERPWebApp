using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class SiteRepository : Repository<Site>, ISiteRepository
    {
        public SiteRepository(ApplicationDbContext context) : base(context)
        {
        }

    }
}