using ERPWebApp.Models.Sellers;

namespace ERPWebApp.Data.Repositories.Interface
{

    public interface ISellerMarginRepository: IRepository<SellerMargins>
    {
        Task<List<SellerMargins>> GetSellerMarginsAsync();
        Task<List<SellerMargins>> GetSellerMarginsByDateRangeAsync(int? storeId, DateTime startDate, DateTime endDate);
    }


}
