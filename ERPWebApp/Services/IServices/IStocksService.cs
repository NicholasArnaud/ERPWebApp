using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IStocksService : IService<Stock>
    {
        decimal SetupVolumeTally(int ChosenSite);
        IQueryable<Product> GetProducts(string searchValue,
            bool? zeroQtyStock,
            int? subCategoryId,
            int? siteId,
            int? departmentId,
            int? productTagId,
            int? vendorId,
            string sortColumn,
            string sortColumnDirection,
            int? storeId
        );
        IQueryable<Stock> GetProductsStock(
            string searchValue,
            string sku,
            string sortColumn,
            string sortColumnDirection,
            string role,
            int? storeId
        );
        Task<bool> DeleteConfirmed(int id);
        List<Report> StockHistoryReport_Old(int locationId, DateTime selectedDate);
        List<Report> GetOnHandBySiteFilter(int SiteId);
        List<Report> GetStockHistoryReport(int siteId, DateTime selectedDate);
        ReportMetaData GetStockHistoryReport(DateTime StartDate, DateTime EndDate, int ProductId, int SubCategoryId, int DepartmentId, int ShipStationStoreId, int PageNo, int PageSize);
    }
}