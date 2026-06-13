using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;
using System.Text;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class BatchViewController : Controller
{
    private readonly IBatchViewService _batchViewService;
    private readonly IProductService _productService;

    public BatchViewController(IBatchViewService batchViewService, IProductService productService)
    {
        _batchViewService = batchViewService;
        _productService = productService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetBatchData(string sku = null, int? departmentId = null)
    {
        var batches = await _batchViewService.GetAllBatches(sku, departmentId);
        return Json(batches);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetailsForBatch(int orderBatchId)
    {
        var productDetails = await _batchViewService.GetProductDetailsForBatch(orderBatchId);
        return Json(productDetails);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetailsWithOrderBatchtemForBatch(int orderBatchId)
    {
        var productDetails = await _batchViewService.GetProductDetailsWithBatchItemForBatch(orderBatchId);
        return Json(productDetails);
    }
    

    [HttpGet]
    public async Task<IActionResult> GetAllActiveProducts()
    {
        var products = await _batchViewService.GetAllActiveProducts();
        return Json(products);
    }
    [HttpGet]
    public JsonResult GetAllActiveSkus(string searchTerm = "")
    {
        var skus = _productService.GetList(query => query
            .Where(p => p.IsActive && (p.Sku.Contains(searchTerm) || p.Description.Contains(searchTerm)))
            .Select(p => new ProductInfo { Sku = p.Sku, Description = p.Description })
            .Distinct()
            .OrderBy(p => p.Sku)
        );
        return Json(skus);
    }
    [HttpGet]
    public async Task<IActionResult> GetOrderDetailsForBatch(int orderBatchId)
    {
        var orderDetails = await _batchViewService.GetOrderDetailsForBatch(orderBatchId);
        return Json(orderDetails);
    }
    [HttpGet]
    public async Task<JsonResult> GetAllActiveDepartments()
    {
        var departments = await _batchViewService.GetAllActiveDepartments();
        return Json(departments.Select(d => new { d.DepartmentId, d.DepartmentName }));
    }
    [HttpGet]
    public async Task<IActionResult> ExportBatchToCsv(int orderBatchId)
    {
        var productDetails = await _batchViewService.GetProductDetailsForBatch(orderBatchId);
        var orderDetails = await _batchViewService.GetOrderDetailsForBatch(orderBatchId);
        var batchDetails = await _batchViewService.GetBatchDetails(orderBatchId);


        var sb = new StringBuilder();

        sb.AppendLine($"Batch Name: {batchDetails.BatchNumber}");

        sb.AppendLine("Product SKU,Quantity,,Order Number");

        var maxRows = Math.Max(productDetails.Count, orderDetails.Count);

        for (int i = 0; i < maxRows; i++)
        {
            var productSku = i < productDetails.Count ? productDetails[i].productSku : string.Empty;
            var quantity = i < productDetails.Count ? productDetails[i].quantity.ToString() : string.Empty;
            var orderNumber = i < orderDetails.Count ? orderDetails[i].orderNumber : string.Empty;

            sb.AppendLine($"{productSku},{quantity},,{orderNumber}");
        }

        var fileName = $"BatchDetails - {batchDetails.BatchNumber}.csv";
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return File(bytes, "text/csv", fileName);
    }
}
