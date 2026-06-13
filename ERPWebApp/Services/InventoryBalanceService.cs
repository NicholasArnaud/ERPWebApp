using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class InventoryBalanceService : Service<InventoryBalance>, IInventoryBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryBalanceService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Report> GetReport(int ProductId)
        {
            return _unitOfWork.InventoryBalance.GetReport(ProductId);
        }

    }
}
