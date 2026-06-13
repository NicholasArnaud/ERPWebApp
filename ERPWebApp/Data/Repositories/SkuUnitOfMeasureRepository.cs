using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;

namespace ERPWebApp.Data.Repositories
{
    public class SkuUnitOfMeasureRepository : Repository<SkuUnitOfMeasure>, ISkuUnitOfMeasureRepository
    {
        public SkuUnitOfMeasureRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}