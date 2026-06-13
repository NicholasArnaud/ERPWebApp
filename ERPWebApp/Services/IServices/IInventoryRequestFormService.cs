using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IInventoryRequestFormService : IService<InventoryRequestForm>
    {
        Task CloseInventoryRequestAsync(
            InventoryRequestForm entity,
            Stock fromStock,
            Stock toStock,
            MoveStockHistory stockHistory
        );
    }
}