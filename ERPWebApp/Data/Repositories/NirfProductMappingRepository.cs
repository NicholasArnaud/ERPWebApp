using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.Data.Repositories
{
    public class NirfProductMappingRepository : Repository<NirfProductMapping>, INirfProductMappingRepository
    {
        public NirfProductMappingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}