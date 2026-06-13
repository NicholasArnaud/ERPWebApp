using ERPWebApp.Models.NirfForms;
using System.Linq.Expressions;
namespace ERPWebApp.Services.IServices
{
    public interface INirfForecastingService
    {
        NirfForecasting Get(Expression<Func<NirfForecasting, bool>> expression);
        Task<NirfForecasting> GetAsync(Expression<Func<NirfForecasting, bool>> expression);
        Task<int> RemoveAsync(int id);
        Task<NirfForecasting> AddAsync(NirfForecasting entity);
    }
}