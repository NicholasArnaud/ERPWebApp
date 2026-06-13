using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfImageMappingRepository : Repository<NirfImageMapping>, INirfImageMappingRepository
    {
        public NirfImageMappingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}