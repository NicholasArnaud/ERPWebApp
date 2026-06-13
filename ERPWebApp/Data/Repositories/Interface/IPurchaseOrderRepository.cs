using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
    {
        Task<List<PurchaseOrder>> GetActivePurchaseOrdersByProductAsync(int productId);
        Task<int> GetProductOnOrderQtyAsync(int productId);
    }
}