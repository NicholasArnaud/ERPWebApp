using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ShippingMethodService : Service<ShippingMethod>, IShippingMethodService
    {
        IUnitOfWork _unitOfWork;
        public ShippingMethodService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}