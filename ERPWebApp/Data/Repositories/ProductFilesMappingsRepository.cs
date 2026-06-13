using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories
{
    public class ProductFilesMappingsRepository : Repository<ProductFilesMappings>, IProductFilesMappingsRepository
    {
        public ProductFilesMappingsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}