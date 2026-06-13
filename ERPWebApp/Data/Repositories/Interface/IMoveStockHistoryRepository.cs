using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IMoveStockHistoryRepository : IRepository<MoveStockHistory>
    {
        Task<IEnumerable<MoveStockHistory>> GetStockHistoriesCustomSelectionAsync(int id);
        Task<(IEnumerable<StockMovementHistory>, int)> GetStockMovementHistoryAsync(SearchParameters search, bool? isExternal, string sku);
    }
}