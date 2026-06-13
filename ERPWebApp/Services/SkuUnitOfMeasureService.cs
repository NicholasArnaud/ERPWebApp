using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SkuUnitOfMeasureService : Service<SkuUnitOfMeasure>, ISkuUnitOfMeasureService
    {
        IUnitOfWork _unitOfWork;
        public SkuUnitOfMeasureService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}