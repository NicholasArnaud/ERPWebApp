using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;
using static ERPWebApp.Models.Orders.Order;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.ORDER)]
public partial class OrdersController
{

    [HttpPost]
    [IgnoreAntiforgeryToken]  
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    public async Task<IActionResult> BatchCreation(string ERPOrderIdsJson,int BatchType,string BatchName,string assignedDepartmentsJson,string replacementSkusJson,bool IsDeductible = true)
    {
        List<int> ERPOrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);
        List<ReplacementSku> replacementSkus = replacementSkusJson != null ? JsonConvert.DeserializeObject<List<ReplacementSku>>(replacementSkusJson) : null;
        List<AssignedDepartment> assignedDepartments = assignedDepartmentsJson != null ? JsonConvert.DeserializeObject<List<AssignedDepartment>>(assignedDepartmentsJson) : null;

        List<Order> orders = await _orderBatchService.GetOrdersWithProductsByERPOrderIdsAsync(ERPOrderIds);

        var shipStationOrderIds = new List<long>();
        var storeIds = new HashSet<long>();

        foreach (var order in orders)
        {
            await _orderItemService.AssignProductIds(order.items);
                shipStationOrderIds.Add(order.orderId);
                storeIds.Add(order.advancedOptions.storeId);
        }

        BatchCreationResult result = await _orderBatchService.CreateBatchAsync(ERPOrderIds,BatchType,BatchName,assignedDepartments,replacementSkus, User.Identity.Name, IsDeductible);

        if (!result.Success)
        {
            if (result.MissingSkus != null && result.MissingSkus.Count > 0)
            {
                ViewBag.MissingSkus = result.MissingSkus;
                return Json(new { status = "missing_skus", missingSkus = result.MissingSkus });
            }
            if (result.UnassignedDepartments != null && result.UnassignedDepartments.Count > 0)
            {
                ViewBag.UnassignedDepartments = result.UnassignedDepartments;
                return Json(new { status = "unassigned_departments", unassignedDepartments = result.UnassignedDepartments });
            }
            return Json(new { status = "error", message = result.Message });
        }

        TempData["BatchNumber"] = result.CompleteBatchNumber;
        ViewBag.ActiveProducts = await _orderBatchService.GetActiveProductsAsync();
        SetTempDataValues(); 

        return Json(new { status = "success", message = result.Message });
    }

    [HttpGet]
    public JsonResult GetAllActiveSkus()
    {
        List<ProductInfo> skus = _productService.GetList(query => query
            .Where(p => p.IsActive)
            .Select(p => new ProductInfo { Sku = p.Sku, Description = p.Description })
            .Distinct()
        );
        return Json(skus);
    }

    [HttpGet]
    public async Task<IActionResult> CheckForDuplicateBatches(string ERPOrderIdsJson)
    {
        List<int> ERPOrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);
        List<DuplicateBatchInfo> duplicateBatchInfos = await _orderBatchService.CheckDuplicateBatchesByERPOrderIdsAsync(ERPOrderIds);

        if (duplicateBatchInfos.Count > 0)
        {
            return Json(new { status = "duplicate_batches", duplicateBatchInfos });
        }
        else
        {
            return Json(new { status = "no_duplicates" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> PrintPickList()
    {
        string batchNumber = TempData["BatchNumber"] as string;
        bool includeImages = false;
        ViewBag.BatchNumber = batchNumber;
        List<SimplifiedInventoryPickList> pickListDetails = await _orderBatchService.GetSimplifiedPickListDetailsByBatchNumberAsync(batchNumber, includeImages);

        return View(pickListDetails);
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveDepartments()
    {
        var activeDepartments = await _orderBatchService.GetActiveDepartmentsAsync();
        return Ok(activeDepartments);
    }

    [HttpPost]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    public async Task<IActionResult> AddOrdersToBatch(string ERPOrderIdsJson, int BatchId, string assignedDepartmentsJson = null, string replacementSkusJson = null)
    {
        List<int> ERPOrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);
        List<ReplacementSku> replacementSkus = replacementSkusJson != null ? JsonConvert.DeserializeObject<List<ReplacementSku>>(replacementSkusJson) : null;
        List<AssignedDepartment> assignedDepartments = assignedDepartmentsJson != null ? JsonConvert.DeserializeObject<List<AssignedDepartment>>(assignedDepartmentsJson) : null;

        List<Order> orders = await _orderBatchService.GetOrdersWithProductsByERPOrderIdsAsync(ERPOrderIds);

        var shipStationOrderIds = new List<long>();
        var storeIds = new HashSet<long>();

        foreach (var order in orders)
        {
            await _orderItemService.AssignProductIds(order.items);
            shipStationOrderIds.Add(order.orderId);
            storeIds.Add(order.advancedOptions.storeId);
        }

        var result = await _orderBatchService.AddOrdersToBatchAsync(BatchId, ERPOrderIds, assignedDepartments, replacementSkus);

        if (!result.Success)
        {
            if (result.MissingSkus != null && result.MissingSkus.Count > 0)
            {
                return Json(new { status = "missing_skus", missingSkus = result.MissingSkus });
            }

            if (result.UnassignedDepartments != null && result.UnassignedDepartments.Count > 0)
            {
                return Json(new { status = "unassigned_departments", unassignedDepartments = result.UnassignedDepartments });
            }

            return Json(new { status = "error", message = result.Message });
        }

        return Json(new { status = "success", message = result.Message });
    }
}