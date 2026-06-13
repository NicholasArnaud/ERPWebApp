using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;

namespace ERPWebApp.Data.Repositories
{
    public class SkuColorRepository : Repository<SkuColor>, ISkuColorRepository
    {
        public SkuColorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}