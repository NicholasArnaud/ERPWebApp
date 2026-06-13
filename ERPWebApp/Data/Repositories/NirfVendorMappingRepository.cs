using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfVendorMappingRepository : Repository<NirfVendorMapping>, INirfVendorMappingRepository
    {
        public NirfVendorMappingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}