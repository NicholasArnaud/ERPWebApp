using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IMiscProductRepository: IRepository<MiscProduct>
    {
        Task<List<MiscProduct>> GetMiscProductsByPurchaseOrderId(int purchaseOrderId);
        Task DeleteMiscProductAsync(int id, string modifiedByUser);
        Task UpdateMiscProducts(List<MiscProduct> miscProducts);
    }
}
