using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Services
{
    public class OrderTagService : Service<OrderTag>, IOrderTagService
    {
        IUnitOfWork _unitOfWork;
        public OrderTagService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}