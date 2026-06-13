using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;
using static ERPWebApp.Models.BatchViewModel;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Orders;


namespace ERPWebApp.Services
{
    public class BatchViewService : Service<BatchView>, IBatchViewService
    {
        IUnitOfWork _unitOfWork;
        IOrderBatchService _orderBatchService;

        public BatchViewService(IUnitOfWork unitOfWork, IOrderBatchService orderBatchService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderBatchService = orderBatchService;
        }

        public async Task<List<BatchView>> GetAllBatches(string sku = null, int? departmentId = null)
        {
            return await _unitOfWork.BatchView.GetAllBatches(sku, departmentId);
        }
        public async Task<OrderBatch> GetBatchDetails(int orderBatchId)
        {
            return await _unitOfWork.OrderBatch.FilterOneAsync(ob => ob.OrderBatchId == orderBatchId);
        }
        public async Task<List<ProductDetail>> GetProductDetailsForBatch(int orderBatchId)
        {
            return await _unitOfWork.BatchView.GetProductDetailsForBatch(orderBatchId);
        }
        public async Task<List<Product>> GetAllActiveProducts()
        {
            return await _unitOfWork.BatchView.GetAllActiveProducts();
        }
        public async Task<List<OrderDetail>> GetOrderDetailsForBatch(int orderBatchId)
        {
            return await _unitOfWork.BatchView.GetOrderDetailsForBatch(orderBatchId);
        }
        public async Task<List<Department>> GetAllActiveDepartments()
        {
            return await _unitOfWork.BatchView.GetAllActiveDepartments();
        }

        public async Task<List<ProductDetailWithOrderBatchItem>> GetProductDetailsWithBatchItemForBatch(int orderBatchId)
        {
            return await _unitOfWork.BatchView.GetProductDetailsWithOrderBatchtemForBatch(orderBatchId);
        }

    }
}
