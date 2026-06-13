using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfShippingRepository : Repository<NirfShipping>, INirfShippingRepository
    {
        public NirfShippingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}