using ERPWebApp.Models.NirfForms;
using System.Linq.Expressions;
namespace ERPWebApp.Services.IServices
{
    public interface INirfPackagingService
    {
        NirfPackaging Get(Expression<Func<NirfPackaging, bool>> expression);
        Task<NirfPackaging> GetAsync(Expression<Func<NirfPackaging, bool>> expression);
        Task<int> RemoveAsync(int id);
        Task<NirfPackaging> AddAsync(NirfPackaging entity);
    }
}