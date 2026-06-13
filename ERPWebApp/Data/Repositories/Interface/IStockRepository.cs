using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.Data.SqlClient;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IStockRepository : IRepository<Stock>
    {
        IQueryable<StockProductContainer> GetStockProductContainersBySiteId(int siteId);

        List<Report> StockHistoryReport_Old(int locationId, DateTime selectedDate);
        List<Report> GetOnHandReport(int siteId);
        List<Report> GetStockHistoryReport(int siteId, DateTime selectedDate);
        ReportMetaData GetStockHistoryReport(
            string procedure,
            SqlParameter[] parameters,
            int timeout = 0
        );
        Task<List<Stock>> GetAllStocksWithProductAndLocationAsync();
        Task<(List<Stock>, int)> GetStockToCountAsync(int siteId, CycleCountFrequency frequency, SearchParameters search, bool? isStarted);
    }
}