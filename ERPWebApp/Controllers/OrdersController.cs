using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using ERPWebApp.Extensions;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Controllers;

[Authorize(
Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ProductionBasic + "," + RoleList.ShippingBasic + "," + RoleList.ShippingManager)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public partial class OrdersController(IOrderService orderService,
    IOrderTagService orderTagService,
    IOrderShippingService orderShippingService,
    IOrderFulfillmentService orderFulfillmentService,
    IShipStationStoreService shipStationStoreService,
    IShippingScanoutService shippingScanoutService,
    IProductService productService,
    IDepartmentService departmentService,
    IWebhooks webhooks,
    IOrderBatchService orderBatchService,
    UserManager<IdentityUser> userManager,
    ILogger<OrdersController> logger,
    IOrderItemService orderItemService
) : Controller
{
    private readonly IOrderService _orderService = orderService;
    private readonly IOrderTagService _orderTagService = orderTagService;
    private readonly IOrderShippingService _orderShippingService = orderShippingService;
    private readonly IOrderFulfillmentService _orderFulfillmentService = orderFulfillmentService;
    private readonly IShipStationStoreService _shipstationStoreService = shipStationStoreService;
    private readonly IShippingScanoutService _shippingScanoutService = shippingScanoutService;
    private readonly IProductService _productService = productService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IWebhooks _webhooks = webhooks;
    private readonly IOrderBatchService _orderBatchService = orderBatchService;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly ILogger<OrdersController> _logger = logger;
    private readonly IOrderItemService _orderItemService = orderItemService;

    // GET: Orders  
    public async Task<IActionResult> Index(
        string orderNumber,
        OrderStatus[] orderStatus = null,
        int storeId = 0,
        int[] orderTagId = null,
        int[] productIds = null,
        int[] departmentIds = null,
        string itemName = null,
        string orderStartDate = "", string orderEndDate = "", string shipByDate = "",
        int? orderBatchId = null,
        List<string> excludeItemNames = null,
        bool autoEnterDetails = true,
        bool includeBatchedOrders = true
    )
    {
        orderStatus ??= [OrderStatus.awaiting_shipment];
        // Set the ViewData properties from TempData (if available)  
        var viewModel = new OrderViewModel
        {
            OrderNumber = (string)(TempData["OrderNumber"] ?? orderNumber),
            OrderStatus = (OrderStatus[])(TempData["OrderStatus"] ?? orderStatus),
            StoreId = (int)(TempData["StoreId"] ?? storeId),
            DepartmentIds = (int[])(TempData["DepartmentIds"] ?? departmentIds),
            OrderTagId = (int[])(TempData["OrderTagId"] ?? orderTagId),
            ProductIds = (int[])(TempData["ProductIds"] ?? productIds),
            OrderStartDate = (string)(TempData["OrderStartDate"] ?? orderStartDate),
            OrderEndDate = (string)(TempData["OrderEndDate"] ?? orderEndDate),
            ShipByDate = (string)(TempData["ShipByDate"] ?? shipByDate),
            OrderBatchId = (int?)(TempData["OrderBatchId"] ?? orderBatchId),
            ExcludeItemNames = TempData["ExcludeItemNames"] as List<string> ?? excludeItemNames,
            AutoEnterDetails = (bool)(TempData["AutoEnterDetails"] ?? autoEnterDetails),
            IncludeBatchedOrders = (bool)(TempData["IncludeBatchedOrders"] ?? includeBatchedOrders),
            ItemName = (string)(TempData["ItemName"] ?? itemName)

        };

        List<ShipStationStore> storeList = await _shipstationStoreService.GetListAsync(
            query => query
                    .Where(s => s.IsActive)
                    .Select(s => new ShipStationStore() { ShipStationStoreId = s.ShipStationStoreId, StoreName = s.StoreName })
                    .OrderBy(s => s.StoreName)
        );

        if (storeList != null)
        {
            viewModel.StoreNames = new SelectList(storeList, nameof(ShipStationStore.ShipStationStoreId), nameof(ShipStationStore.StoreName));
        }

        List<Product> productList = await _productService.GetListAsync(
            query => query
                .Where(p => p.IsActive
                    && (
                        departmentIds == null
                        || departmentIds.Length == 0
                        || p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                    )
                )
                .Include(p => p.Departments.Where(d => d.IsProduction && d.IsActive))
                .Select(p => new Product() { ProductId = p.ProductId, Sku = p.Sku })
                .OrderBy(p => p.Sku)
        );

        if (productList != null)
        {
            viewModel.Products = new SelectList(productList, nameof(Product.ProductId), nameof(Product.Sku));
        }

        List<Department> departmentList = await _departmentService.GetListAsync(
            query => query
                .Where(d => d.IsActive && d.IsProduction)
                .Select(d => new Department() { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                .OrderBy(d => d.DepartmentName)
        );

        if (departmentList != null)
        {
            viewModel.Departments = new SelectList(departmentList, nameof(Department.DepartmentId), nameof(Department.DepartmentName));
        }

        var shipstationTagList = await _orderTagService.GetListAsync(
            query => query
                    .Select(t => new { TagId = t.tagId, TagName = t.name })
                    .OrderBy(t => t.TagName)
        );

        if (shipstationTagList != null)
        {
            viewModel.Tags = new SelectList(shipstationTagList, "TagId", "TagName");
        }

        DateTime fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
        List<OrderBatch> orderBatchList = await _orderBatchService.GetListAsync(
            query => query
                .Where(b => b.CreateDate >= fiveDaysAgo)
        );

        if (orderBatchList != null)
        {
            viewModel.OrderBatches = new SelectList(orderBatchList, nameof(OrderBatch.OrderBatchId), nameof(OrderBatch.BatchNumber));
        }

        List<OrderBatch> orderBatchesForOrderAdditionList = await _orderBatchService.GetOrderBatchesWithoutPickedItems();

        if (orderBatchList != null)
        {
            viewModel.OrderBatchesForOrderAddition = new SelectList(orderBatchesForOrderAdditionList, nameof(OrderBatch.OrderBatchId), nameof(OrderBatch.BatchNumber));
        }

        viewModel.Order = new Order();

        return View(viewModel);
    }


    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetOrders(
        int draw,
        int start,
        int length,
        string orderNumber,
        string itemName,
        OrderStatus[] orderStatus,
        int storeId,
        int[] productIds,
        int[] departmentIds,
        int[] orderTagId,
        string orderStartDate,
        string orderEndDate,
        string shipByDate,
        int orderColumn = 0,
        string orderDir = "asc",
        int? orderBatchId = null,
        List<string> excludeItemNames = null,
        bool includeBatchedOrders = true
    )
    {
        if (start < 0 || length <= 0)
        {
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0 });
        }

        try
        {
            List<string> ordernumbers = orderNumber?
                .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? [];

            var (orders, totalCount) = await _orderService.GetOrdersAsync(
                start,
                length,
                ordernumbers,
                itemName,
                orderStatus,
                storeId,
                productIds,
                departmentIds,
                orderTagId,
                orderStartDate,
                orderEndDate,
                shipByDate,
                GetSortColumn(orderColumn),
                orderDir,
                orderBatchId,
                excludeItemNames,
                includeBatchedOrders
            );

            var orderIdsList = orders.Select(o => o.ERPOrderId).ToList();
            var orderBatchNumbers = await _orderBatchService.GetOrderBatchNumbersByOrderIds(orderIdsList);

            //Remove the circular references
            orders.ForEach(x =>
            {
                x.items.ForEach(y => y.Order = null);
                x.advancedOptions.Order = null;
                x.Tags.ForEach(y => y.Orders = null);
            });

            // Prepare the data for DataTables  
            var data = orders.Select(o =>
            {
                (string statusClass, string statusIcon) = GetOrderStatusDetails(o.orderStatus);
                string itemNameCol = o.items.Select(x => x.sku).Where(x => x != "").Count() == 1
                        ? o.items.Select(x => x.name).FirstOrDefault()
                        : "(" + o.items.Select(x => x.sku).Where(x => x != "").Count().ToString() + " Items)";
                string itemSkuCol = o.items.Where(x => x.lineItemKey != "Discount").Select(x => x.sku).Distinct().Count() == 1
                        ? o.items.Where(x => x.lineItemKey != "Discount").Select(x => x.sku).Distinct().FirstOrDefault()
                        : "(" + o.items.Where(x => x.lineItemKey != "Discount").Select(x => x.sku).Distinct().Count().ToString() + " SKUs)";

                string itemQuantityCol = o.items.Where(x => x.lineItemKey != "Discount").Count() > 0
                        ? o.items.Where(x => x.lineItemKey != "Discount").Sum(x => x.quantity).ToString()
                        : "0";
                var batchNumber = orderBatchNumbers.TryGetValue(o.ERPOrderId, out var b) ? b : "N/A";
                return new
                {
                    o.ERPOrderId,
                    o.orderNumber,
                    orderDate = o.orderDate.ToString("yyyy-MM-dd"),
                    orderAge = (DateTime.Now - o.orderDate).Days.ToString() + " Day(s)",
                    shipByDate = o.shipByDate?.ToString("yyyy-MM-dd"),
                    orderStatus = $"{statusClass}{statusIcon}<span>{o.orderStatus}</span>",
                    orderTotal = o.orderTotal.ToString("C"),
                    customerNotes = o.customerNotes ?? "",
                    internalNotes = o.internalNotes ?? "",
                    storeName = string.IsNullOrEmpty(o.advancedOptions.storeName) ? "Custom/Unknown" : o.advancedOptions.storeName,
                    tags = o.Tags.ToArray(),
                    shipDate = o.shipDate?.ToString("yyyy-MM-dd"),
                    estimatedShipmentCost = o.estimatedShipmentCost.ToString("C"),
                    itemName = itemNameCol,
                    itemSku = itemSkuCol,
                    itemQuantity = itemQuantityCol,
                    isDuplicated = (o.IsDuplicated != null && o.IsDuplicated.Value) ? "<span>Yes</span>" : "<span>No</span>",
                    reason = o.duplicationReason.ToString(),
                    parentOrder = o.ParentERPOrderId,
                    batchNumber
                };
            });

            ViewData["OrderStatus"] = orderStatus;
            ViewData["OrderNumber"] = orderNumber;
            ViewData["StoreId"] = storeId;
            ViewData["ProductIds"] = productIds;
            ViewData["DepartmentIds"] = departmentIds;
            ViewData["OrderTagId"] = orderTagId;
            ViewData["OrderStartDate"] = orderStartDate;
            ViewData["OrderEndDate"] = orderEndDate;
            ViewData["ShipByDate"] = shipByDate;
            ViewData["ExcludeItemNames"] = excludeItemNames;

            // Return the JSON data for DataTables  
            return Json(new { draw, recordsTotal = totalCount, recordsFiltered = totalCount, data });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }


    [HttpPost, ActionName("GetOrderUpdatesBulk")]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> GetOrderUpdatesBulk(string ERPOrderIdsJson)
    {
        List<int> OrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);
        try
        {
            _ = await _orderService.GetOrderUpdates(OrderIds);

        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }

        // Store the TempData values for the next request  
        SetTempDataValues();

        // Redirect back to the Index page  
        return RedirectToAction("Index", new
        {
            orderNumber = ViewData["OrderNumber"],
            orderStatus = ViewData["OrderStatus"],
            storeId = ViewData["StoreId"],
            productId = ViewData["ProductId"],
            departmentId = ViewData["DepartmentId"],
            orderTagId = ViewData["OrderTagId"],
            orderStartDate = ViewData["OrderStartDate"],
            orderEndDate = ViewData["OrderEndDate"],
            shipByDate = ViewData["ShipByDate"]
        });
    }

    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    public async Task<IActionResult> GetRateEstimate([Bind("ERPOrderId,orderId,orderKey,orderNumber,weight,dimensions")] Order inputOrderShipping)
    {
        try
        {
            //Retrieve all the order info for the user  
            Order dbData = await _orderShippingService.GetOrderByOrderIdAndKeyCustomSelectAsync(inputOrderShipping.orderId, inputOrderShipping.orderKey);

            if (!ModelState.IsValid)
            {
                ViewData["ScanOutTime"] = "";
                ViewData["BatchTime"] = "";
                if (dbData.orderStatus == OrderStatus.shipped && dbData.orderShipments.Any(os => !os.voided))
                {
                    IEnumerable<int> orderShipmentIds = dbData.orderShipments.Select(os => os.OrderShipmentId);
                    ShippingScanout shipmentScanouts = await _shippingScanoutService.GetAsync(x => orderShipmentIds.Contains(x.OrderShipmentId.Value));
                    ViewData["ScanOutTime"] = shipmentScanouts?.CreateDate.ToString() ?? "";
                }
                else if (dbData.orderStatus == OrderStatus.shipped && dbData.orderFulfillments.Any(of => !of.voided))
                {
                    IEnumerable<int> orderFulfillmentIds = dbData.orderFulfillments.Select(os => os.OrderFulfillmentId);
                    ShippingScanout fulfillmentScanouts = await _shippingScanoutService.GetAsync(x => orderFulfillmentIds.Contains(x.OrderFulfillmentId.Value));
                    ViewData["ScanOutTime"] = fulfillmentScanouts?.CreateDate.ToString() ?? "";
                }
                return View(nameof(Details), dbData);
            }
            //updates the order with new values  
            dbData.weight = inputOrderShipping.weight;
            dbData.dimensions = inputOrderShipping.dimensions;
            dbData.advancedOptions.labelMessageReference1 = inputOrderShipping.orderNumber;
            dbData = await _orderShippingService.GetRateEstimate(dbData);
            // Return the required data as a JSON object  
            return Json(new
            {
                dbData.carrierId,
                dbData.carrierCode,
                dbData.carrierNickname,
                dbData.serviceCode,
                dbData.estimatedShipmentCost,
                dbData.packageCode,
                shipFrom = dbData.shipFrom.ToJson()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        ;

    }


    //[HttpPost]
    //
    //[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    //public async Task<IActionResult> GetRateEstimateBulk(string ERPOrderIdsJson)
    //{
    //    List<int> ERPOrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);

    //    if (ERPOrderIds.Count > 100)
    //    {
    //        //_logger.LogWarning("Too many orders selected for rate estimate bulk action. Count: {orderCount}", ERPOrderIds.Count);
    //        return BadRequest("Due to API limitations, too many orders were selected. Please select a list of orders resulting in less than 100 iterations.");
    //    }

    //    Func<IQueryable<Order>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Order, OrderDimensions>> query = (IQueryable<Order> order) => order.Where(x => ERPOrderIds.Contains(x.ERPOrderId))
    //    .Include(x => x.items)
    //    .ThenInclude(x => x.Product)
    //    .Include(x => x.advancedOptions)
    //    .Include(x => x.shipFrom)
    //    .Include(x => x.shipTo)
    //    .Include(x => x.weight)
    //    .Include(x => x.dimensions);
    //    try
    //    {
    //        List<Order> orders = await _orderService.GetListAsync(query);
    //        foreach (Order orderData in orders)
    //        {
    //            try
    //            {
    //                orderData.advancedOptions.labelMessageReference1 = orderData.orderNumber;
    //                _ = await _orderShippingService.GetRateEstimate(orderData);
    //            }
    //            // Move on to the next order if it is being modified somewhere else.  
    //            catch (DbUpdateConcurrencyException ex)
    //            {
    //                //_logger.LogWarning(ex, "Concurrency conflict occurred for order with ERPOrderId: {ERPOrderId}", orderData.ERPOrderId);
    //                continue;
    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.LogError(ex, "Error occurred while processing rate estimate for order with ERPOrderId: {ERPOrderId}", orderData.ERPOrderId);
    //            }

    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while processing rate estimate bulk action.");
    //        return BadRequest("An error occurred while processing the rate estimate bulk action.");
    //    }

    //    // Store the TempData values for the next request    
    //    SetTempDataValues();

    //    // Redirect back to the Index page    
    //    return RedirectToAction("Index", new
    //    {
    //        orderNumber = ViewData["OrderNumber"],
    //        orderStatus = ViewData["OrderStatus"],
    //        storeId = ViewData["StoreId"],
    //        productId = ViewData["ProductId"],
    //        departmentId = ViewData["DepartmentId"],
    //        orderTagId = ViewData["OrderTagId"],
    //        orderStartDate = ViewData["OrderStartDate"],
    //        orderEndDate = ViewData["OrderEndDate"],
    //        shipByDate = ViewData["ShipByDate"]
    //    });
    //}


    [HttpPost]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    public async Task<IActionResult> AddOrRemoveTag(int ERPOrderId, int[] TagIds, bool addOrRemove)
    {
        List<OrderTag> shipstationTagList = await _orderTagService.GetAllAsync();

        // Perform the bulk action here, using the ERPOrderIdsJson and TagIds arrays
        Order order = await _orderService.GetAsync(x => x.ERPOrderId == ERPOrderId, includes: [o => o.Tags]);
        order.Tags ??= [];
        foreach (int TagId in TagIds)
        {
            _ = await _orderService.AddOrRemoveShipStationTagAsync(order.orderId, TagId, addOrRemove);
            if (addOrRemove)
            {
                order.tagIds = order.tagIds?.Append(TagId).ToArray();
                order.Tags.Add(shipstationTagList.Single(x => x.tagId == TagId));
            }
            else
            {
                OrderTag tagToRemove = shipstationTagList.Single(x => x.tagId == TagId);
                _ = order.Tags.RemoveAll(tag => tag.tagId == tagToRemove.tagId);
            }

            _ = await _orderService.UpdateAsync(order);
        }
        return RedirectToAction(nameof(Details), new { id = ERPOrderId });
    }

    [HttpPost]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
    public async Task<IActionResult> AddOrRemoveTagBulk(string ERPOrderIdsJson, int[] TagIds, bool addOrRemove)
    {
        List<int> ERPOrderIds = JsonConvert.DeserializeObject<List<int>>(ERPOrderIdsJson);

        if (ERPOrderIds.Count * TagIds.Length > 100)
        {
            //_logger.LogWarning("Too many orders or tags selected for tag bulk action. Count: {orderCount}, Tags: {tagCount}", ERPOrderIds.Count, TagIds.Length);
            return BadRequest("Due to API limitations, too many orders or tags selected. Please select a combination of orders and tags that result in less than 100 iterations.");
        }
        List<OrderTag> shipstationTagList = await _orderTagService.GetAllAsync();

        // Perform the bulk action here, using the ERPOrderIdsJson and TagIds arrays
        foreach (int ERPOrderId in ERPOrderIds)
        {
            try
            {
                Func<IQueryable<Order>, IQueryable<Order>> query = (IQueryable<Order> order) => order.Where(x => ERPOrderIds.Contains(x.ERPOrderId));
                Order order = await _orderService.GetAsync(x => x.ERPOrderId == ERPOrderId, includes: [o => o.Tags]);
                foreach (int TagId in TagIds)
                {
                    _ = await _orderService.AddOrRemoveShipStationTagAsync(order.orderId, TagId, addOrRemove);
                    if (addOrRemove)
                    {
                        order.tagIds = order.tagIds?.Append(TagId).ToArray();
                        order.Tags.Add(shipstationTagList.Single(x => x.tagId == TagId));
                    }
                    else
                    {
                        OrderTag tagToRemove = shipstationTagList.Single(x => x.tagId == TagId);
                        _ = order.Tags.RemoveAll(tag => tag.tagId == tagToRemove.tagId);
                    }

                    _ = await _orderService.UpdateAsync(order);
                }

            }
            //Move on to the next order if it is being modified somewhere else.
            catch (DbUpdateConcurrencyException ex)
            {
                //_logger.LogWarning(ex, "Concurrency conflict occurred for order with ERPOrderId: {ERPOrderId}", ERPOrderId);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing add or remove tag bulk action for order with ERPOrderId: {ERPOrderId}", ERPOrderId);
            }
        }

        // Store the TempData values for the next request  
        SetTempDataValues();

        // Redirect back to the Index page  
        return RedirectToAction("Index", new
        {
            orderNumber = ViewData["OrderNumber"],
            orderStatus = ViewData["OrderStatus"],
            storeId = ViewData["StoreId"],
            productId = ViewData["ProductId"],
            departmentId = ViewData["DepartmentId"],
            orderTagId = ViewData["OrderTagId"],
            orderStartDate = ViewData["OrderStartDate"],
            orderEndDate = ViewData["OrderEndDate"],
            shipByDate = ViewData["ShipByDate"]
        });
    }

    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.ShippingBasic)]
    public async Task<JsonResult> GenerateLabelAndShip(
        [Bind("cwaOrderId,orderId,orderKey,orderNumber,weight,dimensions,items")] Order inputOrderShipping,
        bool? ignoreBadAddress
    )
    {
        try
        {
            //Retrieve all the order info for the user  
            Order dbData = await _orderShippingService.GetOrderByOrderIdAndKeyCustomSelectAsync(inputOrderShipping.orderId, inputOrderShipping.orderKey);
         

            if (!ModelState.IsValid)
            {
                ViewData["ScanOutTime"] = "";
                ViewData["BatchTime"] = "";
                if (dbData.orderStatus == OrderStatus.shipped && dbData.orderShipments.Any(os => !os.voided))
                {
                    IEnumerable<int> orderShipmentIds = dbData.orderShipments.Select(os => os.OrderShipmentId);
                    ShippingScanout shipmentScanouts = await _shippingScanoutService.GetAsync(x => orderShipmentIds.Contains(x.OrderShipmentId.Value));
                    ViewData["ScanOutTime"] = shipmentScanouts?.CreateDate.ToString() ?? "";
                }
                else if (dbData.orderStatus == OrderStatus.shipped && dbData.orderFulfillments.Any(of => !of.voided))
                {
                    IEnumerable<int> orderFulfillmentIds = dbData.orderFulfillments.Select(os => os.OrderFulfillmentId);
                    ShippingScanout fulfillmentScanouts = await _shippingScanoutService.GetAsync(x => orderFulfillmentIds.Contains(x.OrderFulfillmentId.Value));
                    ViewData["ScanOutTime"] = fulfillmentScanouts?.CreateDate.ToString() ?? "";
                }
                throw new Exception("Order properties not set correctly");
            }
            // Get the username of the user who just created this label  
            string shippedByUsername = User.Identity.Name;

            // Updates the order with new values  
            dbData.weight = inputOrderShipping.weight;
            dbData.dimensions = inputOrderShipping.dimensions;
            dbData.advancedOptions.labelMessageReference1 = inputOrderShipping.orderNumber;

            var address = await _orderService.GetOrderShipToAddressAsync(dbData.orderId, dbData.orderKey);
            var queryString = address.GenerateUspsValidationQueryString();
            var uspsAddress = await _orderService.UspsAddressValidation(queryString);
            dbData = dbData.Sources.FirstOrDefault().Name switch
            {
                OrderSourceEnum.zazzle => await _orderShippingService.GenerateLabelZazzle(dbData, shippedByUsername),
                OrderSourceEnum.shipstation or OrderSourceEnum.orderdesk or OrderSourceEnum.custom or OrderSourceEnum.skulabs => await _orderShippingService.GenerateLabelShipEngine(dbData, shippedByUsername),
                _ => throw new NotImplementedException()
            };

            if (dbData.orderShipments.Any())
            {
                string base64Data = dbData.orderShipments.OrderByDescending(s => s.createDate).FirstOrDefault()?.labelData["data:application/pdf;base64,".Length..];
                string filename = $"Labels-{dbData.orderNumber}.pdf";
                await _orderBatchService.UpdateOrderBatchItemsStatusToCompletedAsync(dbData.orderNumber);
                // Need to have a check here that checks this orderNumber in each batch. If it is, then set the OrderBatchItems that contain that orderNumber to set their Status
                return Json(new { success = true, labelData = base64Data, filename });
            }
            return Json(new { success = false, error = "Label generation failed" });

        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ProductionBasic + "," + RoleList.ShippingBasic + "," + RoleList.Manager)]
    public async Task<IActionResult> FindMissingOrder(string orderNumber, int storeId, int? tagId)
    {
        if (tagId != null && string.IsNullOrEmpty(orderNumber) && storeId == 0)
        {
            var orders = await _orderService.GetShipStationOrdersWithTag((int)tagId);
            await _orderService.ProcessOrderNotify(orders);
            return Redirect(nameof(Index));
        }
        ShipStationStore selectedShipStationStore = _shipstationStoreService.Get(x => x.ShipStationStoreId == storeId);

        if (selectedShipStationStore == null)
        {
            //_logger.LogWarning("ShipStationStore not found for storeId: {0}", storeId);
            return NotFound("ShipStationStore not found.");
        }

        Order missingOrder = await _orderService.FindMissingOrder(orderNumber, selectedShipStationStore.StoreId);

        return RedirectToAction(nameof(Details), new { id = missingOrder.ERPOrderId });
    }

    public async Task<IActionResult> StraightToOrder([Bind("orderNumber")] string orderNumber)
    {
        if (string.IsNullOrEmpty(orderNumber))
        {
            return RedirectToAction(nameof(Index));
        }
        Order order = await _orderService.GetAsNoTrackingAsync(x => x.orderNumber == orderNumber);
        if (order == null)
        {
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Details), new { id = order.ERPOrderId });
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        List<OrderTag> shipstationTagList = await _orderTagService.GetAllAsync();
        ViewBag.Tags = new SelectList(shipstationTagList.Select(x => new { TagId = x.tagId, TagName = x.name }).OrderBy(x => x.TagName).AsEnumerable(), "TagId", "TagName");

        ViewData["ScanOutTime"] = "";
        ViewData["BatchTime"] = "";

        Order order = (await _orderService.GetOrderUpdates([id.Value])).SingleOrDefault();
        if (order == null)
        {
            return NotFound();
        }

        if (order.orderStatus == OrderStatus.shipped && order.orderShipments.Any(os => !os.voided))
        {
            IEnumerable<int> orderShipmentIds = order.orderShipments.Select(os => os.OrderShipmentId);
            ShippingScanout shipmentScanouts = await _shippingScanoutService.GetAsync(x => orderShipmentIds.Contains(x.OrderShipmentId.Value));
            ViewData["ScanOutTime"] = shipmentScanouts?.CreateDate.ToString() ?? "";
        }
        else if (order.orderStatus == OrderStatus.shipped && order.orderFulfillments.Any(of => !of.voided))
        {
            IEnumerable<int> orderFulfillmentIds = order.orderFulfillments.Select(os => os.OrderFulfillmentId);
            ShippingScanout fulfillmentScanouts = await _shippingScanoutService.GetAsync(x => orderFulfillmentIds.Contains(x.OrderFulfillmentId.Value));
            ViewData["ScanOutTime"] = fulfillmentScanouts?.CreateDate.ToString() ?? "";
        }

        var batchNumberList = await _orderBatchService.GetOrderBatchNumberByOrderId(order.ERPOrderId);
        if (batchNumberList != null && batchNumberList.TryGetValue(order.ERPOrderId, out var batchNumbers) && batchNumbers.Any())
        {
            ViewData["BatchNumbers"] = batchNumbers;
        }

        var orderBatchItem = await _orderBatchService.GetOrderBatchItemByERPOrderId(id.Value);

        if (orderBatchItem != null)
        {
            ViewData["BatchTime"] = orderBatchItem.OrderBatch.CreateDate.ToString();
        }

        //get stores
        List<ShipStationStore> storesList = await _shipstationStoreService.GetAllAsync();
        ViewBag.Stores = new SelectList(storesList.Select(x => new { x.StoreId, x.StoreName }).OrderBy(x => x.StoreName).AsEnumerable(), "StoreId", "StoreName", order.advancedOptions.storeId);

        decimal totalWeightInOunces = 0;
        decimal totalVolume = 0;
        List<string> zeroWeightSkus = [];
        List<string> zeroVolumeSkus = [];

        foreach (var item in order.items)
        {
            // Getting weight and volume information by performing the necessary calculations using data from the products themselves.
            decimal weightInOunces = 0;
            if (item.Product != null && item.ERPBundleId == null)
            {
                weightInOunces = order.weight.value;
                if (item.Product.ShippingWeightUnit == WeightUnit.Ounce)
                {
                    weightInOunces = item.Product.ShippingWeightAmount;
                }
                else if (item.Product.ShippingWeightUnit == WeightUnit.Pound)
                {
                    weightInOunces = item.Product.ShippingWeightAmount * 16;
                }
                decimal productVolume = item.Product.ShippingHeight * item.Product.ShippingLength * item.Product.ShippingWidth;

                // If weight or volume is at 0, that means we need to throw an error since the product isn't set up correctly.
                if (weightInOunces == 0)
                {
                    zeroWeightSkus.Add(item.Product.Sku);
                }

                if (productVolume == 0)
                {
                    zeroVolumeSkus.Add(item.Product.Sku);
                }
                totalWeightInOunces += weightInOunces * item.quantity;
                totalVolume += productVolume * item.quantity;
            }
            else if (item.ERPBundleId != null && item.Bundle != null)
            {
                foreach (var bundleItem in item.Bundle.BundleItems)
                {
                    if (bundleItem.Product != null)
                    {
                        weightInOunces = order.weight.value;
                        if (bundleItem.Product.ShippingWeightUnit == WeightUnit.Ounce)
                        {
                            weightInOunces = bundleItem.Product.ShippingWeightAmount;
                        }
                        else if (bundleItem.Product.ShippingWeightUnit == WeightUnit.Pound)
                        {
                            weightInOunces = bundleItem.Product.ShippingWeightAmount * 16;
                        }
                        decimal productVolume = bundleItem.Product.ShippingHeight * bundleItem.Product.ShippingLength * bundleItem.Product.ShippingWidth;

                        if (weightInOunces == 0)
                        {
                            zeroWeightSkus.Add(bundleItem.Product.Sku);
                        }
                        if (productVolume == 0)
                        {
                            zeroVolumeSkus.Add(bundleItem.Product.Sku);
                        }
                        totalWeightInOunces += weightInOunces * bundleItem.Quantity;
                        totalVolume += productVolume * bundleItem.Quantity;
                    }
                }
            }
        }

        if (totalWeightInOunces < 1)
        {
            totalWeightInOunces = 1;
        }

        ViewData["OrderWeight"] = totalWeightInOunces;
        ViewData["OrderVolume"] = totalVolume;
        ViewData["ZeroWeightSkus"] = zeroWeightSkus;
        ViewData["ZeroVolumeSkus"] = zeroVolumeSkus;

        order.weight.units = OrderWeight.Units.ounces;

        return View(order);
    }


    //get product details by product id
    [HttpGet]
    public async Task<ActionResult<DuplicateOrderItemDTO>> GetProductBy(int productId)
    {
        var product = await _productService.GetAsync(p => p.ProductId == productId);

        if (product == null)
        {
            return null;
        }

        var dupProductDTO = new DuplicateOrderItemDTO
        {
            Quantity = 1,
            Sku = product.Sku,
            Name = product.Description,
            UnitPrice = product.Cost + product.FulfillmentCost + product.LaborCost + product.OverseasCost,
            ProductId = product.ProductId,
            Product = product
        };

        //set image url
        if (product.ProductImages != null && product.ProductImages.Any())
        {
            var image = product.ProductImages.FirstOrDefault(i => i.IsDefault);

            if (image != null)
            {
                dupProductDTO.ImageUrl = image.FileUrl;
            }
        }

        return Ok(dupProductDTO);
    }

    [HttpPost, ActionName("Void")]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.Manager)]
    public async Task<IActionResult> VoidAsync([Bind("OrderShipmentId,ERPOrderId,ShipEngineShipmentId")] OrderShipment orderShipment)
    {
        // Retrieve the existing OrderShipment from the database  
        orderShipment = await _orderShippingService.GetAsync(x => x.OrderShipmentId == orderShipment.OrderShipmentId);
        OrderFulfillment existingOrderFulfillment = await _orderFulfillmentService.GetAsync(x => x.trackingNumber == orderShipment.trackingNumber);
        if (orderShipment != null)
        {
            if (existingOrderFulfillment != null && !string.IsNullOrEmpty(orderShipment.ShipEngineShipmentId))
            {
                var result = await _orderFulfillmentService.VoidAsync(orderShipment.ERPOrderId, existingOrderFulfillment);
            }
            else if (existingOrderFulfillment != null && (orderShipment.carrierCode.ToLower() == "usps" || orderShipment.carrierCode.ToLower() == "stamps.com"))
            {
                var result = await _orderShippingService.VoidUspsLabel(orderShipment.trackingNumber);
                if (!result)
                {
                    _logger.LogError("Failed to void USPS label for OrderShipmentId: {OrderShipmentId}", orderShipment.OrderShipmentId);
                }
            }

            await _orderShippingService.VoidAsync(orderShipment.ERPOrderId, orderShipment);
        }
        else
        {
            //_logger.LogWarning("OrderShipment not found.");
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id = orderShipment.ERPOrderId });

    }

    [HttpPost, ActionName("VoidFulfillment")]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.ShippingManager + "," + RoleList.Manager)]
    public async Task<IActionResult> VoidAsync([Bind("OrderFulfillmentId,ERPOrderId")] OrderFulfillment orderFulfillment)
    {
        try
        {
            // Retrieve the existing OrderFulfillment from the database  
            OrderFulfillment existingOrderFulfillment = await _orderFulfillmentService.GetAsync(x => x.OrderFulfillmentId == orderFulfillment.OrderFulfillmentId);

            if (existingOrderFulfillment != null)
            {
                // Validate the properties  
                if (existingOrderFulfillment.ERPOrderId != orderFulfillment.ERPOrderId)
                {
                    //_logger.LogWarning("The received data is not valid.");
                    return BadRequest("The received data is not valid.");
                }

                // Continue with the VoidAsync method if the properties match  
                Order result = await _orderFulfillmentService.VoidAsync(orderFulfillment.ERPOrderId, existingOrderFulfillment);

                if (result == null)
                {
                    _logger.LogError("Error occurred while voiding shipment.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while voiding the shipment.");
                }
            }
            else
            {
                //_logger.LogWarning("OrderFulfillment not found.");
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = orderFulfillment.ERPOrderId });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred while voiding fulfillment.");
            return Conflict("A concurrency conflict occurred while voiding the fulfillment.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while voiding fulfillment.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while voiding the fulfillment.");
        }
    }

    private static string GetSortColumn(int orderColumn) =>
        orderColumn switch
        {
            1 => "orderNumber",
            2 or 3 => "orderDate",
            4 => "shipByDate",
            6 => "orderStatus",
            7 => "orderTotal",
            8 => "advancedOptions.storeName",
            9 => "shipDate",
            10 => "estimatedShipmentCost",
            11 => "items.Where(x=>x.sku !=\"\").FirstOrDefault().name",
            _ => "orderNumber"
        };

    private void SetTempDataValues()
    {
        TempData["OrderNumber"] = ViewData["OrderNumber"];
        TempData["OrderStatus"] = ViewData["OrderStatus"];
        TempData["StoreId"] = ViewData["StoreId"];
        TempData["ProductId"] = ViewData["ProductId"];
        TempData["DepartmentId"] = ViewData["DepartmentId"];
        TempData["OrderTagId"] = ViewData["OrderTagId"];
        TempData["OrderStartDate"] = ViewData["OrderStartDate"];
        TempData["OrderEndDate"] = ViewData["OrderEndDate"];
        TempData["ShipByDate"] = ViewData["ShipByDate"];
        TempData["AutoEnterDetails"] = ViewData["AutoEnterDetails"];
        TempData["IncludeBatchedOrders"] = ViewData["IncludeBatchedOrders"];
    }

    private async Task PrepareDuplicateOrderData(Order order)
    {
        //get stores
        List<ShipStationStore> storesList = await _shipstationStoreService.GetAllAsync();
        ViewBag.Stores = new SelectList(storesList.Select(x => new { x.StoreId, x.StoreName }).OrderBy(x => x.StoreName).AsEnumerable(), "StoreId", "StoreName");

        //get order statuses
        var values = Enum.GetValues(typeof(OrderStatus))
                 .Cast<OrderStatus>()
                 .Where(e => e != OrderStatus.shipped && e != OrderStatus.cancelled)
                 .Select(e => new
                 {
                     Value = (int)e,
                     Text = e.GetType()
                             .GetMember(e.ToString())
                             .First()
                             .GetCustomAttributes<DisplayAttribute>()
                             .First()
                             .Name
                 });
        var selectList = new SelectList(values, "Value", "Text");
        ViewBag.OrderStatusList = selectList;

        //get order tags
        List<OrderTag> tagsList = await _orderService.GetShipStationTags();
        ViewBag.OrderTags = new SelectList(tagsList.Select(x => new { x.tagId, x.name }).OrderBy(x => x.name).AsEnumerable(), "tagId", "name");
    }

    private static (string statusClass, string statusIcon) GetOrderStatusDetails(OrderStatus orderStatus)
    {
        string statusClass;
        string statusIcon;

        switch (orderStatus)
        {
            case OrderStatus.awaiting_shipment:
                statusClass = "<span class=\"badge badge-info-lighten\">";
                statusIcon = "<i class=\"mdi mdi-timer-sand\"></i>";
                break;
            case OrderStatus.shipped:
                statusClass = "<span class=\"badge badge-success-lighten\">";
                statusIcon = "<i class=\"mdi mdi-truck-delivery\"></i>";
                break;
            case OrderStatus.cancelled:
                statusClass = "<span class=\"badge badge-danger-lighten\">";
                statusIcon = "<i class=\"mdi mdi-cancel\"></i>";
                break;
            case OrderStatus.on_hold:
                statusClass = "<span class=\"badge badge-warning-lighten\">";
                statusIcon = "<i class=\"mdi mdi-timer-sand-paused\"></i>";
                break;
            case OrderStatus.awaiting_payment:
                statusClass = "<span class=\"badge badge-secondary-lighten\">";
                statusIcon = "<i class=\"mdi mdi-cash\"></i>";
                break;
            default:
                statusClass = "";
                statusIcon = "";
                break;
        }

        return (statusClass, statusIcon);
    }
}
