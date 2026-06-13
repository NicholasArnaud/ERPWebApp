using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductFilesMappingsService : Service<ProductFilesMappings>, IProductFilesMappingsService
    {
        IUnitOfWork _unitOfWork;
        public ProductFilesMappingsService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}