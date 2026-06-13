using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[AutoValidateAntiforgeryToken]
public class BatchItemStatusController : Controller
{
    private readonly IBatchItemStatusService _batchItemStatusService;
    private readonly IDepartmentService _departmentService;

    public BatchItemStatusController(IBatchItemStatusService batchItemStatusService, IDepartmentService departmentService)
    {
        _batchItemStatusService = batchItemStatusService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index()
    {
        var batchItemStatuses = await _batchItemStatusService.GetAllWithDepartmentsAsync();
        var departments = await _departmentService.GetListAsync(d => d.IsActive && d.IsProduction);
        ViewData["BatchItemStatuses"] = batchItemStatuses;
        ViewData["Departments"] = departments;
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> CreateBatchItemStatus(BatchItemStatus batchItemStatus)
    {
        // Get the execution sequence of the "Complete" status for the department  
        int completeStatusSequence = await _batchItemStatusService.GetExecutionSequenceOfStatus(batchItemStatus.DepartmentId, "Completed");

        // Insert the new status before the "Complete" status  
        batchItemStatus.ExecutionSequence = completeStatusSequence;
        batchItemStatus.IsDeletable = true;

        await _batchItemStatusService.InsertStatusBeforeAsync(batchItemStatus);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBatchItemStatus(int id)
    {
        var statusToDelete = await _batchItemStatusService.GetAsync(b => b.BatchItemStatusId == id);
        int deletedSequence = statusToDelete.ExecutionSequence;
        int departmentId = statusToDelete.DepartmentId;

        var nextStatus = await _batchItemStatusService.GetNextStatusAsync(departmentId, deletedSequence);

        if (nextStatus != null)
        {
            var orderBatchItemsToUpdate = await _batchItemStatusService.GetOrderBatchItemsByStatusIdAsync(id);

            foreach (var item in orderBatchItemsToUpdate)
            {
                item.BatchItemStatusId = nextStatus.BatchItemStatusId;
                await _batchItemStatusService.UpdateOrderBatchItemAsync(item);
            }
        }

        await _batchItemStatusService.RemoveAsync(id);

        // Reorder the ExecutionSequence of the remaining statuses      
        await _batchItemStatusService.ReorderExecutionSequenceAfterDeletion(deletedSequence, departmentId);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateExecutionOrder(List<StatusOrderUpdate> newExecutionOrder)
    {
        foreach (var item in newExecutionOrder)
        {
            var status = await _batchItemStatusService.GetAsync(b => b.BatchItemStatusId == item.StatusId);
            status.ExecutionSequence = item.ExecutionSequence;
            await _batchItemStatusService.UpdateAsync(status);
        }

        return Ok();
    }

    public class StatusOrderUpdate
    {
        public int StatusId { get; set; }
        public int ExecutionSequence { get; set; }
    }
}
