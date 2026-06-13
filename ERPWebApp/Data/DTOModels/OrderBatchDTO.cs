using static ERPWebApp.Data.Repositories.OrderBatchRepository;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.DTOModels
{
    public class OrderBatchDTO
    {
        
    }
    public class ProductInfo
    {
        public string Sku { get; set; }
        public string Description { get; set; }
    }
    public class LocationInfo
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int TotalAvailable { get; set; }
        public LocationType Type { get; set; }
        public bool IsDefault { get; set; }
    }
    #region OrderBatch
    public class StockTransfer
    {
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int OrderBatchId { get; set; }
        public int OrderBatchItemId { get; set; }
        public List<int> OrderBatchItemIdList { get; set; }
        public int OrderBatchProductMappingId { get; set; }
    }
    public class RemoveUnknownOrdersRequest
    {
        public int OrderBatchId { get; set; }
        public List<UnknownProduct> UnknownProducts { get; set; }
    }

    public class UnknownProduct
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
    #region PickList
    public class OrderQuantity
    {
        public int ERPOrderId { get; set; }
        public string OrderNumber { get; set; }
    }

    public class InventoryPickList
    {
        public string Sku { get; set; }
        public string InvalidSku { get; set; }
        public bool isAltItem { get; set; }
        public string Description { get; set; }
        public int AmountRequired { get; set; }
        public int ERPProductId { get; set; }
        public int ERPOrderItemId { get; set; }
        public int ERPOrderId { get; set; }
        public int Quantity { get; set; }
        public string OrderNumber { get; set; }
        public int Department {  get; set; }
        public List<int> DepartmentList { get; set; }
        public List<OrderQuantity> OrderQuantities { get; set; }
        public List<OrderItem.OrderItemOption> OrderOptions { get; set; }
    }
    public class SimplifiedInventoryPickList
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string FromLocation { get; set; }
        public string ImageUrl { get; set; }
    }
    public class ExpandedPickList
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public int OrderBatchItemId { get; set; }
        public int OrderBatchId { get; set; }
        public string BatchNumber { get; set; }
        public string FromLocation { get; set; }
    }
    public class BatchItemPickList
    {
        public int ProductId { get; set; }
        public int ERPOrderItemId { get; set; }
        public int ERPOrderId { get; set; }
        public List<Department> Departments { get; set; }
    }
    #endregion
    public class BatchWithItems
    {
        public int OrderBatchId { get; set; }
        public string BatchNumber { get; set; }
        public List<OrderBatchItem> Items { get; set; }
    }

    public class DepartmentStatusDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<BatchItemStatusDto> Statuses { get; set; }
    }

    public class BatchItemStatusDto
    {
        public int BatchItemStatusId { get; set; }
        public string StatusName { get; set; }
        public int ExecutionSequence { get; set; }
    }
    public class BatchCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<MissingSkuEntry> MissingSkus { get; set; }
        public List<int> UnassignedDepartments { get; set; }
        public List<SimplifiedInventoryPickList> SimplifiedPickList { get; set; }
        public string CompleteBatchNumber { get; set; }
    }

    public class BatchOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<MissingSkuEntry> MissingSkus { get; set; }
        public List<int> UnassignedDepartments { get; set; }
    }

    //Making this class to drastically reduce the amount of data being sent to the front end. Things were breaking for absurdly large quantities of orders.
    public class ReplacementSku
    {
        public string OriginalSku { get; set; }
        public string NewSku { get; set; }
        public int NewPID { get; set; }
    }
    public class AssignedDepartment
    {
        public int OrderItemId { get; set; }
        public int AssignedDepartmentId { get; set; }
    }
    public class MissingSkuEntry
    {
        public string Sku { get; set; }
        public List<OrderItem.OrderItemOption> OrderOptions { get; set; }
    }

    public class DuplicateBatchInfo
    {
        public string BatchNumber { get; set; }
        public string OrderNumber { get; set; }
    }
}
