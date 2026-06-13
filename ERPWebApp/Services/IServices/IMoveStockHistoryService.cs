using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Services.IServices
{
    public interface IMoveStockHistoryService : IService<MoveStockHistory>
    {
        Task<MoveStock> MoveStock(MoveStock moveStock);
        Task<MoveStock> AddStock(MoveStock addStock);
        Task<MoveStock> RemoveStock(MoveStock moveStock);
        Task<(IEnumerable<StockMovementHistory>, int)> GetStockMovementHistoryAsync(SearchParameters search, bool? isExternal, string sku);
    }
}