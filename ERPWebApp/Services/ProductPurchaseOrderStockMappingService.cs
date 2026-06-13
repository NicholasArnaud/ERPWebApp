using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductPurchaseOrderStockMappingService : Service<ProductPurchaseOrderStockMapping>, IProductPurchaseOrderStockMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductPurchaseOrderStockMappingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}