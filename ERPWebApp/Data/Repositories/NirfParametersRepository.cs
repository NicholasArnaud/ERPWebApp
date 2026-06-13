using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfParametersRepository : Repository<NirfParameters>, INirfParametersRepository
    {
        public NirfParametersRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}