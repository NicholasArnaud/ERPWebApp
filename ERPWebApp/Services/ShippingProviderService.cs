using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class ShippingProviderService : Service<ShippingProvider>, IShippingProviderService
    {
        IUnitOfWork _unitOfWork;
        public ShippingProviderService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}