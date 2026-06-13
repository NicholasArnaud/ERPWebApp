using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SkuCategoryService : Service<SkuCategory>, ISkuCategoryService
    {
        IUnitOfWork _unitOfWork;
        public SkuCategoryService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}