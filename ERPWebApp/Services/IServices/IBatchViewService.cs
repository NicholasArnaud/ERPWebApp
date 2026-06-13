using static ERPWebApp.Models.BatchViewModel;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Services.IServices
{
    public interface IBatchViewService : IService<BatchView>
    {
        Task<List<BatchView>> GetAllBatches(string sku = null, int? departmentId = null);
        Task<List<ProductDetail>> GetProductDetailsForBatch(int orderBatchId);
        Task<List<Product>> GetAllActiveProducts();
        Task<List<OrderDetail>> GetOrderDetailsForBatch(int orderBatchId);
        Task<List<Department>> GetAllActiveDepartments();
        Task<OrderBatch> GetBatchDetails(int orderBatchId);
        Task<List<ProductDetailWithOrderBatchItem>> GetProductDetailsWithBatchItemForBatch(int orderBatchId);

    }
}
