using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class IntegrationRepository : Repository<Integration>, IIntegrationRepository
    {
        public IntegrationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
