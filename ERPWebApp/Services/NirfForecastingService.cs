using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
using System.Linq.Expressions;
namespace ERPWebApp.Services
{
    public class NirfForecastingService : INirfForecastingService
    {
        IUnitOfWork _unitOfWork;
        public NirfForecastingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NirfForecasting> AddAsync(NirfForecasting entity)
        {
            var result = await _unitOfWork.NirfForecasting.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public NirfForecasting Get(Expression<Func<NirfForecasting, bool>> expression)
        {
            return _unitOfWork.NirfForecasting.FilterOne(expression);
        }

        public async Task<NirfForecasting> GetAsync(Expression<Func<NirfForecasting, bool>> expression)
        {
            return await _unitOfWork.NirfForecasting.FilterOneAsync(expression);
        }

        public async Task<int> RemoveAsync(int id)
        {
            await _unitOfWork.NirfForecasting.DeleteAsync(id);
            return await _unitOfWork.SaveChangesAsync();
        }
    }
}