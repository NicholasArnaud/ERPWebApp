using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class PurchaseOrderFilesMappingService : Service<PurchaseOrderFilesMapping>, IPurchaseOrderFilesMappingService
    {
        IUnitOfWork _unitOfWork;
        public PurchaseOrderFilesMappingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}