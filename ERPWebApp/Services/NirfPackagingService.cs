using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
using System.Linq.Expressions;

namespace ERPWebApp.Services
{
    public class NirfPackagingService : INirfPackagingService
    {
        IUnitOfWork _unitOfWork;
        public NirfPackagingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NirfPackaging> AddAsync(NirfPackaging entity)
        {
            var result = await _unitOfWork.NirfPackaging.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public NirfPackaging Get(Expression<Func<NirfPackaging, bool>> expression)
        {
            return _unitOfWork.NirfPackaging.FilterOne(expression);
        }

        public async Task<NirfPackaging> GetAsync(Expression<Func<NirfPackaging, bool>> expression)
        {
            return await _unitOfWork.NirfPackaging.FilterOneAsync(expression);
        }

        public async Task<int> RemoveAsync(int id)
        {
            await _unitOfWork.NirfPackaging.DeleteAsync(id);
            return await _unitOfWork.SaveChangesAsync();
        }
    }
}