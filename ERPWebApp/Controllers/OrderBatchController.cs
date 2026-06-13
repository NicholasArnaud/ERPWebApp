using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPWebApp.Controllers
{
    [CwaFeatureGate(CwaFeatures.ORDER)]
    public class OrderBatchController : Controller
    {
        private OrderBatch _OrderBatch;
        private readonly IOrderBatchService _orderBatchService;
        private readonly IProductService _productService;
        private readonly IStocksService _stockService;
        private readonly IDepartmentService _departmentService;

        public OrderBatchController(
            IOrderBatchService orderBatchService,
            IProductService productService,
            IStocksService stockService,
            IDepartmentService departmentService)
        {
            _orderBatchService = orderBatchService;
            _productService = productService;
            _stockService = stockService;
            _departmentService = departmentService;
            _OrderBatch = new OrderBatch();
        }
        public async Task<IActionResult> Index()
        {
            var orderBatches = await _orderBatchService.GetAllAsync();
            var filteredBatches = await _orderBatchService.GetFilteredOrderBatchesAsync();
            List<Department> departmentList = await _departmentService.GetListAsync(
                query => query
                        .Where(d => d.IsActive && d.IsProduction)
                        .Select(d => new Department() { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                        .OrderBy(d => d.DepartmentName)
            );
            ViewData["OrderBatches"] = orderBatches;
            ViewData["FilteredOrderBatches"] = filteredBatches;
            ViewData["Departments"] = departmentList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompletedBatchItems(int orderBatchId)
        {
            try
            {
                var batchItems = await _orderBatchService.GetBatchItems(orderBatchId);
                var batchItemsWithFilteredProducts = await _orderBatchService.GetFilteredProductsForBatchItems(batchItems);
                return PartialView("_CompletedBatchesPartial", batchItemsWithFilteredProducts);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLocationInfo(string sku)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var locationInfo = await _orderBatchService.GetLocationInfo(sku, userId);
            return Json(locationInfo);
        }
        [HttpGet]
        public async Task<IActionResult> GetAltItemNumber(int orderBatchItemId)
        {
            var product = await _orderBatchService.GetProductByOrderBatchItemId(orderBatchItemId);
            var altItemNumber = product?.AltItemNumber;
            return Json(new { AltItemNumber = altItemNumber });
        }

        [HttpPost]
        public async Task<IActionResult> TransferStock(List<StockTransfer> stockTransfers)
        {
            try
            {
                (bool transactionSuccess, string errorMessage, string nextStatusName) = await _orderBatchService.TransferStock(stockTransfers, User.Identity.Name);

                if (transactionSuccess)
                {
                    TempData["SuccessMessage"] = "Stock transferred and statuses updated successfully";
                    return Json(new { success = true, nextStatusName = nextStatusName });
                }
                else
                {
                    return Json(new { success = false, errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductIdBySku(string sku)
        {
            int? productId = await _orderBatchService.GetProductIdBySku(sku);
            return Ok(productId);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus(int orderBatchId)
        {
            var orderBatch = await _orderBatchService.GetAsync(ob => ob.OrderBatchId == orderBatchId);
            if (orderBatch != null)
            {
                return Ok(orderBatch.Status);
            }
            return NotFound();
        }

        // TO DO: Same with the service layer function. This might need reversion later, but will comment it out for now.
        // ADDENDUM: Now is later.

        [HttpPost]
        public async Task<ActionResult> RemoveOrders(int cwaOrderId, int orderBatchId, bool isUnknown, string unknownProducts)
        {
            bool success;
            success = await _orderBatchService.RemoveOrders(cwaOrderId, orderBatchId);

            if (success)
            {
                //This needs to return a success, not the HTML. Needto change this Monday.
                return RedirectToAction("Index", "OrderBatch");
            }
            else
            {
                TempData["ErrorMessage"] = "Order removal failed.";
                return RedirectToAction("Index", "OrderBatch");
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckTransferStatus(int orderBatchId, int cwaOrderId)
        {

            var anyTransferred = await _orderBatchService.AnyItemsPickedAsync(orderBatchId, cwaOrderId);

            return Json(new { anyTransferred });
        }

        /*
        [HttpPost]
        public async Task<ActionResult> RemoveUnknownOrders([FromBody] RemoveUnknownOrdersRequest request)
        {
            int orderBatchId = request.OrderBatchId;
            List<UnknownProduct> unknownProducts = request.UnknownProducts;

            bool success = await _orderBatchService.RemoveUnknownOrders(orderBatchId, unknownProducts);

            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Order removal failed." });
            }
        }
        */
        [HttpPost]
        public async Task<IActionResult> RemoveBatch(int orderBatchId)
        {
            int affectedRows = await _orderBatchService.RemoveAsync(orderBatchId);
            return Ok(affectedRows > 0);
        }

        [HttpGet]
        public async Task<IActionResult> GetDesignBatchItems(int orderBatchId)
        {
            try
            {
                var designBatchItems = await _orderBatchService.GetDesignBatchItemsAsync(orderBatchId);

                // Populate the ImageSrc property for each product in the list    
                foreach (var batchItem in designBatchItems)
                {
                    var product = await _productService.GetAsync(p => p.ProductId == batchItem.ProductId);
                    if (product != null && product.ProductImages != null && product.ProductImages.Count > 0)
                    {
                        batchItem.ImageSrc = product.ProductImages.First().FileUrl;
                    }
                    else
                    {
                        batchItem.ImageSrc = "/images/default-product-image.png"; // Set a default image if the product has no image    
                    }
                }
 
                var departmentStatusLines = await _orderBatchService.GetDepartmentStatusLinesAsync(orderBatchId);

                foreach (var item in designBatchItems)
                {
                    item.IsValidStock = await _orderBatchService.IsValidStock(item.ProductId);
                    var locationsWithStock = await _orderBatchService.GetLocationsWithStockAsync(item.ProductId);

                    item.PickOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.PickOnly).ToList();
                    item.ReceiveOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.ReceiveOnly).ToList();
                }

                var combinedModel = Tuple.Create(designBatchItems, departmentStatusLines);

                return PartialView("_DesignBatchesPartial", combinedModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        //TO DO: Commented out RequiresPO and ready to reimplement when the new PO system is in.
        [HttpGet]
        public async Task<IActionResult> GetTransferableBatchItems(int orderBatchId)
        {
            try
            {
                var designBatchItems = await _orderBatchService.GetDesignBatchItemsAsync(orderBatchId);
                var departmentStatusLines = await _orderBatchService.GetDepartmentStatusLinesAsync(orderBatchId);
                var batch = await _orderBatchService.GetAsync(ob => ob.OrderBatchId == orderBatchId);
                var batchPORequired = batch.RequiresPO;

                var groupedItems = designBatchItems
                    .GroupBy(item => item.Sku)
                    .Select(group => new DesignBatchItemViewModel
                    {
                        Sku = group.Key,
                        ProductId = group.First().ProductId,
                        Quantity = group.Sum(item => item.Quantity),
                        Description = group.First().Description,
                        DepartmentName = group.First().DepartmentName,
                        Status = group.First().Status,
                        ImageSrc = group.First().ImageSrc,
                        OrderBatchId = group.First().OrderBatchId,
                        OrderBatchItemId = group.First().OrderBatchItemId,
                        OrderBatchItemIdList = group.Select(item => item.OrderBatchItemId).ToList(),
                        BatchItem = group.First().BatchItem,
                        RequiresPO = batchPORequired,
                        ERPOrderId = group.First().ERPOrderId
                    }).ToList();

                foreach (var item in groupedItems)
                {
                    item.IsValidStock = await _orderBatchService.IsValidStock(item.ProductId);
                    var locationsWithStock = await _orderBatchService.GetLocationsWithStockAsync(item.ProductId);

                    item.PickOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.PickOnly).ToList();
                    item.ReceiveOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.ReceiveOnly).ToList();
                }

                var combinedModel = Tuple.Create(groupedItems, departmentStatusLines);
                return PartialView("_TransferrableBatchesPartial", combinedModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStatus(int orderBatchItemId)
        {
            var batchItem = await _orderBatchService.GetBatchItemByOrderBatchItemIdAsync(orderBatchItemId);
            var productStatus = await _orderBatchService.GetProductStatusByOrderBatchItemIdAsync(orderBatchItemId);

            // Return the BatchItem and ProductStatus as a JSON response  
            return Json(new { BatchItem = batchItem, ProductStatus = productStatus });
        }

        [HttpGet]
        public async Task<IActionResult> PrintSimplifiedPickList(string batchNumber, int orderBatchId, bool includeImages)
        {
            TempData["BatchNumber"] = batchNumber;
            TempData["OrderBatchId"] = orderBatchId;
            TempData["IncludeImages"] = includeImages;

            List<SimplifiedInventoryPickList> pickListDetails = await _orderBatchService.GetSimplifiedPickListDetailsByBatchNumberAsync(batchNumber, includeImages);

            return View("SimplePickList", pickListDetails);
        }

        [HttpGet]
        public async Task<IActionResult> PrintExpandedPickList(int orderBatchId, string batchNumber)
        {
            TempData["BatchNumber"] = batchNumber;
            List<ExpandedPickList> pickListDetails = await _orderBatchService.GetExpandedPickListDetailsByOrderBatchIdAsync(orderBatchId);
            return View("ExpandedPickList", pickListDetails);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderBatchProgress(int orderBatchItemId)
        {
            // Call the service layer method to update the progress  
            await _orderBatchService.UpdateOrderBatchProgressAsync(orderBatchItemId);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderBatchItemAsync(int orderBatchItemId, int batchItemStatusId)
        {
            await _orderBatchService.UpdateOrderBatchItemStatusAsync(orderBatchItemId, batchItemStatusId);
            return Json(new { success = true });
        }


        // This might transition later to a more specific query, rather than all AltItem products.
        [HttpGet]
        public async Task<IActionResult> GetAllProductsWithAltItemNumbers()
        {
            var products = await _orderBatchService.GetAllProductsWithAltItemNumbersAndStockAsync();
            return Json(products.Select(p => new { p.ProductId, p.Sku }));
        }

        [HttpGet]
        public async Task<IActionResult> GetSkuByProductId(int productId)
        {
            var sku = await _orderBatchService.GetSkuByProductId(productId);
            //var product = await _productService.GetAsync(p => p.ProductId == productId);
            if (sku != null)
            {
                return Json(new { Sku = sku });
            }
            return NotFound();
        }
        public async Task<List<DepartmentStatusLineViewModel>> GetDepartmentStatusLines(int orderBatchId)
        {

                var departmentStatuses = await _orderBatchService.GetDepartmentStatusLinesAsync(orderBatchId);

                return departmentStatuses;
        }
        [HttpGet]
        public async Task<IActionResult> GetDepartmentStatuses()
        {
            try
            {
                var departmentStatuses = await _orderBatchService.GetDepartmentStatusesAsync();
                return Ok(departmentStatuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SetRequiresPo([FromBody] int orderBatchId)
        {
            try
            {
                bool isUpdated = await _orderBatchService.SetRequiresPoAsync(orderBatchId);
                if (!isUpdated)
                {
                    return Json(new { success = false, message = "Failed to update the RequiresPO field." });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
