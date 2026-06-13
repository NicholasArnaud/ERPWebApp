using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Services
{
    public class BatchItemStatusService : Service<BatchItemStatus>, IBatchItemStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BatchItemStatusService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BatchItemStatus>> GetAllWithDepartmentsAsync()
        {
            return await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query
                    .Include(b => b.Department)
            ).ToListAsync();
        }

        public async Task<BatchItemStatus> GetNextStatusAsync(int departmentId, int executionSequence)
        {
            return await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query
                    .Where(b => b.DepartmentId == departmentId && b.ExecutionSequence == executionSequence + 1)
            ).FirstOrDefaultAsync();
        }

        public async Task ReorderExecutionSequenceAfterDeletion(int deletedSequence, int departmentId)
        {
            var affectedStatuses = await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query
                    .Where(b => b.DepartmentId == departmentId && b.ExecutionSequence > deletedSequence)
            ).ToListAsync();

            foreach (var status in affectedStatuses)
            {
                status.ExecutionSequence--;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetExecutionSequenceOfStatus(int departmentId, string statusName)
        {
            return await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query
                    .Where(b => b.DepartmentId == departmentId && b.StatusName == statusName)
            ).Select(b => b.ExecutionSequence).FirstOrDefaultAsync();
        }

        public async Task InsertStatusBeforeAsync(BatchItemStatus newStatus)
        {
            // Get all statuses with execution sequence greater than or equal to the new status's execution sequence    
            var affectedStatuses = await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query
                    .Where(b => b.DepartmentId == newStatus.DepartmentId && b.ExecutionSequence >= newStatus.ExecutionSequence)
            ).ToListAsync();

            // Increment the execution sequence of the affected statuses    
            foreach (var status in affectedStatuses)
            {
                status.ExecutionSequence++;
            }

            // Add the new status to the context    
            _unitOfWork.BatchItemStatus.Add(newStatus);

            // Save the changes    
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByStatusIdAsync(int statusId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchItemsByStatusIdAsync(statusId);
        }

        public async Task UpdateOrderBatchItemAsync(OrderBatchItem orderBatchItem)
        {
            await _unitOfWork.OrderBatch.UpdateOrderBatchItemAsync(orderBatchItem);
        }
    }
}