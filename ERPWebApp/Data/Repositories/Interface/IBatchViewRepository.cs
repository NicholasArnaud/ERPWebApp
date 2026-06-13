using static ERPWebApp.Models.BatchViewModel;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IBatchViewRepository : IRepository<BatchView>
    {
        Task<List<BatchView>> GetAllBatches(string sku = null, int? departmentId = null);
        Task<List<ProductDetail>> GetProductDetailsForBatch(int orderBatchId);
        Task<List<Product>> GetAllActiveProducts();
        Task<List<OrderDetail>> GetOrderDetailsForBatch(int orderBatchId);
        Task<List<Department>> GetAllActiveDepartments();
        Task<List<ProductDetailWithOrderBatchItem>> GetProductDetailsWithOrderBatchtemForBatch(int orderBatchId);

    }
}
