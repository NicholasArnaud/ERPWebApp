using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Services.IServices
{
    public interface IOrderBatchService : IService<OrderBatch>
    {
        #region Pre-Creation Checks
        Task<List<MissingSkuEntry>> GetMissingSkusListAsync(List<InventoryPickList> inventoryPickList);
        Task<List<int>> GetUnassignableDepartments(List<InventoryPickList> inventoryPickList);
        Task<List<DuplicateBatchInfo>> CheckDuplicateBatchesByERPOrderIdsAsync(List<int> ERPOrderIds);
        #endregion

        #region Batch Creation
        Task<BatchCreationResult> CreateBatchAsync(List<int> ERPOrderIds,int BatchType,string BatchName,List<AssignedDepartment> assignedDepartments,List<ReplacementSku> replacementSkus, string User, bool IsDeductible = true);
        Task<InventoryPickList> CreatePickListItem(string sku, string description, int amountRequired, int ERPProductId, int ERPOrderItemId, Order order, ReplacementSku replacement, AssignedDepartment assignedDepartment, OrderItem item);

        #endregion

        #region Batch Dropdown Population
        Task<List<BatchItemViewModel>> GetFilteredProductsForBatchItems(List<BatchItemViewModel> batchViewModels);
        #endregion

        #region Location Info Related
        Task<List<LocationInfo>> GetLocationInfo(string sku, string userId);
        Task<List<DesignBatchItemViewModel>> GetDesignBatchItemsWithLocationsAsync(int orderBatchId);
        Task<List<LocationInfo>> GetLocationsWithStockAsync(int productId);
        Task<int?> GetProductIdBySku(string sku);
        Task<Product> GetProductByOrderBatchItemId(int orderBatchItemId);

        Task<List<BatchItemViewModel>> GetBatchItems(int orderBatchId);
        Task<List<BatchItemViewModel>> HandleSkuSets(List<BatchItemViewModel> batchItemViewModels);

        #endregion

        #region Transfer Related
        Task<(bool, string, string)> TransferStock(List<StockTransfer> stockTransfers, string currentUserName);
        #endregion

        #region Order Removal
        Task<bool> RemoveOrders(int cwaOrderId, int orderBatchId);
        Task<bool> AnyItemsPickedAsync(int orderBatchId, int cwaOrderId);
        #endregion
        Task<List<Order>> GetOrdersWithProductsByERPOrderIdsAsync(List<int> cwaOrderIds);
        Task<List<Product>> GetActiveProductsAsync();
        Task<OrderBatchItem> GetOrderBatchItemByERPOrderId(int cwaOrderId);
        Task<List<SimplifiedInventoryPickList>> GetSimplifiedPickListDetailsByBatchNumberAsync(string batchNumber, bool includeImages);
        Task<List<ExpandedPickList>> GetExpandedPickListDetailsByOrderBatchIdAsync(int orderBatchId);
        Task<string> GetCompleteBatchNumberByBatchNumberAsync(string batchNumber);
        Task<List<DesignBatchItemViewModel>> GetDesignBatchItemsAsync(int orderBatchId);
        Task<BatchItemStatus> GetProductStatusByOrderBatchItemIdAsync(int orderBatchItemId);
        Task<List<int>> GetDepartmentIdsByProductIdAsync(int productId);
        Task<OrderBatchItem> UpdateOrderBatchProgressAsync(int orderBatchItemId);
        Task<OrderBatchItem> UpdateOrderBatchProgressAsync(int orderBatchItemId, int? desiredBatchStatusId = null);
        Task<OrderBatchItem> GetBatchItemByOrderBatchItemIdAsync(int orderBatchItemId);
        Task<List<Product>> GetAllProductsWithAltItemNumbersAndStockAsync();
        Task<string> GetSkuByProductId(int productId);
        Task<List<DepartmentStatusLineViewModel>> GetDepartmentStatusLinesAsync(int orderBatchId);
        Task UpdateOrderBatchItemsStatusToCompletedAsync(string orderNumber);
        Task UpdateOrderBatchItemStatusAsync(int orderBatchItemId, int batchItemStatusId);
        Task<Department> GetDepartmentByIdAsync(int departmentId);
        Task<List<Department>> GetActiveDepartmentsAsync();
        Task<List<OrderBatch>> GetFilteredOrderBatchesAsync();
        Task<List<OrderBatch>> GetOrderBatchesWithoutPickedItems();
        Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchId(int orderBatchId);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(int orderBatchId);
        Task<List<DepartmentStatusDto>> GetDepartmentStatusesAsync();
        Task<bool> IsValidStock(int productId);
        Task<BatchOperationResult> AddOrdersToBatchAsync(int batchId, List<int> cwaOrderIds, List<AssignedDepartment> assignedDepartments = null, List<ReplacementSku> replacementSkus = null);
        Task<Dictionary<int, string>> GetOrderBatchNumbersByOrderIds(List<int> orderIds);
        Task<string> OrderBatchItemStatusTransferUpdates(int orderBatchItemId, int productId);
        Task<Dictionary<int, List<string>>> GetOrderBatchNumberByOrderId(int orderId);
        Task<bool> UpdateOrderBatchPurchaseOrderDetailsAsync(int purchaseOrderId, List<int> batchIds);
        Task<bool> SetRequiresPoAsync(int orderBatchId);
        Task<bool> UndoBatchPOIdAssignmentAsync(int purchaseOrderId, List<int> batchIds);
        Task UndoTransfer(OrderBatchItem item);
    }
}
