using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using System.Linq.Expressions;

namespace ERPWebApp.Services
{
    public class BundleService : Service<Bundle>, IBundleService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BundleService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Bundle> GetBundleWithItemsAsync(int id)
        {
            Expression<Func<Bundle, object>>[] includes = { b => b.BundleItems };
            return await _unitOfWork.Bundles.FilterOneAsync(b => b.BundleId == id, includes);
        }
    }
}
