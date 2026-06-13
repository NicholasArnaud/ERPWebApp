using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;
using QuestPDF.Fluent;
using static ERPWebApp.Controllers.PurchaseOrders.PurchaseOrdersController;

namespace ERPWebApp.Services.IServices
{
    public interface IPurchaseOrderService : IService<PurchaseOrder>
    {
        Task<List<PurchaseOrder>> GetActivePurchaseOrdersByProductAsync(int productId);
        Task ForceCloseAsync(int id, string closeNote);
        void Close(int id);
        Task AddMiscProductAsync(MiscProduct miscProduct);
        Task<List<MiscProduct>> GetMiscProductsByPurchaseOrderId(int purchaseOrderId);
        Task AddMiscProductListAsync(List<MiscProduct> miscProductList);
        Task DeleteMiscProductAsync(int id, string modifiedByUser);
        Task UpdateMiscProducts(List<MiscProduct> miscProductList);
        Task<byte[]> GeneratePdfWithProductsAndMisc(
                    List<CombinedProductInfo> combinedProductInfoList,
                    PurchaseOrder purchaseOrderSingle);
        Task AddProductPurchaseOrdersAsync(List<ProductPurchaseOrder> productPurchaseOrderList);
    }
}