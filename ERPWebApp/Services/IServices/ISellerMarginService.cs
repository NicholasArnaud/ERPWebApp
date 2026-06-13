using ERPWebApp.Models.Sellers;
namespace ERPWebApp.Services.IServices
{

    public interface ISellerMarginService: IService<SellerMargins>
    {
        Task<List<SellerMargins>> GetSellerMarginsAsync();
        Task<List<SellerMargins>> GetSellerMarginsByDateRangeAsync(int? storeId, DateTime startDate, DateTime endDate);
    }

}
