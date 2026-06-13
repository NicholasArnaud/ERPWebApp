using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SkuColorService : Service<SkuColor>, ISkuColorService
    {
        IUnitOfWork _unitOfWork;
        public SkuColorService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}