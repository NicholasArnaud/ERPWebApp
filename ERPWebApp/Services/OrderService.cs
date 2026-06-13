using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.Reports;
using ERPWebApp.Services.IServices;

using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

using static ERPWebApp.Data.DTOModels.ZazzleDTO.ZazzleRequest.Response.Result;
using static ERPWebApp.Models.Orders.Order;
using static ERPWebApp.Models.Orders.OrderItem;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Globalization;

namespace ERPWebApp.Services;

/// <summary>  
/// Implementation for the OrderService Interface, providing methods for managing Order, OrderShipment, and OrderFulfillment entities.  
/// </summary>  
public class OrderService(
    IUnitOfWork unitOfWork,
    IWebhooks webhooks,
    IHttpClientFactory httpClientFactory,
    ILogger<OrderService> logger,
    IOrderItemService orderItemService,
    IOrderBatchService orderBatchService
) : Service<Order>(unitOfWork), IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IWebhooks _webhooks = webhooks;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<OrderService> _logger = logger;
    private readonly IOrderItemService _orderItemService = orderItemService;
    private readonly IOrderBatchService _orderBatchService = orderBatchService;
    private readonly DateTime _now = TimeZoneInfo.ConvertTime(
            DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };
    /// <summary>  
    /// Processes the 'ORDER_NOTIFY' webhook payload.  
    /// </summary>  
    /// <param name="orders">The list of orders to process.</param>
    public async Task ProcessOrderNotify(List<Order> orders)
    {
        try
        {
            var allTagIds = orders
                .Where(order => order?.tagIds != null)
                .SelectMany(order => order?.tagIds).ToHashSet();
            var existingTagIds = _unitOfWork.OrderTags
                .GetListByFilter(t => allTagIds.Contains(t.tagId))
                .Select(t => t.tagId)
                .ToHashSet();
            foreach (Order order in orders)
            {

                if (order?.tagIds == null || !existingTagIds.Overlaps(order.tagIds))
                {
                    List<OrderTag> tags = await GetShipStationTags();

                    // Create a HashSet for faster lookups
                    HashSet<int> orderTagIds = order?.tagIds?.ToHashSet() ?? [];

                    // Filter tags based on orderTagIds
                    List<OrderTag> tagList = [.. tags.Where(x => orderTagIds.Contains(x.tagId))];

                    // Find new tags by comparing with existingTagIds
                    var newTags = tagList
                        .Where(t => !existingTagIds.Contains(t.tagId))
                        .ToList();

                    // Add new tags to the database
                    _unitOfWork.OrderTags.AddRange(newTags);
                    existingTagIds.UnionWith(newTags.Select(t => t.tagId));
                }

                order.Tags = await _unitOfWork.OrderTags.GetListByFilterAsync(x => order.tagIds.Contains(x.tagId));
                order.requestedShippingService = order.requestedShippingService?[..Math.Min(order.requestedShippingService.Length, 50)];
                OrderSource shipstationSource = _unitOfWork.OrderSource.FilterOne(x => x.Name == OrderSourceEnum.shipstation);
                Order existingOrder = await _unitOfWork.Orders.GetOrderByOrderIdAndKeyCustomSelectAsync(order.orderId, order.orderKey);

                //to identify the duplicated orders if the order is not saved through ship station API response
                if (existingOrder == null && !string.IsNullOrEmpty(order.orderNumber))
                {
                    bool isDuplicate = await IsExistsAsync(o => o.orderNumber == order.orderNumber && o.IsDuplicated != null && o.IsDuplicated.Value);

                    //if the order is a duplicate order, skip
                    if (isDuplicate) { continue; }
                }
                //if the order is a duplicate order and order is already saved from ship station API response, skip
                if (existingOrder != null && existingOrder.IsDuplicated != null && existingOrder.IsDuplicated.Value)
                {
                    continue;
                }

                try
                {
                    if (existingOrder == null)
                    {
                        existingOrder = order;
                        existingOrder.shipByDate = _now;
                        existingOrder.Sources.Add(shipstationSource);
                        string storeName = await _unitOfWork.ShipStationStores.GetStoreNameById(order.advancedOptions.storeId);
                        existingOrder.advancedOptions.storeName = storeName;
                        existingOrder = Add(existingOrder);
                    }
                    else
                    {
                        if (!existingOrder.Sources.Contains(shipstationSource))
                        {
                            existingOrder.Sources.Add(shipstationSource);
                        }
                        string storeName = await _unitOfWork.ShipStationStores.GetStoreNameById(order.advancedOptions.storeId);
                        existingOrder.Tags = order.Tags;
                        existingOrder.tagIds = order.tagIds;
                        existingOrder.orderStatus = order.orderStatus;
                        existingOrder.advancedOptions = order.advancedOptions;
                        existingOrder.advancedOptions.storeName = storeName;
                        existingOrder.customerNotes = order.customerNotes;
                        existingOrder.internalNotes = order.internalNotes;
                        existingOrder.customerEmail = order.customerEmail;
                        existingOrder.shipTo = order.shipTo;
                        //Get ShipStations order item ids
                        var shipstationOrderItemIds = order.items.Select(x => x.orderItemId).ToList();
                        //Check if any items are missing from the existing order
                        if (existingOrder.items.Any(x => !shipstationOrderItemIds.Contains(x.orderItemId)))
                        {
                            List<OrderItem> itemsToUpdate = [];
                            //Iterate over all ShipStation Items
                            order.items.ForEach(x =>
                            {
                                //Get pre-existing item from the database
                                var shipstationMatchedItem = existingOrder.items.FirstOrDefault(y => y.lineItemKey == x.lineItemKey);
                                if (shipstationMatchedItem != null)
                                {
                                    x.ERPBundleId = shipstationMatchedItem.ERPBundleId;
                                    x.ERPProductId = shipstationMatchedItem.ERPProductId;
                                    x.ERPOrderItemId = shipstationMatchedItem.ERPOrderItemId;
                                    x.ERPOrderId = shipstationMatchedItem.ERPOrderId;
                                    itemsToUpdate.Add(x);
                                }
                                else
                                {
                                    x.ERPOrderId = existingOrder.ERPOrderId;
                                    itemsToUpdate.AddRange(x);
                                }
                            });
                            existingOrder.items = itemsToUpdate;
                        }
                        await UpdateAsync(existingOrder);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _orderItemService.AssignProductIds(existingOrder.items);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Database update exception occurred for {Order} in ProcessOrderNotify: {Message}", order.orderNumber, ex.Message);
                    continue;
                }
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("Database update exception occurred in ProcessOrderNotify: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Invalid operation exception occurred in ProcessOrderNotify: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred in ProcessOrderNotify: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>  
    /// Processes the 'ITEM_SHIP_NOTIFY' webhook payload.  
    /// </summary>  
    /// <param name="shipments">The list of shipments to process.</param>
    public async Task ProcessItemShipNotify(List<OrderShipment> shipments)
    {
        try
        {
            List<Order> newOrders = [];

            foreach (OrderShipment shipment in shipments)
            {
                //Adjust DateTime Values

                //ShipStation always sends the shipDate as a Date, not a DateTime.
                //ShipStation sets createDate in PST which is 2 hours behind CST.
                shipment.shipDate = shipment.createDate.AddHours(2);

                Order existingOrder = await _unitOfWork.Orders.GetOrderByOrderIdAndKeyCustomSelectAsync(shipment.orderId, shipment.orderKey);
                OrderSource shipstationSource = _unitOfWork.OrderSource.FilterOne(x => x.Name == OrderSourceEnum.shipstation);
                // Retrieve the order from ShipStation API
                Order updatedOrder = shipment.Order ?? await GetShipStationOrderByOrderId(shipment.orderId);

                if (existingOrder == null)
                {
                    // If the order still does not exist, continue with the next shipment    
                    if (existingOrder == null)
                    {
                        continue;
                    }
                    existingOrder = updatedOrder;

                    existingOrder.Sources.Add(shipstationSource);

                    existingOrder.dimensions ??= new OrderDimensions();
                    existingOrder.weight ??= new OrderWeight();
                    await _unitOfWork.Orders.AddAsync(existingOrder);
                }
                else
                {
                    //Get ShipStations order item ids
                    var shipstationOrderItemIds = updatedOrder.items.Select(x => x.orderItemId).ToList();
                    //Check if any items are missing from the existing order
                    if (existingOrder.items.Any(x => !shipstationOrderItemIds.Contains(x.orderItemId)))
                    {
                        List<OrderItem> itemsToUpdate = [];
                        //Iterate over all ShipStation Items
                        updatedOrder.items.ForEach(x =>
                        {
                            //Get pre-existing item from the database
                            var shipstationMatchedItem = existingOrder.items.FirstOrDefault(y => y.lineItemKey == x.lineItemKey);
                            if (shipstationMatchedItem != null)
                            {
                                x.ERPBundleId = shipstationMatchedItem.ERPBundleId;
                                x.ERPProductId = shipstationMatchedItem.ERPProductId;
                                x.ERPOrderItemId = shipstationMatchedItem.ERPOrderItemId;
                                x.ERPOrderId = shipstationMatchedItem.ERPOrderId;
                                itemsToUpdate.Add(x);
                            }
                            else
                            {
                                x.ERPOrderId = existingOrder.ERPOrderId;
                                itemsToUpdate.AddRange(x);
                            }
                        });
                        existingOrder.items = itemsToUpdate;
                    }
                }
                if (!existingOrder.Sources.Contains(shipstationSource))
                {
                    existingOrder.Sources.Add(shipstationSource);
                }

                updatedOrder.advancedOptions.storeName = await _unitOfWork.ShipStationStores.GetStoreNameById(existingOrder.advancedOptions.storeId);
                shipment.advancedOptions.storeName = updatedOrder.advancedOptions.storeName;

                // Check if an existing shipment with the same shipmentId already exists for the order  
                OrderShipment existingShipment = existingOrder.orderShipments?.FirstOrDefault(os => os.shipmentId == shipment.shipmentId);

                existingOrder.shipDate = shipment.shipDate;
                existingOrder.shipFrom = shipment.shipFrom;
                existingOrder.orderStatus = OrderStatus.shipped;
                existingOrder.userId = shipment.userId;
                existingOrder.estimatedShipmentCost = shipment.shipmentCost;
                existingOrder.customerNotes = updatedOrder.customerNotes?.Normalize();
                existingOrder.internalNotes = updatedOrder.internalNotes?.Normalize();
                existingOrder.dimensions = shipment.dimensions ?? updatedOrder.dimensions;
                updatedOrder.advancedOptions.ERPOrderId = existingOrder.ERPOrderId;
                existingOrder.advancedOptions = updatedOrder.advancedOptions;
                await _orderBatchService.UpdateOrderBatchItemsStatusToCompletedAsync(existingOrder.orderNumber);
                Update(existingOrder);

                if (existingOrder.advancedOptions.mergedOrSplit)
                {
                    foreach (long mergedId in existingOrder.advancedOptions.mergedIds)
                    {
                        Order combinedOrder = await _unitOfWork.Orders.GetOrderByOrderIdCustomSelectNoTrackingAsync(mergedId);
                        Order ShipstationOrder = await GetShipStationOrderByOrderId(mergedId);
                        if (combinedOrder != null && ShipstationOrder == null)
                        {
                            await _unitOfWork.Orders.DeleteAsync(combinedOrder.ERPOrderId);
                        }
                        else if (ShipstationOrder != null && combinedOrder == null)
                        {

                            newOrders.Add(ShipstationOrder);
                        }
                    }
                }
                if (existingShipment == null)
                {
                    List<OrderItem> appliedItems = await GetAppliedItemsAsync(shipment);
                    OrderShipment newShipment = PrepareShipmentFromExistingOrder(existingOrder, shipment, appliedItems);
                    //await UpdateShipmentWithOsmEstimatesAsync(_webhooks, newShipment);
                    _unitOfWork.OrderShipments.Update(newShipment);
                }
                await _unitOfWork.SaveChangesAsync();
                await _orderItemService.AssignProductIds(existingOrder.items);
            }
            if (newOrders.Count > 0)
            {
                await ProcessOrderNotify(newOrders);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("Database update exception occurred in ProcessItemShipNotify: {Message}", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Invalid operation exception occurred in ProcessItemShipNotify: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred in ProcessItemShipNotify: {Message}", ex.Message);
        }
    }

    /// <summary>  
    /// Processes the 'FULFILLMENT_SHIPPED' webhook payload.  
    /// </summary>  
    /// <param name="fulfillments">The list of fulfillments to process.</param>
    public async Task ProcessFulfillmentShipped(List<OrderFulfillment> fulfillments)
    {
        try
        {
            List<Order> newOrders = [];
            CustomShippingDirector director = new();
            CustomShippingBuilder builder = new();
            foreach (OrderFulfillment fulfillment in fulfillments)
            {
                //Adjust DateTime Values

                //ShipStation always sends the shipDate as a Date, not a DateTime.
                //ShipStation sets createDate in PST which is 2 hours behind CST.
                fulfillment.shipDate = fulfillment.createDate.AddHours(2);

                Order existingOrder = await _unitOfWork.Orders.GetOrderByOrderIdCustomSelectAsync(fulfillment.orderId);
                OrderSource shipstationSource = _unitOfWork.OrderSource.FilterOne(x => x.Name == OrderSourceEnum.shipstation);

                if (existingOrder == null)
                {
                    // Retrieve the order from ShipStation API  
                    existingOrder = await GetShipStationOrderByOrderId(fulfillment.orderId);

                    // If the order still does not exist, continue with the next fulfillment  
                    if (existingOrder == null)
                    {
                        continue;
                    }
                    existingOrder.Sources.Add(shipstationSource);

                    Add(existingOrder);
                    await _unitOfWork.SaveChangesAsync();
                    await _orderItemService.AssignProductIds(existingOrder.items);
                }
                else
                {
                    if (!existingOrder.Sources.Contains(shipstationSource))
                    {
                        existingOrder.Sources.Add(shipstationSource);
                    }
                    Order updatedOrder = fulfillment.Order ?? await GetShipStationOrderByOrderId(fulfillment.orderId);

                    if (updatedOrder == null)
                    {
                        continue;
                    }
                    existingOrder.shipFrom = updatedOrder.shipFrom;
                    existingOrder.shipTo = updatedOrder.shipTo;
                    existingOrder.customerNotes = updatedOrder.customerNotes?.Normalize();
                    existingOrder.internalNotes = updatedOrder.internalNotes?.Normalize();
                    existingOrder.shipTo.residential = true; //Set residential to true this will help enforce a higher shipment estimate.
                    existingOrder.billTo = updatedOrder.billTo;
                    existingOrder.orderFulfillments = updatedOrder.orderFulfillments;
                    existingOrder.weight = updatedOrder.weight ?? new OrderWeight();
                    existingOrder.dimensions = updatedOrder.dimensions ?? new OrderDimensions();
                    //Get ShipStations order item ids
                    var shipstationOrderItemIds = updatedOrder.items.Select(x => x.orderItemId).ToList();
                    //Check if any items are missing from the existing order
                    if (existingOrder.items.Any(x => !shipstationOrderItemIds.Contains(x.orderItemId)))
                    {
                        List<OrderItem> itemsToUpdate = [];
                        //Iterate over all ShipStation Items
                        updatedOrder.items.ForEach(x =>
                        {
                            //Get pre-existing item from the database
                            var shipstationMatchedItem = existingOrder.items.FirstOrDefault(y => y.lineItemKey == x.lineItemKey);
                            if (shipstationMatchedItem != null)
                            {
                                x.ERPBundleId = shipstationMatchedItem.ERPBundleId;
                                x.ERPProductId = shipstationMatchedItem.ERPProductId;
                                x.ERPOrderItemId = shipstationMatchedItem.ERPOrderItemId;
                                x.ERPOrderId = shipstationMatchedItem.ERPOrderId;
                                itemsToUpdate.Add(x);
                            }
                            else
                            {
                                x.ERPOrderId = existingOrder.ERPOrderId;
                                itemsToUpdate.AddRange(x);
                            }
                        });
                        existingOrder.items = itemsToUpdate;
                    }

                    // Update the order properties  
                    existingOrder.shipDate = fulfillment.shipDate;
                    existingOrder.orderStatus = OrderStatus.shipped;
                    existingOrder.userId = fulfillment.userId;
                    string storeName = await _unitOfWork.ShipStationStores.GetStoreNameById(existingOrder.advancedOptions.storeId);
                    existingOrder.advancedOptions.storeName = storeName;

                    //If has any non-voided shipments or estimatedShipmentCost is zero, get the rate estimate
                    if (existingOrder.orderShipments == null || existingOrder.estimatedShipmentCost == 0)
                    {
                        try
                        {

                            CustomShipping customShipping = director.Construct(builder, existingOrder);
                            existingOrder.shipFrom = customShipping.OrderShippingInfo;
                            List<string> carrierIds = [.. customShipping.AppliedShipperIds.Where(x => x.Value == CustomShipping.ShipperApi.ShipEngine).Select(x => x.Key)];
                            List<ShipEngineShippingEstimate> returnedEstimateInfo = await GetShipEngineEstimatedShipmentRate(carrierIds, existingOrder);
                            string shipEngineCarrierCode = (fulfillment.carrierCode) switch
                            {
                                "USPS" => "stamps_com",
                                "DHL eCommerce" or "DHL Express" => "endicia",
                                "FedEx" => "fedex",
                                "UPS" => "ups",
                                _ => fulfillment.carrierCode,
                            };
                            //Remove estimates that do not have a shipping amount  
                            _ = returnedEstimateInfo.RemoveAll(x => !x.shipping_amount.HasValue);
                            builder.RemoveInvalidShippingServicesByShippingEstimates(returnedEstimateInfo);
                            var selectedCheapestRate = returnedEstimateInfo.Where(x => x.carrier_code == shipEngineCarrierCode).OrderBy(x => x.shipping_amount.Value.amount).FirstOrDefault();
                            if (selectedCheapestRate == null)
                            {
                                //_logger.LogWarning("No rate estimate found for {Order} with carrier {CarrierCode}", existingOrder.orderNumber, fulfillment.carrierCode);
                            }
                            else
                            {
                                existingOrder.estimatedShipmentCost = selectedCheapestRate.shipping_amount.Value.amount;
                                existingOrder.serviceCode = selectedCheapestRate.service_code;
                            }
                            existingOrder.carrierCode = fulfillment.carrierCode;

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("API Rate Estimate Call failed for: {Order}, See exception: {Message}", existingOrder.orderNumber, ex.Message);
                        }
                    }

                    if (existingOrder.advancedOptions.mergedOrSplit)
                    {
                        foreach (long mergedId in existingOrder.advancedOptions.mergedIds)
                        {
                            Order combinedOrder = await _unitOfWork.Orders.GetOrderByOrderIdCustomSelectNoTrackingAsync(mergedId);
                            Order ShipstationOrder = await GetShipStationOrderByOrderId(mergedId);
                            if (combinedOrder != null && ShipstationOrder == null)
                            {
                                await _unitOfWork.Orders.DeleteAsync(combinedOrder.ERPOrderId);
                            }
                            else if (ShipstationOrder != null && combinedOrder == null)
                            {
                                newOrders.Add(ShipstationOrder);
                            }
                        }
                    }
                    // Check for existing fulfillments to avoid duplicates  
                    OrderFulfillment existingFulfillment = existingOrder.orderFulfillments
                        ?.FirstOrDefault(ef => ef.trackingNumber == fulfillment.trackingNumber);
                    if (existingFulfillment == null)
                    {
                        // Update the fulfillment properties  
                        fulfillment.ERPOrderId = existingOrder.ERPOrderId;
                        await _unitOfWork.OrderFulfillments.AddAsync(fulfillment);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _orderItemService.AssignProductIds(existingOrder.items);
                    await _orderBatchService.UpdateOrderBatchItemsStatusToCompletedAsync(fulfillment.orderNumber);
                }
                if (newOrders.Count > 0)
                    await ProcessOrderNotify(newOrders);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("Database update exception occurred in ProcessFulfillmentShipped: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Invalid operation exception occurred in ProcessFulfillmentShipped: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred in ProcessFulfillmentShipped: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Grabs the first order in the database by order number
    /// </summary>
    /// <param name="orderNumber">The Order Number to search by</param>
    /// <returns>The base Order object</returns>
    public async Task<Order> GetOrderByOrderNumberAsync(string orderNumber) => await _unitOfWork.Orders.GetByQueryAsync(x =>
           x.Include(x => x.advancedOptions)
           .Include(x => x.Tags)
           .Include(x => x.dimensions)
           .Include(x => x.insuranceOptions)
           .Include(x => x.shipFrom)
           .Include(x => x.shipTo)
           .Include(x => x.billTo)
           .Include(x => x.orderFulfillments)
           .Include(x => x.orderShipments)
           .Include(x => x.Sources)
           .Include(x => x.items)
               .ThenInclude(item => item.Bundle)
               .ThenInclude(bundle => bundle.BundleItems)
               .ThenInclude(bundleItem => bundleItem.Product)
           .Include(order => order.items)
               .ThenInclude(items => items.Product)
               .ThenInclude(products => products.Departments)
           .Include(x => x.items)
               .ThenInclude(xx => xx.options)
   .Where(order => order.orderNumber == orderNumber || order.orderKey == orderNumber));

    /// <summary>  
    /// Retrieves the applied items for a shipment.  
    /// </summary>   
    /// <param name="shipment">The OrderShipment instance to get the applied items for.</param>  
    /// <returns>A list of applied OrderItem instances.</returns>
    private async Task<List<OrderItem>> GetAppliedItemsAsync(OrderShipment shipment)
    {
        List<OrderItem> appliedItems = [];

        if (shipment.shipmentItems != null)
        {
            foreach (OrderItem shipmentItem in shipment.shipmentItems)
            {
                if (shipmentItem.ERPProductId == default)
                {
                    Models.Inventory.Product cwaProduct = await _unitOfWork.Products.FilterOneAsync(
                        p => p.IsActive && p.Departments.Any(d => d.IsProduction),
                        [
                            p => p.Departments
                        ]
                    );


                    if (cwaProduct != null)
                    {
                        shipmentItem.ERPProductId = cwaProduct.ProductId;
                    }
                }

                List<OrderItem> orderItems = await _unitOfWork.OrderItems.GetListByFilterAsync(x => x.orderItemId == shipmentItem.orderItemId);
                appliedItems.AddRange(orderItems);
            }
        }

        return appliedItems;
    }

    /// <summary>  
    /// Creates a new OrderShipment instance from an existing order and shipment details.  
    /// </summary>  
    /// <param name="existingOrder">The existing order to create a shipment for.</param>  
    /// <param name="shipment">The shipment details to apply to the order.</param>  
    /// <param name="appliedItems">The list of applied OrderItem instances for the shipment.</param>  
    /// <returns>A new OrderShipment instance.</returns>
    private static OrderShipment PrepareShipmentFromExistingOrder(Order existingOrder, OrderShipment shipment, List<OrderItem> appliedItems)
    {
        OrderShipment newShipment = new()
        {
            ERPOrderId = existingOrder.ERPOrderId,
            dimensions = shipment.dimensions ?? existingOrder.dimensions,
            weight = shipment.weight,
            trackingNumber = shipment.trackingNumber,
            orderId = shipment.orderId,
            userId = shipment.userId,
            orderKey = shipment.orderKey,
            createDate = shipment.createDate,
            shipmentId = shipment.shipmentId,
            shipDate = shipment.shipDate,
            shipmentCost = shipment.shipmentCost,
            insuranceCost = shipment.insuranceCost,
            carrierCode = shipment.carrierCode,
            serviceCode = shipment.serviceCode,
            packageCode = shipment.packageCode,
            confirmation = shipment.confirmation,
            voided = shipment.voided,
            marketplaceNotified = shipment.marketplaceNotified,
            shipFrom = shipment.shipFrom ?? existingOrder.shipFrom,
            shipTo = shipment.shipTo,
            advancedOptions = existingOrder.advancedOptions,
            shipmentItems = appliedItems,
            labelData = shipment.labelData,
            testLabel = shipment.testLabel,
            IsExpedited = shipment.IsExpedited,
            notifyErrorMessage = shipment.notifyErrorMessage,
            formData = shipment.formData,
            voidDate = shipment.voidDate,
            batchNumber = shipment.batchNumber,
            ShippingAccountId = shipment.ShippingAccountId,
            warehouseId = shipment.warehouseId
        };

        return newShipment;
    }

    ///// <summary>  
    ///// Updates the shipment with OSM shipping estimates.  
    ///// </summary>
    ///// <param name="_webhooks">The Webhook instance to utilize.</param>  
    ///// <param name="newShipment">The OrderShipment instance to update.</param>
    //private static async Task UpdateShipmentWithOsmEstimatesAsync(IWebhooks _webhooks, OrderShipment newShipment)
    //{
    //    try
    //    {
    //        if (newShipment.carrierCode == "endicia" && newShipment.shipmentCost == 0)
    //        {
    //            OkObjectResult osmEstimateReturn = await _webhooks.GetOsmEstimatedShipmentRate(newShipment.shipTo.postalCode, newShipment.weight.value, newShipment.weight.units.ToString(), newShipment.dimensions.length, newShipment.dimensions.height, newShipment.dimensions.width, newShipment.dimensions.units.ToString()) as OkObjectResult;
    //            string osmJSONSerialized = JsonConvert.SerializeObject(osmEstimateReturn.Value, Newtonsoft.Json.Formatting.Indented);
    //            JToken osmContext = JObject.Parse(osmJSONSerialized)["Context"];
    //            OSMShippingEstimate osmShippingEstimate = JsonConvert.DeserializeObject<OSMShippingEstimate>(osmContext.ToString());
    //            if (osmShippingEstimate != null && osmShippingEstimate.Packages.Package.Error == "")
    //            {
    //                newShipment.serviceCode = "osm_" + osmShippingEstimate.Packages.Package.MailClass.Replace(" ", "_").ToLower();
    //                newShipment.shipmentCost = osmShippingEstimate.Packages.Package.TotalEstimate ?? 0.00m;
    //            }
    //        }
    //    }
    //    catch (System.Exception)
    //    {
    //        throw;
    //    }
    //}

    public async Task<List<Order>> GetOrderUpdates(List<int> ERPOrderIdList)
    {
        try
        {
            List<Order> OrderList = await _unitOfWork.Orders.GetListByQueryAsync(x =>
            x.Include(x => x.advancedOptions)
            .Include(x => x.dimensions)
            .Include(x => x.weight)
            .Include(x => x.Sources)
            .Include(x => x.items)
            .Where(o => ERPOrderIdList.Contains(o.ERPOrderId)).AsNoTracking());
            IEnumerable<Order> updatedOrders = [];
            await _unitOfWork.BeginTransactionAsync();
            foreach (Order order in OrderList)
            {
                if (order.Sources.Any(source => source.Name == OrderSourceEnum.shipstation ||
                source.Name == OrderSourceEnum.orderdesk) || order.orderKey.StartsWith("OD"))
                {
                    Order matchedShipstationOrder = await GetShipStationOrderByOrderId(order.orderId);

                    if (matchedShipstationOrder.orderStatus == OrderStatus.shipped)
                    {
                        await ProcessItemShipNotify(await GetShipStationShipmentsAsync(matchedShipstationOrder.orderId));
                        await ProcessFulfillmentShipped(await GetShipStationFulfillmentsAsync(matchedShipstationOrder.orderId));
                        Order updatedOrder = await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(order.ERPOrderId);
                        updatedOrders = updatedOrders.Append(updatedOrder);
                    }
                    else
                    {
                        await ProcessOrderNotify([matchedShipstationOrder]);
                        Order updatedOrder = await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(order.ERPOrderId);
                        updatedOrder = SetOrderDimensionsFromItems(updatedOrder);
                        updatedOrder = SetOrderWeightFromItems(updatedOrder);
                        _unitOfWork.Orders.Update(updatedOrder);
                        await _unitOfWork.SaveChangesAsync();
                        updatedOrders = updatedOrders.Append(updatedOrder);
                    }
                }
            }
            await _unitOfWork.CommitAsync();
            return [.. updatedOrders];
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error occurred while getting order updates");
            throw;
        }
    }
    public async Task<Order> FindMissingOrder(string orderNumber, int storeId)
    {
        if (await GetCountAsync(x => x.orderNumber == orderNumber && x.advancedOptions.storeId == storeId) > 0)
        {
            return await GetOrderByOrderNumberAsync(orderNumber);
        }
        try
        {
            // make API request for the initial order details  
            var orderDetails = await GetShipStationOrderDetails(orderNumber, storeId);
            if (orderDetails != null && orderDetails.Count > 0)
            {
                await ProcessOrderNotify(orderDetails);
                return await GetOrderByOrderNumberAsync(orderNumber);
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding missing order.");
            throw;
        }
    }
    public Order MapSkulabsDtoToOrder(SkulabsDTO skulabsDto, string requiredLocationId)
    {
        var filteredItems = skulabsDto.OrderItems
            .Where(item => item.LocationId == requiredLocationId)
            .Select(item => new OrderItem
            {
                quantity = item.Quantity,
                lineItemKey = item.LineId,
                warehouseLocation = item.LocationId,
                ERPOrderItemId = 0,
                options = new List<OrderItemOption>
                {
                        new OrderItemOption { Name = "item_id", value = item.ItemId },
                        new OrderItemOption { Name = "_id", value = item.Id }
                }
            }).ToList();

        if (!filteredItems.Any())
        {
            throw new InvalidOperationException("No items with the specified location_id found in the order.");
        }
        var order = new Order
        {
            orderId = long.TryParse(skulabsDto.OrderNumber, out var parsedOrderId) ? parsedOrderId : 0,
            orderNumber = skulabsDto.OrderNumber,
            userId = skulabsDto.UserId,
            orderKey = skulabsDto.OrderNumber,
            estimatedShipmentCost = skulabsDto.Cost,
            Sources = new List<OrderSource> { new OrderSource { Name = OrderSourceEnum.skulabs } },
            insuranceOptions = new OrderInsuranceOptions
            {
                insuredValue = skulabsDto.InsuranceOptions?.insuredValue ?? 0,
                provider = skulabsDto.InsuranceOptions?.provider
            },
            carrierCode = skulabsDto.Carrier,
            serviceCode = skulabsDto.Service,
            packageCode = skulabsDto.PackageCode,
            confirmation = 0,
            shipFrom = new OrderShippingInfo
            {
                country = skulabsDto.ShipFrom?.country
            },
            shipTo = new OrderShippingInfo
            {
                country = skulabsDto.ShipTo?.country,
                state = skulabsDto.ShipTo?.state,
                city = skulabsDto.ShipTo?.city,
                street1 = skulabsDto.ShipTo?.street1,
                name = skulabsDto.ShipTo?.name,
                postalCode = skulabsDto.ShipTo?.postalCode
            },
            advancedOptions = new OrderAdvancedOptions
            {
                storeId = int.Parse(skulabsDto.StoreId),

                // WarehouseId to 0, since Skulabs Warehouse Id's are a completely different data type (string vs long?)
                warehouseId = 0
            },
            items = filteredItems,
            isExpedited = skulabsDto.IsExpedited,
            dimensions = new OrderDimensions
            {
                height = skulabsDto.Height,
                length = skulabsDto.Length,
                width = skulabsDto.Width,

                // Defaulting to inches due to conversion issues.
                units = 0
            },
            weight = new OrderWeight
            {
                value = skulabsDto.Weight,

                // Same as before, conversion issues, so defaulting to ounces.
                units = OrderWeight.Units.ounces
            }
        };

        return order;
    }
    public async Task<List<Order>> ConvertZazzleToOrder(ZazzleDTO zazzleDTO)
    {
        if (zazzleDTO.zazzleRequest.response.result == null)
        {
            return null;
        }

        var zazzleOrders = zazzleDTO.zazzleRequest.response.result.Orders;
        ShipStationStore storeInfo = await _unitOfWork.ShipStationStores.FilterOneAsync(x => x.StoreName.ToUpper() == "ZAZZLE");

        List<Order> convertedOrders = [];
        foreach (ZazzleOrder zazzleOrder in zazzleOrders)
        {
            bool newOrder = false;
            var dbOrder = await _unitOfWork.Orders.GetOrderByKeyCustomSelectAsync(zazzleOrder.OrderId.ToString());
            if (dbOrder == null)
            {
                newOrder = true;
                dbOrder = new Order
                {
                    orderId = zazzleOrder.OrderId,
                    orderNumber = zazzleOrder.OrderId.ToString(),
                    orderKey = zazzleOrder.OrderId.ToString(),
                    orderDate = zazzleOrder.OrderDate,
                    orderStatus = zazzleOrder.orderStatus switch
                    {
                        ZazzleOrder.OrderStatus.assigned => OrderStatus.awaiting_shipment,
                        ZazzleOrder.OrderStatus.accepted => OrderStatus.awaiting_shipment,
                        ZazzleOrder.OrderStatus.shipped => OrderStatus.shipped,
                        ZazzleOrder.OrderStatus.cancelled => OrderStatus.cancelled,
                        _ => OrderStatus.on_hold,
                    },
                    createDate = DateTime.Now,
                    modifyDate = DateTime.Now,
                    paymentDate = null,
                    shipByDate = zazzleOrder.ShipByDate,
                    orderTotal = 0,
                    Sources = [new OrderSource() { Name = OrderSourceEnum.zazzle }],
                    items = []
                };
                var advancedOptions = new OrderAdvancedOptions
                {
                    warehouseId = 0,
                    nonMachinable = false,
                    saturdayDelivery = false,
                    containsAlcohol = false,
                    mergedOrSplit = false,
                    parentId = null,
                    storeId = storeInfo.StoreId,
                    storeName = storeInfo?.StoreName ?? zazzleDTO.VendorId.ToUpper(),
                    customField1 = "",
                    customField2 = "",
                    customField3 = "",
                    source = OrderSourceEnum.zazzle.ToString(),
                    billToParty = 0,
                    billToAccount = "",
                    billToPostalCode = "",
                    billToCountryCode = "",
                    billToMyOtherAccount = 0
                };
                dbOrder.advancedOptions = advancedOptions;
            }

            List<OrderItem> orderItems = [];
            foreach (ZazzleOrder.ZazzleLineItem zazzleLineItem in zazzleOrder.LineItems)
            {
                OrderItem existingItem = dbOrder.items.SingleOrDefault(x => x.orderItemId == zazzleLineItem.LineItemId);
                OrderItem convertedOrderItem = _orderItemService.ConvertZazzleItemToOrderItem(zazzleLineItem);
                convertedOrderItem = await _orderItemService.ConvertAttributeToProductSku(convertedOrderItem);
                if (existingItem == null)
                {
                    orderItems.Add(convertedOrderItem);
                    continue;
                }
                else
                {
                    convertedOrderItem.ERPOrderItemId = existingItem.ERPOrderItemId;
                    existingItem = convertedOrderItem;
                    orderItems.Add(existingItem);
                }
            }
            dbOrder.items = orderItems;

            convertedOrders.Add(dbOrder);
            _unitOfWork.Orders.Update(dbOrder);

            _ = await _webhooks.AcknowledgeZazzleOrder(zazzleOrder.OrderId.ToString(), (newOrder == true) ? "new" : "update");
        }
        await _unitOfWork.SaveChangesAsync();
        return convertedOrders;
    }
    /// <summary>
    /// Updates the entered order with dimensions calculated from the items within it.
    /// This assumes that all items in the order can be packed into a single box.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public Order SetOrderDimensionsFromItems(Order order)
    {
        //validation check
        if (!order.items.Any(x => x.Product != null || x.Bundle != null))
        {
            order.dimensions ??= new OrderDimensions
            {
                length = 1,
                width = 1,
                height = 1,
                units = OrderDimensions.Units.inches
            };
            return order;
        }

        OrderDimensions newDimensions = new();
        var itemList = order.items.Where(static x => x.Product != null || x.Bundle != null).ToList();
        double totalProductVolume = 0.0;
        foreach (var item in itemList)
        {
            //Create a tuple list to extrapolate the product and quantity from the bundles to do the calculations
            //Its simpler to setup the tuple list with the product and quantity to do the calculations from the bundles and then add the product and quantity from the orderItem
            var productItemList = item.Bundle?.BundleItems.Select(static x => (x.Product, x.Quantity)).ToList() ?? [];

            if (item.Product != null)
            {
                productItemList.Add((item.Product, item.quantity));
            }
            foreach (var productItem in productItemList)
            {
                //Make uniform the dimensional units
                if (productItem.Product.DimensionalUnit == DimensionalUnit.Feet)
                {
                    productItem.Product.DimensionalUnit = DimensionalUnit.Inches;
                    productItem.Product.Length *= 12;
                    productItem.Product.Width *= 12;
                    productItem.Product.Height *= 12;
                }
                else if (productItem.Product.DimensionalUnit == DimensionalUnit.Meters)
                {
                    productItem.Product.DimensionalUnit = DimensionalUnit.Centimeters;
                    productItem.Product.Length *= 100;
                    productItem.Product.Width *= 100;
                    productItem.Product.Height *= 100;
                }
                newDimensions.units = productItem.Product.DimensionalUnit switch
                {
                    DimensionalUnit.Inches => OrderDimensions.Units.inches,
                    DimensionalUnit.Centimeters => OrderDimensions.Units.centimeters,
                    _ => OrderDimensions.Units.inches
                };
                totalProductVolume += Convert.ToDouble(productItem.Product.Length * productItem.Product.Width * productItem.Product.Height * productItem.Quantity);
            }
        }
        var productsBoxed = (decimal)Math.Round(Math.Cbrt(totalProductVolume), 2);//Presuming all products can be stuffed into a cube
        newDimensions.height = productsBoxed;
        newDimensions.width = productsBoxed;
        newDimensions.length = productsBoxed;

        order.dimensions = newDimensions;
        return order;
    }

    public Order SetOrderWeightFromItems(Order order)
    {
        if (!order.items.Any(x => x.Product != null || x.Bundle != null))
        {
            order.weight = new OrderWeight
            {
                value = 1,
                units = OrderWeight.Units.ounces
            };
            return order;
        }

        OrderWeight newWeight = new()
        {
            value = 0
        };

        foreach (var item in order.items.Where(static x => x.Product != null || x.Bundle != null))
        {
            //Create a touble list to extrapulate the product and quantity from the bundles to do the calculations
            //Its simpler to setup the tuple list with the product and quantity to do the calculations from the bundles and then add the product and quantity from the orderItem
            var productItemList = item.Bundle?.BundleItems.Select(static x => (x.Product, x.Quantity)).ToList() ?? [];
            if (item.Product != null)
            {
                productItemList.Add((item.Product, item.quantity));
            }

            foreach (var productItem in productItemList)
            {
                if (productItem.Product.WeightUnit == WeightUnit.Pound)
                {
                    productItem.Product.WeightUnit = WeightUnit.Ounce;
                    productItem.Product.WeightAmount *= 16;
                }
                newWeight.value += productItem.Product.WeightAmount * productItem.Quantity;
                newWeight.units = productItem.Product.WeightUnit switch
                {
                    WeightUnit.Ounce => OrderWeight.Units.ounces,
                    WeightUnit.Pound => OrderWeight.Units.pounds,
                    _ => OrderWeight.Units.ounces
                };
            }
        }
        order.weight = newWeight;
        return order;
    }

    public async Task<Order> ConvertShopifyToOrder(ShopifyDTO shopifyDTO)
    {
        if (shopifyDTO == null)
        {
            return null;
        }

        var shopifyOrder = shopifyDTO;
        Order convertedOrder = null;

        var dbOrder = await _unitOfWork.Orders.GetOrderByKeyCustomSelectAsync(shopifyOrder.OrderId.ToString());

        if (dbOrder == null)
        {
            dbOrder = new Order
            {
                orderId = shopifyOrder.OrderId,
                orderNumber = shopifyOrder.OrderNumber.ToString(),
                orderKey = shopifyOrder.OrderId.ToString(),
                orderDate = shopifyOrder.CreatedAt,
                orderStatus = shopifyDTO.FulfillmentStatus switch
                {
                    "cancelled" => OrderStatus.cancelled,
                    "error" or "failure" => OrderStatus.on_hold,
                    "open" or "pending" => OrderStatus.awaiting_shipment,
                    "success" => OrderStatus.shipped,
                    _ => throw new ArgumentOutOfRangeException(nameof(shopifyDTO.FulfillmentStatus), $"Unexpected value: {shopifyDTO.FulfillmentStatus}")
                },
                createDate = DateTime.Now,
                modifyDate = DateTime.Now,
                paymentDate = null,
                shipByDate = null, // Need to check this out, because there's nothing like this on Shopify.  
                orderTotal = 0,
                Sources = new List<OrderSource> { new OrderSource { Name = OrderSourceEnum.shopify } }, // Need to run this by Nick.  
                items = new List<OrderItem>()
            };

            var advancedOptions = new OrderAdvancedOptions
            {
                warehouseId = 0,
                nonMachinable = false,
                saturdayDelivery = false,
                containsAlcohol = false,
                mergedOrSplit = false,
                parentId = null,
                storeId = -1, // Need assistance on grabbing the right store.  
                storeName = "", // Same as above.  
                customField1 = "",
                customField2 = "",
                customField3 = "",
                source = OrderSourceEnum.shopify.ToString(),
                billToParty = 0,
                billToAccount = "",
                billToPostalCode = "",
                billToCountryCode = "",
                billToMyOtherAccount = 0
            };
            dbOrder.advancedOptions = advancedOptions;
        }
        else
        {
            // Update existing order with new values  
            dbOrder.orderNumber = shopifyOrder.OrderNumber.ToString();
            dbOrder.orderDate = shopifyOrder.CreatedAt;
            dbOrder.orderStatus = shopifyDTO.FulfillmentStatus switch
            {
                "cancelled" => OrderStatus.cancelled,
                "error" or "failure" => OrderStatus.on_hold,
                "open" or "pending" => OrderStatus.awaiting_shipment,
                "success" => OrderStatus.shipped,
                _ => throw new ArgumentOutOfRangeException(nameof(shopifyDTO.FulfillmentStatus), $"Unexpected value: {shopifyDTO.FulfillmentStatus}")
            };
            dbOrder.modifyDate = DateTime.Now;
            // Update advanced options if needed  
            dbOrder.advancedOptions ??= new OrderAdvancedOptions
            {
                warehouseId = 0,
                nonMachinable = false,
                saturdayDelivery = false,
                containsAlcohol = false,
                mergedOrSplit = false,
                parentId = null,
                storeId = -1, // Need assistance on grabbing the right store.  
                storeName = null, // Same as above.  
                customField1 = "",
                customField2 = "",
                customField3 = "",
                source = OrderSourceEnum.shopify.ToString(),
                billToParty = 0,
                billToAccount = "",
                billToPostalCode = "",
                billToCountryCode = "",
                billToMyOtherAccount = 0
            };
        }

        List<OrderItem> orderItems = new List<OrderItem>();
        decimal totalWeightInOunces = 0;

        foreach (LineItem li in shopifyDTO.LineItems)
        {
            OrderItem existingItem = dbOrder.items.SingleOrDefault(x => x.orderItemId == li.Id);
            OrderItem convertedOrderItem = _orderItemService.ConvertShopifyItemToOrderItem(li);

            if (existingItem == null)
            {
                orderItems.Add(convertedOrderItem);
            }
            else
            {
                existingItem.sku = convertedOrderItem.sku;
                existingItem.quantity = convertedOrderItem.quantity;
                existingItem.unitPrice = convertedOrderItem.unitPrice;
                existingItem.Product = convertedOrderItem.Product;
                orderItems.Add(existingItem);
            }
            if (convertedOrderItem.weight != null && convertedOrderItem.weight.units == OrderWeight.Units.ounces)
            {
                totalWeightInOunces += convertedOrderItem.weight.value;
            }
        }

        dbOrder.items = orderItems;
        dbOrder.weight = new OrderWeight
        {
            value = totalWeightInOunces,
            units = OrderWeight.Units.ounces,
            WeightUnits = (int)OrderWeight.Units.ounces
        };
        convertedOrder = dbOrder;

        _unitOfWork.Orders.Update(dbOrder);
        await _unitOfWork.SaveChangesAsync();

        return convertedOrder;
    }

    public async Task UpdateOrdersAsync(List<Order> updatedOrders)
    {
        foreach (var updatedOrder in updatedOrders)
        {
            var existingOrder = await _unitOfWork.Orders.GetByQueryAsync(query =>
                query.Where(order => order.orderId == updatedOrder.orderId)
                .Include(order => order.advancedOptions)
                .Include(order => order.shipTo)
                .Include(order => order.billTo)
            );

            if (existingOrder != null)
            {
                existingOrder.orderStatus = updatedOrder.orderStatus;
                existingOrder.advancedOptions = updatedOrder.advancedOptions;
                existingOrder.shipTo = updatedOrder.shipTo;
                existingOrder.gift = updatedOrder.gift;
                existingOrder.giftMessage = updatedOrder.giftMessage;
                existingOrder.billTo = updatedOrder.billTo;
                existingOrder.modifyDate = updatedOrder.modifyDate;
                existingOrder.userId = updatedOrder.userId;

                _unitOfWork.Orders.Update(existingOrder);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public Task<(List<Order>, int)> GetOrdersAsync(
        int start,
        int length,
        List<string> ordernumbers,
        string itemName,
        OrderStatus[] orderStatus,
        int storeId,
        int[] productIds,
        int[] departmentIds,
        int[] orderTagId,
        string orderStartDate,
        string orderEndDate,
        string shipByDate,
        string orderColumn,
        string orderDir = "asc",
        int? orderBatchId = null,
        List<string> excludeItemNames = null,
        bool includeBatchedOrders = true
    ) => _unitOfWork.Orders.GetOrdersAsync(
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
        orderColumn,
        orderDir,
        orderBatchId,
        excludeItemNames,
        includeBatchedOrders
    );

    public Task<List<Product>> GetOrderProducts(string orderNumber) => _unitOfWork.Orders.GetOrderProducts(orderNumber);
    public List<Dictionary<string, string>> GetOrderItemsByDate(DateTime fromDate, DateTime toDate)
    {
        try
        {

            IQueryable<Dictionary<string, string>> query(IQueryable<Order> order) => order
                .Where(x => x.shipDate >= fromDate && x.shipDate <= toDate && x.orderStatus == OrderStatus.shipped)
                .Include(x => x.items)
                    .ThenInclude(x => x.Product)
                .OrderBy(x => x.shipByDate)
                .SelectMany(
                    x => x.items.Select(
                        Item => new Dictionary<string, string>{
                            {"Shipped Date", x.shipDate.ToString()},
                            {"Order number", x.orderNumber },
                            {"Product Sku", Item.Product.Sku },
                            {"Order item sale cost", Item.unitPrice.ToString() },
                            {"Product fulfillment", Item.Product.FulfillmentCost.ToString() },
                            {"Product cost", Item.Product.Cost.ToString() },
                            {"Product labor cost", Item.Product.LaborCost.ToString() }
                        }
                    )
                );

            return _unitOfWork.Orders.GetListByQuery(query);

        }
        catch (Exception)
        {
            throw;
        }
    }
    public List<Dictionary<string, string>> GetOrderItemsSumByDate(DateTime fromDate, DateTime toDate)
    {
        return _unitOfWork.Orders.GetOrderAndItemsSumByDate(fromDate, toDate);
    }

    public Task<List<YearlyProductShippedReport>> GetYearlyProductCountReport()
    {
        return _unitOfWork.Orders.GetYearlyProductCountReport();
    }

    public Task<OrderShippingInfo> GetOrderShipToAddressAsync(long orderId, string orderKey)
    => _unitOfWork.Orders.GetOrderShipToAddressAsync(orderId, orderKey);



    //API CALLS

    /// <summary>  
    /// Sets the 'completed' tag on the specified order.  
    /// </summary>  
    /// <param name="orderId">The order ID to set the 'completed' tag on.</param>  
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    public async Task<string> SetShipStationCompletedTag(string orderId, string tagId = "99825")
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                orderId,
                tagId
            }), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync("orders/addtag", jsonContent);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }

    /// <summary>  
    /// Retrieves the ShipStation tags for the account.
    /// </summary>  
    /// <returns>An OrderTag list containing the ShipStation tags.</returns>
    public async Task<List<OrderTag>> GetShipStationTags()
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        var response = await client.GetFromJsonAsync<List<OrderTag>>("accounts/listtags", options);
        return response;
    }

    /// <summary>  
    /// Retrieves the ShipStation tags information based on the provided tag IDs.  
    /// </summary>  
    /// <param name="tagIds">An array of integers containing the list of tag IDs to fetch information for.</param>  
    /// <returns>A OrderTag list containing the tag information if successful, otherwise an ArguementNullException.</returns>  
    public async Task<List<OrderTag>> GetShipStationTagsByIdsAsync(int[] tagIds)
    {
        ArgumentNullException.ThrowIfNull(tagIds);
        var allTags = await GetShipStationTags();
        return [.. allTags.Where(x => tagIds.Contains(x.tagId))];
    }

    /// <summary>  
    /// Retrieves the order details for the specified shipstation order id.  
    /// </summary>  
    /// <param name="orderId">The order id to retrieve details for.</param>  
    /// <returns>An IActionResult containing the order details, or an error if the operation fails.</returns>  
    public async Task<Order> GetShipStationOrderByOrderId(long orderId)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
            var order = await client.GetFromJsonAsync<Order>($"orders/{orderId}", options);
            return order;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to parse order;{ex}", ex);
            throw;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }

    }
    /// <summary>  
    /// Retrieves the orders associated with the specified tag ID.  
    /// </summary>  
    /// <param name="tagId">The tag ID to retrieve orders for.</param>  
    /// <returns>An IActionResult containing the orders, or an error if the operation fails.</returns>  
    public async Task<List<Order>> GetShipStationOrdersWithTag(int tagId)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
            List<Order> orders = [];
            int currentPage = 1;
            int totalPages;
            do
            {
                var response = await client.GetAsync($"orders/listbytag?orderStatus=awaiting_shipment&tagId={tagId}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var inlinejson = await client.GetFromJsonAsync<ShipStationWebhookOrderDTO>($"orders/listbytag?orderStatus=awaiting_shipment&tagId={tagId}&page={currentPage}", options);
                orders.AddRange(inlinejson.Orders);
                currentPage++;
                totalPages = inlinejson.Pages;

            } while (currentPage <= totalPages);
            return orders;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }

    }

    public async Task ForceUpdateOrders(ShipStationOrderFilter shipStationOrderFilter)
    {
        var queryParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (PropertyInfo property in typeof(ShipStationOrderFilter).GetProperties())
        {
            var value = property.GetValue(shipStationOrderFilter);
            if (value != null)
            {
                if (value is DateTime dateTimeValue)
                    queryParams[property.Name] = dateTimeValue.ToString(ShipStationOrderFilter.DateFormat, CultureInfo.InvariantCulture);
                else
                    queryParams[property.Name] = value.ToString();
            }
        }
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        int currentPage = shipStationOrderFilter.Page;
        int totalPages;
        do
        {
            var response = await client.GetFromJsonAsync<ShipStationWebhookOrderDTO>($"orders?{queryParams}", options);
            await ProcessOrderNotify(response.Orders);
            var shippedOrders = response.Orders.Where(x => x.orderStatus == OrderStatus.shipped).ToList();
            foreach (var order in shippedOrders)
            {
                List<OrderShipment> orderShipments = await GetShipStationShipmentsAsync(order.orderId);
                orderShipments.ForEach(x => x.Order = order);
                await ProcessItemShipNotify(orderShipments);
                List<OrderFulfillment> orderFulfillments = await GetShipStationFulfillmentsAsync(order.orderId);
                orderFulfillments.ForEach(x => x.Order = order);
                await ProcessFulfillmentShipped(orderFulfillments);
            }
            currentPage++;
            queryParams["page"] = currentPage.ToString();
            totalPages = response.Pages;

        } while (currentPage <= totalPages);
    }


    /// <summary>  
    /// Retrieves the order details for the specified order number.  
    /// </summary>  
    /// <param name="orderNumber">The order number to retrieve details for.</param>
    /// <param name="storeId"></param>  
    /// <returns>An IActionResult containing the order details, or an error if the operation fails.</returns>  
    public async Task<List<Order>> GetShipStationOrderDetails(string orderNumber, int storeId)
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        var response = await client.GetFromJsonAsync<ShipStationWebhookOrderDTO>($"orders?orderNumber={orderNumber}&storeId={storeId}", options);
        return response.Orders;
    }
    public async Task<string> AddOrRemoveShipStationTagAsync(long orderId, int tagId, bool addOrRemoveTag)
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                orderId,
                tagId
            }), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync("orders/" + ((addOrRemoveTag == true) ? "addtag" : "removetag"), jsonContent);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }

    /// <summary>  
    /// Sets the specified order as shipped with the given carrier code, ship date, and tracking number.  
    /// </summary>  
    /// <param name="orderId">The order ID to set as shipped.</param>  
    /// <param name="carrierCode">The carrier code to use for shipping.</param>  
    /// <param name="shipDate">The ship date of the order.</param>  
    /// <param name="trackingNumber">The tracking number for the shipment.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns> 
    public async Task<Order> SetOrderAsShipped(string orderId, string carrierCode, DateTime shipDate, string trackingNumber)
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                orderId,
                carrierCode,
                shipDate,
                trackingNumber,
                notifyCustomer = true,
                notifySalesChannel = true
            }), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync("orders/markasshipped", jsonContent);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Order>(jsonResponse);
    }
    public async Task<List<OrderShipment>> GetShipStationShipmentsAsync(long orderId)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
            var response = await client.GetFromJsonAsync<ShipStationWebhookOrderDTO>($"shipments?orderId={orderId}", options);
            return response.Shipments;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to pull Order Shipment;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }

    /// <summary>  
    /// Retrieves the fulfillment information for the specified ShipStation order ID and manual tracking import.  
    /// </summary>  
    /// <param name="orderId">The ShipStation order ID to retrieve fulfillment information for.</param>  
    /// <returns>An IActionResult containing the fulfillment information, or an error if the operation fails.</returns> 
    public async Task<List<OrderFulfillment>> GetShipStationFulfillmentsAsync(long orderId)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
            var response = await client.GetFromJsonAsync<ShipStationWebhookOrderDTO>($"fulfillments?orderId={orderId}", options);
            return response.Fulfillments;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to pull Order Shipment;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }
    /// <summary>  
    /// Retrieves the ShipEngine estimated shipment rate for the specified carrier IDs and order.  
    /// </summary>  
    /// <param name="carrierIds">A list of carrier IDs to get estimated shipment rates for.</param>  
    /// <param name="order">The order for which to get estimated shipment rates.</param>  
    /// <returns>A json string containing the ShipEngine estimated shipment rates, or an error if the operation fails.</returns> 
    public async Task<List<ShipEngineShippingEstimate>> GetShipEngineEstimatedShipmentRate(List<string> carrierIds, Order order)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    carrier_ids = carrierIds,
                    weight = new
                    {
                        unit = order.weight.units.ToString() switch { "ounces" => "ounce", "pounds" => "pound", _ => order.weight.units.ToString() },
                        order.weight.value
                    },
                    dimensions = new
                    {
                        unit = order.dimensions.units.ToString() switch { "inches" => "inch", "centimeters" => "centimeter", _ => order.dimensions.units.ToString() },
                        height = order.dimensions.height.ToString(),
                        length = order.dimensions.length.ToString(),
                        width = order.dimensions.width.ToString()
                    },
                    from_country_code = order.shipFrom.country,
                    from_postal_code = order.shipFrom.postalCode,
                    from_city_locality = order.shipFrom.city,
                    from_state_province = order.shipFrom.state,
                    to_country_code = order.shipTo.country,
                    to_city_locality = order.shipTo.city,
                    to_state_province = order.shipTo.state,
                    to_postal_code = order.shipTo.postalCode,
                    confirmation = "none",
                    address_residential_indicator = order.shipTo.residential switch { true => "yes", false => "no", _ => "unknown" },
                    ship_date = DateTime.Now
                }), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync("rates/estimate", jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var estimateList = JsonSerializer.Deserialize<List<ShipEngineShippingEstimate>>(jsonResponse);
            return estimateList;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get rate estimate.");
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }

    /// <summary>  
    /// Retrieves the ShipEngine order label for the specified tracking number.  
    /// </summary>  
    /// <param name="TrackingNumber">The tracking number for which to get the order label.</param>  
    /// <returns>An IActionResult containing the ShipEngine order label, or an error if the operation fails.</returns>  
    public async Task<ShipEngineLabel> GetShipEngineOrderLabel(string TrackingNumber)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
            using HttpResponseMessage response = await client.GetAsync($"labels?label_status=completed&tracking_number={TrackingNumber}");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<ShipEngineLabelRoot>(jsonResponse).ShipEngineLabels.FirstOrDefault();
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
    }

    /// <summary>  
    /// Voids the shipment label for the specified shipment ID.  
    /// </summary>  
    /// <param name="shipmentId">The ShipEngine Label ID for which to void the label.</param>  
    /// <returns>An ShipEngineVoidMessage indicating the success or failure of the operation.</returns>
    public async Task<ShipStationVoidMessage> VoidShipmentLabel(long shipmentId)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                shipmentId
            }), Encoding.UTF8, "application/json");
        using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
        using HttpResponseMessage response = await client.PostAsync("voidlabel", jsonContent);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ShipStationVoidMessage>(jsonResponse);
    }

    public async Task<ShipEngineVoidMessage> VoidFulfillmentLabel(string labelId)
    {
        using StringContent jsonContent = new("", Encoding.UTF8, "application/json");
        using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
        using HttpResponseMessage response = await client.PutAsync($"labels/{labelId}/void", jsonContent);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ShipEngineVoidMessage>(jsonResponse);
    }

    /// <summary>  
    /// Creates a ShipEngine shipment label for the given order.  
    /// </summary>  
    /// <param name="order">The order for which to create a shipment label.</param>  
    /// <returns>An IActionResult containing the shipment label data, or an error if the operation fails.</returns>
    public async Task<ShipEngineLabel> CreateShipEngineShipmentLabel(Order order)
    {
        var requestBody = new
        {
            label_format = "pdf",
            label_layout = "4x6",
            label_download_type = "inline"
        };

        var shipTo = new
        {
            name = order.shipTo.name[..Math.Min(50, order.shipTo.name.Length)],
            company_name = order.shipTo.company?[..Math.Min(30, order.shipTo.company.Length)],
            phone = order.shipTo.phone,
            address_line1 = order.shipTo.street1,
            address_line2 = order.shipTo.street2,
            address_line3 = order.shipTo.street3,
            city_locality = order.shipTo.city,
            state_province = order.shipTo.state,
            postal_code = order.shipTo.postalCode,
            country_code = order.shipTo.country,
            address_residential_indicator = order.shipTo.residential switch { true => "yes", false => "no", _ => "unknown" }
        };

        var packages = new[]
        {
            new
            {
                weight = new
                {
                    unit = order.weight.units.ToString() switch { "ounces" => "ounce", "pounds" => "pound", _ => order.weight.units.ToString() },
                    value = order.weight.value.ToString()
                },
                dimensions = new
                {
                    unit = order.dimensions.units.ToString() switch { "inches" => "inch", "centimeters" => "centimeter", _ => order.dimensions.units.ToString() },
                    height = order.dimensions.height.ToString(),
                    length = order.dimensions.length.ToString(),
                    width = order.dimensions.width.ToString()
                },
                label_messages = new
                {
                    reference1 = string.IsNullOrEmpty(order.advancedOptions.labelMessageReference1) ? order.orderNumber : order.advancedOptions.labelMessageReference1,
                    reference2 = string.IsNullOrEmpty(order.advancedOptions.labelMessageReference2) ? order.orderId.ToString() : order.advancedOptions.labelMessageReference2,
                    reference3 = order.advancedOptions.labelMessageReference3
                },
                insured_value = order.insuranceOptions != null && order.insuranceOptions.insureShipment ? new
                {
                    currency = "usd",
                    amount = order.insuranceOptions.insuredValue.ToString()
                } : null
            }
        };

        var requestShipmentBody = new
        {
            package_code = order.packageCode,
            service_code = order.serviceCode,
            carrier_id = order.carrierId,
            carrier_code = order.carrierCode,
            ship_to = shipTo,
            confirmation = "delivery",
            packages
        };
        // Convert to JsonObject for conditional additions  
        var requestShipmentJson = JsonSerializer.SerializeToNode(requestShipmentBody).AsObject();

        if (order.shipFrom.postalCode == "70501")
        {
            requestShipmentJson.Add("warehouse_id", "se-21593268");
        }
        else
        {
            var shipFrom = new
            {
                order.shipFrom.name,
                company_name = order.shipFrom.company,
                order.shipFrom.phone,
                address_line1 = order.shipFrom.street1,
                address_line2 = order.shipFrom.street2,
                address_line3 = order.shipFrom.street3,
                city_locality = order.shipFrom.city,
                state_province = order.shipFrom.state,
                postal_code = order.shipFrom.postalCode,
                country_code = order.shipFrom.country,
                address_residential_indicator = order.shipFrom.residential switch { true => "yes", false => "no", _ => "unknown" }
            };

            requestShipmentJson.Add("ship_from", JsonSerializer.SerializeToNode(shipFrom));
        }

        if (order.insuranceOptions != null && order.insuranceOptions.insureShipment)
        {
            requestShipmentJson.Add("insurance_provider", order.insuranceOptions.provider.Value.ToString());
        }

        // Specifically adding these for Wayfair shipments.  
        if (order.advancedOptions.storeId == 1002827)
        {
            var advancedOptions = new
            {
                bill_to_account = "252225761",
                bill_to_country_code = "US",
                bill_to_party = "third_party",
                bill_to_postal_code = "41042"
            };

            requestShipmentJson.Add("advanced_options", JsonSerializer.SerializeToNode(advancedOptions));
        }
        var request = JsonSerializer.SerializeToNode(requestBody).AsObject();
        request.Add("shipment", requestShipmentJson);
        try
        {
            // Convert final JsonObject to JSON string  
            string requestBodyJson = JsonSerializer.Serialize(request);
            using StringContent jsonContent = new(requestBodyJson, Encoding.UTF8, "application/json");
            using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
            using HttpResponseMessage response = await client.PostAsync("labels", jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ShipEngineLabel>(jsonResponse);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
    }

    public async Task<UspsAddressValidationResponse> UspsAddressValidation(string queryString)
    {
        try
        {
            string requestBodyJson = JsonSerializer.Serialize(queryString);
            using StringContent jsonContent = new(requestBodyJson, Encoding.UTF8, "application/json");
            using HttpClient client = _httpClientFactory.CreateClient("USPS");
            using HttpResponseMessage response = await client.GetAsync($"addresses/v3/address?{queryString}");
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<UspsFailingResponse>();
                _logger.LogWarning("USPS Address Validation failed;{errorResponse}", errorResponse.Error.Message);
            }
            else
                response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UspsAddressValidationResponse>(jsonResponse);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to validate address with USPS;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }

    }
}