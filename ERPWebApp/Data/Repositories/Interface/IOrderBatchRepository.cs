using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IOrderBatchRepository : IRepository<OrderBatch>
    {
        #region Batch Creation Related
        Task<OrderBatch> CreateOrderBatch(string batchNumber, string User, BatchType? batchType = null, bool IsDeductible = true);
        Task<List<OrderBatchItem>> CreateOrderBatchItems(int orderBatchId, List<InventoryPickList> inventoryPickList, bool isDeductible);
        #endregion

        #region Batch Dropdown Population
        Task<List<Product>> GetFilteredProducts(string skuPrefix);
        #endregion

        #region Post-Table Creation
        bool IsAltItemCheck(string sku);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchId(int orderBatchId);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(int orderBatchId);
        Task<OrderBatchItem> GetOrderBatchItemByOrderBatchItemId(int orderBatchItemId);
        Task<BatchItemStatus> GetNextBatchItemStatusByDepartmentAndExecutionSequence(int departmentId, int executionSequence);
        Task<OrderBatchItem> UpdateOrderBatchItem(OrderBatchItem orderBatchItem);
        #endregion

        #region Location Info Related
        Task<List<Stock>> GetStocksBySku(string sku);
        Task<List<Location>> GetLocationsByStocks(List<Stock> stocks);
        Task<List<Location>> GetReceiveOnlyLocations();
        #endregion

        #region Transfer Related
        Task<string> GetSkuByProductId(int productId);
        #endregion
        #region Transactions
        Task<bool> ExecuteTransactionAsync(Func<Task> action);
        #endregion

        Task<Stock> GetStockByLocationIdAndProductId(int locationId, int productId);
        Task<List<Order>> GetOrdersWithProductsByERPOrderIdsAsync(List<int> cwaOrderIds);
        Task<OrderBatchItem> GetOrderBatchItemByERPOrderId(int cwaOrderId);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByERPOrderIds(List<int> ERPOrderIds);
        Task<List<SimplifiedInventoryPickList>> GetSimplifiedPickListDetailsByBatchNumberAsync(string batchNumber, bool includeImages);
        Task<List<ExpandedPickList>> GetExpandedPickListDetailsByOrderBatchIdAsync(int orderBatchId);
        Task<string> GetCompleteBatchNumberByBatchNumberAsync(string batchNumber);
        Task<OrderBatchItem> GetOrderBatchItemByIdAsync(int orderBatchItemId);
        Task<BatchItemStatus> GetBatchItemStatusByIdAsync(int batchItemStatusId);
        Task<BatchItemStatus> GetNextBatchItemStatusAsync(int departmentId, int currentExecutionSequence);
        Task UpdateOrderBatchItemAsync(OrderBatchItem orderBatchItem);
        Task<BatchItemStatus> GetProductStatusByOrderBatchItemIdAsync(int orderBatchItemId);
        Task<OrderBatchItem> GetBatchItemByOrderBatchItemIdAsync(int orderBatchItemId);
        Task<bool> AllItemsCompletedForOrderBatchAsync(int orderBatchId);
        Task<BatchItemStatus> GetLastBatchItemStatusAsync(int departmentId);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByStatusIdAsync(int statusId);
        Task<Department> GetDepartmentForBatchItemByIdAsync(int departmentId);
        Task<List<OrderBatch>> GetFilteredOrderBatchesAsync();
        Task<List<OrderBatch>> GetOrderBatchesWithoutPickedItems();
        Task<Dictionary<int, string>> GetOrderBatchNumbersByOrderIds(List<int> orderIds);
        Task<Dictionary<int, List<string>>> GetOrderBatchNumberByOrderId(int orderId);
        Task<bool> UpdateOrderBatchPurchaseOrderDetails(int purchaseOrderId, List<int> batchIds);
        Task<bool> UndoBatchPOIdAssignment(int purchaseOrderId, List<int> batchIds);
    }

}
