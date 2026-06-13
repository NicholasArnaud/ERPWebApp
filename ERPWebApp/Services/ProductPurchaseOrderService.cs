using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductPurchaseOrderService : Service<ProductPurchaseOrder>, IProductPurchaseOrderService
    {
        IUnitOfWork _unitOfWork;

        public ProductPurchaseOrderService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}