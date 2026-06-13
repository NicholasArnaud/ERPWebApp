using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfForecastingRepository : Repository<NirfForecasting>, INirfForecastingRepository
    {
        public NirfForecastingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}