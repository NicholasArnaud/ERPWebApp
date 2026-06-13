using ERPWebApp.Models.Orders;

namespace ERPWebApp.Services.IServices
{
    public interface IBatchItemStatusService : IService<BatchItemStatus>
    {
        Task<List<BatchItemStatus>> GetAllWithDepartmentsAsync();
        Task<BatchItemStatus> GetNextStatusAsync(int departmentId, int executionSequence);
        Task ReorderExecutionSequenceAfterDeletion(int deletedSequence, int departmentId);
        Task<int> GetExecutionSequenceOfStatus(int departmentId, string statusName);
        Task InsertStatusBeforeAsync(BatchItemStatus newStatus);
        Task<List<OrderBatchItem>> GetOrderBatchItemsByStatusIdAsync(int statusId);
        Task UpdateOrderBatchItemAsync(OrderBatchItem orderBatchItem);
    }
}