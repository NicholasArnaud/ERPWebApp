using ERPWebApp.Models.Sellers;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class SellerMarginService : Service<SellerMargins>, ISellerMarginService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SellerMarginService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SellerMargins>> GetSellerMarginsAsync()
        {
            return await _unitOfWork.SellerMargins.GetSellerMarginsAsync();
        }

        public async Task<List<SellerMargins>> GetSellerMarginsByDateRangeAsync(int? storeId, DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.SellerMargins.GetSellerMarginsByDateRangeAsync(storeId, startDate, endDate);
        }
    }

}
