using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
namespace ERPWebApp.Services.IServices
{
    public interface ICycleCountService : IService<CycleCount>
    {
        Task<(List<Stock>, int)> GetStockToCountAsync(
            int siteId,
            SearchParameters search,
            bool? isStarted
        );
        List<Report> GetCycleCountReport(DateTime startDate, DateTime endDate, int locationId);

        Task<Stock> EditCycleCount (CycleCount cycleCount, string verifiedBy);
        Task StartCycleCountAsync(List<int> stockIds);
        Task StartCycleCountAsync(int stockId);
        Task StartCycleCountForSiteAsync(int siteId);
    }
}