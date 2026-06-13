using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Services
{
    public class OrderBatchService(IUnitOfWork unitOfWork, IUserSiteMappingService userSiteMappingService)
        : Service<OrderBatch>(unitOfWork), IOrderBatchService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUserSiteMappingService _userSiteMappingService = userSiteMappingService;

        #region Pre-Creation Checks
        public async Task<List<MissingSkuEntry>> GetMissingSkusListAsync(List<InventoryPickList> inventoryPickList)
        {
            var missingSkusList = new List<MissingSkuEntry>();

            foreach (var pickListEntry in inventoryPickList)
            {
                Product matchingProduct = null;
                try
                {
                    matchingProduct = await _unitOfWork.Products.FilterOneAsync(p =>
                        p.ProductId == pickListEntry.ERPProductId || p.Sku == pickListEntry.Sku);
                    //Including the Sku check, specifically for testing DB.  
                }
                catch (Exception)
                {
                    // Log the exception or handle it accordingly  
                    // For now, we'll just set matchingProduct to null  
                    matchingProduct = null;
                }

                if (matchingProduct == null || matchingProduct.IsActive == false)
                {
                    if (string.IsNullOrEmpty(pickListEntry.InvalidSku))
                    {
                        pickListEntry.InvalidSku = pickListEntry.Sku;
                    }

                    var orderOptions = pickListEntry.OrderOptions;

                    missingSkusList.Add(new MissingSkuEntry
                    {
                        Sku = pickListEntry.Sku,
                        OrderOptions = orderOptions
                    });
                   
                }
            }

            return missingSkusList;
        }

        public async Task<List<int>> GetUnassignableDepartments(List<InventoryPickList> inventoryPickList)
        {
            List<int> unassignedDepartments = new List<int>();

            foreach (InventoryPickList item in inventoryPickList)
            {
                if (item.Department == 0)
                {
                    int? productId = item.ERPProductId;

                    if (productId.HasValue)
                    {
                        var departments = await GetDepartmentIdsByProductIdAsync(productId.Value);
                        if (departments == null || departments.Count == 0 || departments.Count > 1 || (departments.Count == 1 && departments[0] == 0))
                        {
                            unassignedDepartments.Add(item.ERPOrderItemId);
                        }
                    }
                    else
                    {
                        unassignedDepartments.Add(item.ERPOrderItemId);
                    }
                }
            }

            return unassignedDepartments;
        }

        public async Task<List<DuplicateBatchInfo>> CheckDuplicateBatchesByERPOrderIdsAsync(List<int> ERPOrderIds)
        {
            if (ERPOrderIds.Count == 0)
            {
                throw new Exception("No orders could be found.");
            }

            List<OrderBatchItem> orderBatchItems = await _unitOfWork.OrderBatch.GetOrderBatchItemsByERPOrderIds(ERPOrderIds);

            List<DuplicateBatchInfo> duplicateBatchInfoList = orderBatchItems
                .Select(obi => new DuplicateBatchInfo
                {
                    BatchNumber = obi.OrderBatch.BatchNumber,
                    OrderNumber = obi.OrderNumber
                }).ToList();

            return duplicateBatchInfoList;
        }

        #endregion

        #region Batch Creation

        public async Task<BatchCreationResult> CreateBatchAsync(List<int> ERPOrderIds, int BatchType, string BatchName, List<AssignedDepartment> assignedDepartments, List<ReplacementSku> replacementSkus, string User, bool IsDeductible = true)
        {
            var result = new BatchCreationResult();

            try
            {
                List<Order> orders = await GetOrdersWithProductsByERPOrderIdsAsync(ERPOrderIds);

                if (assignedDepartments != null)
                {
                    foreach (Order order in orders)
                    {
                        foreach (OrderItem item in order.items)
                        {
                            var assignedDepartment = assignedDepartments.FirstOrDefault(ad => ad.OrderItemId == item.orderItemId);
                            if (assignedDepartment != null)
                            {
                                item.Product.Departments.Clear();
                                Department assignedDept = await GetDepartmentByIdAsync(assignedDepartment.AssignedDepartmentId);
                                if (assignedDept != null)
                                {
                                    var newDepartment = new Department
                                    {
                                        DepartmentId = assignedDept.DepartmentId,
                                        DepartmentName = assignedDept.DepartmentName
                                    };
                                    item.Product.Departments.Add(newDepartment);
                                }
                            }
                        }
                    }
                }

                List<InventoryPickList> inventoryPickLists = new List<InventoryPickList>();
                foreach (Order order in orders)
                {
                    foreach (OrderItem item in order.items)
                    {
                        if (item.name == "Discount" || item.adjustment == true || item.unitPrice < 0) continue;

                        var replacement = replacementSkus?.FirstOrDefault(r => r.OriginalSku == item.sku);
                        var assignedDepartment = assignedDepartments?.FirstOrDefault(ad => ad.OrderItemId == item.ERPOrderItemId);

                        if (item.Bundle != null)
                        {
                            if (item.Bundle.BundleItems == null || !item.Bundle.BundleItems.Any())
                            {
                                // Grabbing bundle items by the bundleId if we have a bundle, but no items.
                                item.Bundle.BundleItems = await _unitOfWork.BundleItems
                                    .FindAsync(bi => bi.BundleId == item.Bundle.BundleId);
                                foreach (var bi in item.Bundle.BundleItems)
                                {
                                    bi.Product = await _unitOfWork.Products.GetByIdAsync(bi.ProductId);
                                }
                            }
                            foreach (var bundleItem in item.Bundle.BundleItems)
                            {
                                InventoryPickList pickList = await CreatePickListItem(bundleItem.Product.Sku, bundleItem.Product.Description, bundleItem.Quantity * item.quantity, bundleItem.ProductId, item.ERPOrderItemId, order, replacement, assignedDepartment, item);
                                inventoryPickLists.Add(pickList);
                            }
                        }
                        else
                        {
                            InventoryPickList pickList = await CreatePickListItem(item.sku, item.name, item.quantity, item.ERPProductId.HasValue ? item.ERPProductId.Value : 0, item.ERPOrderItemId, order, replacement, assignedDepartment, item);
                            inventoryPickLists.Add(pickList);
                        }
                    }
                }

                result.MissingSkus = await GetMissingSkusListAsync(inventoryPickLists);
                if (result.MissingSkus.Count > 0)
                {
                    result.Success = false;
                    result.Message = "Missing SKUs found.";
                    return result;
                }
                result.UnassignedDepartments = await GetUnassignableDepartments(inventoryPickLists);
                if (result.UnassignedDepartments.Count > 0)
                {
                    result.Success = false;
                    result.Message = "Unassigned departments found.";
                    return result;
                }
                result.SimplifiedPickList = inventoryPickLists
                    .Select(item => new SimplifiedInventoryPickList
                    {
                        Sku = item.Sku,
                        Quantity = item.AmountRequired,
                        Description = item.Description
                    })
                    .ToList();
                OrderBatch orderBatch = new OrderBatch();

                // Start the transaction for the actual batch and batch item creation process.
                bool success = await _unitOfWork.OrderBatch.ExecuteTransactionAsync(async () =>
                {
                    orderBatch = await _unitOfWork.OrderBatch.CreateOrderBatch(BatchName, User, (BatchType)BatchType, IsDeductible);
                    List<OrderBatchItem> orderBatchItems = await _unitOfWork.OrderBatch.CreateOrderBatchItems(orderBatch.OrderBatchId, inventoryPickLists, IsDeductible);
                });

                if (!success)
                {
                    throw new InvalidOperationException("Error occurred during batch creation");
                }

                result.CompleteBatchNumber = orderBatch.BatchNumber;
                result.Success = true;
                result.Message = "Batch created successfully";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                if (ex.InnerException != null)
                {
                    result.Message += " | Inner Exception: " + ex.InnerException.Message;
                }
                return result;
            }
        }

        public async Task<InventoryPickList> CreatePickListItem(string sku, string description, int amountRequired, int ERPProductId, int ERPOrderItemId, Order order, ReplacementSku replacement, AssignedDepartment assignedDepartment, OrderItem item)
        {
            InventoryPickList pickList = new InventoryPickList
            {
                Sku = sku,
                Description = description,
                AmountRequired = amountRequired,
                ERPProductId = ERPProductId,
                ERPOrderItemId = ERPOrderItemId,
                ERPOrderId = order.ERPOrderId,
                Quantity = amountRequired,
                OrderNumber = order.orderNumber,
                OrderQuantities = new List<OrderQuantity>(),
                OrderOptions = item.options
            };

            if (replacement != null)
            {
                pickList.InvalidSku = pickList.Sku;
                pickList.Sku = replacement.NewSku;
            }
            if (pickList.ERPProductId == 0)
            {
                pickList.ERPProductId = await GetProductIdBySku(pickList.Sku) ?? 0;
            }
            if (pickList.ERPProductId != 0)
            {
                pickList.DepartmentList = await GetDepartmentIdsByProductIdAsync(pickList.ERPProductId);
                if (pickList.DepartmentList.Count != 0)
                {
                    pickList.Department = pickList.DepartmentList[0];
                }
            }
            if (assignedDepartment != null && pickList.ERPOrderItemId == assignedDepartment.OrderItemId)
            {
                pickList.Department = assignedDepartment.AssignedDepartmentId;
            }
            pickList.OrderQuantities.Add(new OrderQuantity
            {
                ERPOrderId = order.ERPOrderId,
                OrderNumber = order.orderNumber
            });

            return pickList;
        }
        #endregion

        #region Batch Dropdown Population

        public async Task<List<BatchItemViewModel>> GetFilteredProductsForBatchItems(List<BatchItemViewModel> batchViewModels)
        {
            foreach (BatchItemViewModel item in batchViewModels)
            {
                // Get the index of the first digit in the SKU  
                bool AltCheck = _unitOfWork.OrderBatch.IsAltItemCheck(item.Sku);
                if (AltCheck == true)
                {
                    int indexOfFirstDigit = item.Sku.IndexOfAny("0123456789".ToCharArray());

                    if (indexOfFirstDigit != -1)
                    {
                        // Get the SKU prefix  
                        string skuPrefix = item.Sku.Substring(0, indexOfFirstDigit);

                        // Call the repository method to fetch the filtered products asynchronously  
                        List<Product> filteredProducts = await _unitOfWork.OrderBatch.GetFilteredProducts(skuPrefix);

                        // Set the FilteredProductSkus property  
                        item.FilteredProductSkus = filteredProducts.Select(p => p.Sku).ToList();
                    }
                    else
                    {
                        // If the SKU doesn't contain any digits, set the FilteredProductSkus property to an empty list  
                        item.FilteredProductSkus = new List<string>();
                    }
                }
                else
                {
                    item.FilteredProductSkus = new List<string>();
                }
            }
            return batchViewModels;
        }
        #endregion

        #region Location Info Related
        public async Task<List<LocationInfo>> GetLocationInfo(string sku, string userId)
        {
            // Getting user site mappings here
            var userSiteMappings = _userSiteMappingService.GetList(x => x.UserId == userId);

            // Optimize database queries by combining them or using eager loading (if possible)  
            List<Stock> stocks = await _unitOfWork.OrderBatch.GetStocksBySku(sku);
            List<Location> locations = await _unitOfWork.OrderBatch.GetLocationsByStocks(stocks);
            List<Location> receiveOnlyLocations = await _unitOfWork.OrderBatch.GetReceiveOnlyLocations();

            // Filtering locations based on user site mappings  
            var siteIds = userSiteMappings.Select(x => x.SiteId).ToHashSet();

            Dictionary<int, LocationInfo> locationInfo = new();
            foreach (Stock stock in stocks)
            {
                Location location = locations.FirstOrDefault(l => l.LocationId == stock.LocationId && siteIds.Contains(l.SiteId));
                if (location == null)
                {
                    continue;
                }
                if (locationInfo.TryGetValue(location.LocationId, out LocationInfo existingLocationInfo))
                {
                    existingLocationInfo.TotalAvailable += stock.TotalAvailable;
                }
                else
                {
                    locationInfo[location.LocationId] = new LocationInfo
                    {
                        LocationId = location.LocationId,
                        LocationName = location.LocationName,
                        TotalAvailable = stock.TotalAvailable,
                        Type = location.Type
                    };
                }
            }

            foreach (Location location in receiveOnlyLocations)
            {
                if (!locationInfo.ContainsKey(location.LocationId) && siteIds.Contains(location.SiteId))
                {
                    locationInfo[location.LocationId] = new LocationInfo
                    {
                        LocationId = location.LocationId,
                        LocationName = location.LocationName,
                        TotalAvailable = 0,
                        Type = location.Type
                    };
                }
            }

            List<LocationInfo> orderedLocationInfo = locationInfo.Values.OrderByDescending(x => x.TotalAvailable).ToList();
            if (orderedLocationInfo.Count > 0)
            {
                orderedLocationInfo[0].IsDefault = true;
            }
            return orderedLocationInfo;
        }

        public async Task<List<DesignBatchItemViewModel>> GetDesignBatchItemsWithLocationsAsync(int orderBatchId)
        {
            var designBatchItems = await GetDesignBatchItemsAsync(orderBatchId);

            foreach (var item in designBatchItems)
            {
                var locationsWithStock = await GetLocationsWithStockAsync(item.ProductId);

                // Separate the locations by type  
                item.PickOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.PickOnly).ToList();
                item.ReceiveOnlyLocations = locationsWithStock.Where(l => l.Type == LocationType.ReceiveOnly).ToList();
            }

            return designBatchItems;
        }

        public async Task<List<LocationInfo>> GetLocationsWithStockAsync(int productId)
        {
            // Fetch stocks with the given productId and where TotalAvailable > 0  
            var stocksWithProduct = await _unitOfWork.Stocks.GetListByFilterAsync(
                s => s.ProductId == productId && s.TotalAvailable > 0,
                includes: new Expression<Func<Stock, object>>[] { s => s.Location }
            );

            // Map to LocationInfo  
            var locationInfoList = stocksWithProduct.Select(stock => new LocationInfo
            {
                LocationId = stock.Location.LocationId,
                LocationName = stock.Location.LocationName,
                TotalAvailable = stock.TotalAvailable,
                Type = stock.Location.Type,
                IsDefault = stock.Location.IsActive
            }).ToList();

            return locationInfoList;
        }

        public async Task<int?> GetProductIdBySku(string sku)
        {
            // Query the database to find the product with the given SKU  
            var product = _unitOfWork.Products.FilterOne(p => p.Sku == sku);

            // If a product is found, return the ProductId, else return null  
            if (product != null)
            {
                return product.ProductId;
            }

            return null;
        }

        public async Task<List<BatchItemViewModel>> GetBatchItems(int orderBatchId)
        {
            List<OrderBatchItem> batchItems = await GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(orderBatchId);
            batchItems = batchItems.Where(item => item.Order.orderStatus != Order.OrderStatus.cancelled).ToList();
            List<BatchItemViewModel> batchItemViewModels = new();

            foreach (OrderBatchItem item in batchItems)
            {
                //List<LocationInfo> locationInfo = await GetLocationInfo(item.Product.Sku);
                BatchItemViewModel itemViewModel = new()
                {
                    BatchItem = item,
                    Sku = item.Product.Sku,
                    //LocationInfo = locationInfo
                };
                batchItemViewModels.Add(itemViewModel);
            }

            return await HandleSkuSets(batchItemViewModels);
        }

        //This function is specifically for calculating what the base version of the "set of" sku is.
        private string GetBaseSku(string sku)
        {
            // Regex pattern to match "SO" followed by any number.
            //We look for "SO" that are followed by one or more digits.
            //We use the @ symbol so that escape characters are treated as literal characters.
            //We can technically remove it and just add an additional backslash to the escape characters, but it's easier to read this way.
            string pattern = @"SO\d+";

            //Here we're just trying to match the pattern to the sku.
            Match match = Regex.Match(sku, pattern);

            //Here we're getting the actual numerical length. The -2 is to not account for 'SO".
            //The purpose of this is to see if what we're trying to find is 1, 01, or 001 (or more technically).
            int numberLength = match.Value.Length - 2;

            //Here we're replacing the matches pattern in the sku with a new string.
            //New string still contains SO, along with a sequence of 0's ending with 1 for the base.
            //Number of 0's is obviously 1 less than the pattern, to account for the last number being 1.
            string baseSku = Regex.Replace(sku, pattern, "SO" + new string('0', numberLength - 1) + "1");

            return baseSku;
        }

        public async Task<List<BatchItemViewModel>> HandleSkuSets(List<BatchItemViewModel> batchItemViewModels)
        {
            Dictionary<string, BatchItemViewModel> combinedItems = new();

            foreach (BatchItemViewModel item in batchItemViewModels)
            {
                string sku = item.Sku;
                string uniqueKey = sku + "_" + item.BatchItem.OrderBatchItemId;

                if (Regex.IsMatch(sku, @"SO0*[1-9][0-9]+|SO0*[2-9]"))
                {
                    string baseSku = GetBaseSku(sku);
                    int multiplier = int.Parse(Regex.Match(sku, @"\d+").Value);

                    int[] specificProductIds = { 1622, 1623, 1624, 1543, 1544, 1545 };
                    if (specificProductIds.Contains(item.BatchItem.ProductId))
                    {
                        baseSku = GetAdjustedSku(baseSku);
                    }

                    BatchItemViewModel existingBaseSkuViewModel = batchItemViewModels.FirstOrDefault(vm => vm.Sku == baseSku);

                    if (existingBaseSkuViewModel != null)
                    {
                        existingBaseSkuViewModel.BatchItem.Quantity += item.BatchItem.Quantity;
                        existingBaseSkuViewModel.LocationInfo.AddRange(item.LocationInfo);
                    }
                    else
                    {
                        Product baseProduct = await _unitOfWork.Products.FilterOneAsync(x => x.Sku == baseSku);
                        if (baseProduct != null)
                        {
                            item.Sku = baseSku;
                            item.BatchItem.Product = baseProduct;
                            //List<LocationInfo> baseSkuLocationInfo = await GetLocationInfo(baseSku);
                            //item.LocationInfo = baseSkuLocationInfo;
                            combinedItems.Add(uniqueKey, item);
                        }
                        else
                        {
                            combinedItems.Add(uniqueKey, item);
                        }
                    }
                }
                else
                {
                    combinedItems.Add(uniqueKey, item);
                }
            }

            return combinedItems.Values.ToList();
        }

        public async Task<List<DesignBatchItemViewModel>> HandleDesignSkuSets(List<DesignBatchItemViewModel> designBatchItemViewModels)
        {
            Dictionary<string, DesignBatchItemViewModel> combinedItems = new();

            foreach (DesignBatchItemViewModel item in designBatchItemViewModels)
            {
                string sku = item.Sku;
                string uniqueKey = sku + "_" + item.OrderBatchItemId;

                if (Regex.IsMatch(sku, @"SO0*[1-9][0-9]+|SO0*[2-9]"))
                {
                    string baseSku = GetBaseSku(sku);
                    int multiplier = int.Parse(Regex.Match(sku, @"SO(\d+)").Groups[1].Value);

                    int[] specificProductIds = { 1622, 1623, 1624, 1543, 1544, 1545 };
                    if (specificProductIds.Contains(item.BatchItem.ProductId))
                    {
                        baseSku = GetAdjustedSku(baseSku);
                    }

                    DesignBatchItemViewModel existingBaseSkuViewModel = designBatchItemViewModels.FirstOrDefault(vm => vm.Sku == baseSku);

                    if (existingBaseSkuViewModel != null)
                    {
                        existingBaseSkuViewModel.BatchItem.Quantity += item.BatchItem.Quantity * multiplier;
                        existingBaseSkuViewModel.Quantity += item.Quantity * multiplier;
                    }
                    else
                    {
                        Product baseProduct = await _unitOfWork.Products.FilterOneAsync(x => x.Sku == baseSku);
                        var existingDepartments = item.BatchItem.Product.Departments;
                        if (baseProduct != null)
                        {
                            item.Sku = baseSku;
                            item.BatchItem.Product = baseProduct;
                            item.BatchItem.Product.Departments = existingDepartments;
                            item.BatchItem.Quantity *= multiplier;
                            item.Quantity *= multiplier;
                            combinedItems.Add(uniqueKey, item);
                        }
                        else
                        {
                            combinedItems.Add(uniqueKey, item);
                        }
                    }
                }
                else
                {
                    combinedItems.Add(uniqueKey, item);
                }
            }

            return combinedItems.Values.ToList();
        }

        // This function is, for now, specific to the golf balls. For some reason, the sku layout is different for Set of 100+, so this is to account for it. Will add more exceptions here as they are found.
        private string GetAdjustedSku(string sku)
        {
            if (sku.Contains("HND"))
            {
                sku = sku.Replace("HND", "");
                return Regex.Replace(sku, @"\d+", "01");
            }
            return sku;
        }
        #endregion

        #region Transfer Related
        public async Task<(bool, string, string)> TransferStock(List<StockTransfer> stockTransfers, string currentUserName)
        {
            try
            {
                string newStatusName = null;
                _ = await _unitOfWork.OrderBatch.ExecuteTransactionAsync(async () =>
                {
                    (_, _, newStatusName) = await TransferStockAsync(stockTransfers, currentUserName);
                });

                return (true, "", newStatusName);
            }

            catch (Exception ex)
            {
                return (false, $"Transfer Error: {ex.Message}", null);
            }
        }

        private async Task<(bool, string, string)> TransferStockAsync(List<StockTransfer> stockTransfers, string currentUserName)
        {
            int orderBatchId = 0;
            OrderBatch orderBatch = null;
            string newStatusName = null;

            var fromStocks = new Dictionary<(int, int), Stock>();
            var toStocks = new Dictionary<(int, int), Stock>();
            var moveStockHistories = new List<MoveStockHistory>();
            var orderBatchItemStatusUpdates = new List<(int, int)>();

            foreach (StockTransfer transfer in stockTransfers)
            {
                orderBatch ??= await _unitOfWork.OrderBatch.FilterOneAsync(ob => ob.OrderBatchId == transfer.OrderBatchId);

                if (transfer.FromLocationId == 0 || transfer.ToLocationId == 0)
                {
                    throw new InvalidOperationException("Invalid From or To location.");
                }

                var fromStockKey = (transfer.FromLocationId, transfer.ProductId);
                var toStockKey = (transfer.ToLocationId, transfer.ProductId);

                if (!fromStocks.TryGetValue(fromStockKey, out var fromStock))
                {
                    fromStock = await _unitOfWork.OrderBatch.GetStockByLocationIdAndProductId(transfer.FromLocationId, transfer.ProductId);
                    fromStocks[fromStockKey] = fromStock;
                }

                if (!toStocks.TryGetValue(toStockKey, out var toStock))
                {
                    toStock = await _unitOfWork.OrderBatch.GetStockByLocationIdAndProductId(transfer.ToLocationId, transfer.ProductId);
                    toStocks[toStockKey] = toStock;
                }

                if (fromStock == null || fromStock.TotalAvailable < transfer.Quantity)
                {
                    string sku = await _unitOfWork.OrderBatch.GetSkuByProductId(transfer.ProductId);
                    throw new Exception($"Insufficient stock in the 'From' location for SKU: {sku}");
                }

                var utcNow = DateTime.UtcNow;
                fromStock.TotalAvailable -= transfer.Quantity;
                fromStock.ModifyDate = utcNow;
                fromStock.ModifyByUser = currentUserName;

                if (toStock != null)
                {
                    toStock.TotalAvailable += transfer.Quantity;
                    toStock.ModifyDate = utcNow;
                    toStock.ModifyByUser = currentUserName;
                }
                else
                {
                    toStock = new Stock
                    {
                        LocationId = transfer.ToLocationId,
                        ProductId = transfer.ProductId,
                        TotalAvailable = transfer.Quantity,
                        ModifyDate = utcNow,
                        ModifyByUser = currentUserName
                    };
                    toStocks[toStockKey] = toStock;
                }

                // Creating the MoveStockHistory entry here.
                Product product = await _unitOfWork.Products.FilterOneAsync(x => x.ProductId == fromStock.ProductId);
                MoveStockHistory moveStockHistory = new()
                {
                    FromStock = fromStock,
                    ToStock = toStock,
                    Sku = product.Sku,
                    DateTime = utcNow,
                    Quantity = transfer.Quantity,
                    Type = ActionType.Transfer,
                    EmployeeName = currentUserName
                };
                moveStockHistories.Add(moveStockHistory);

                // Adding to orderBatchItemStatusUpdates for later processing.
                if (transfer.OrderBatchItemIdList != null && transfer.OrderBatchItemIdList.Any())
                {
                    foreach (var orderBatchItemId in transfer.OrderBatchItemIdList)
                    {
                        orderBatchItemStatusUpdates.Add((orderBatchItemId, transfer.ProductId));
                    }
                }
                else
                {
                    orderBatchItemStatusUpdates.Add((transfer.OrderBatchItemId, transfer.ProductId));
                }

                orderBatchId = transfer.OrderBatchId;
            }

            // Bulk updating stocks here so that it's all at once.
            var allStocks = fromStocks.Values.Concat(toStocks.Values).ToList();
            _unitOfWork.Stocks.UpdateRange(allStocks);

            foreach (var stock in fromStocks.Values)
            {
                _unitOfWork.Stocks.Update(stock);
            }
            foreach (var stock in toStocks.Values)
            {
                if (stock.StockId == 0)
                {
                    await _unitOfWork.Stocks.AddAsync(stock);
                }
                else
                {
                    _unitOfWork.Stocks.Update(stock);
                }
            }

            // Bulk inserting MoveStockHistories  
            await _unitOfWork.MoveStockHistories.AddRangeAsync(moveStockHistories);

            // Process orderBatchItemStatusUpdates  for relevant entries.
            foreach (var (orderBatchItemId, productId) in orderBatchItemStatusUpdates)
            {
                newStatusName = await OrderBatchItemStatusTransferUpdates(orderBatchItemId, productId);
            }

            if (orderBatch != null)
            {
                bool allItemsPickedOrCanceled = await AllOrderBatchItemsPickedOrCanceledAsync(orderBatch.OrderBatchId);

                orderBatch.Status = orderBatch.Type == BatchType.Inventory && allItemsPickedOrCanceled
                    ? OrderBatchStatus.Completed
                    : OrderBatchStatus.InProgress;

                await UpdateAsync(orderBatch);
            }

            return (true, null, newStatusName);
        }

        private async Task<bool> AllOrderBatchItemsPickedOrCanceledAsync(int orderBatchId)
        {
            try
            {
                var orderBatchItems = await _unitOfWork.OrderBatchItem
                    .QueryFilter(q => q
                        .Where(obi => obi.OrderBatchId == orderBatchId)
                        .Include(obi => obi.Order)
                    ).ToListAsync();

                return orderBatchItems.All(obi => obi.IsPicked || obi.Order.orderStatus == Order.OrderStatus.cancelled);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> OrderBatchItemStatusTransferUpdates(int orderBatchItemId, int productId)
        {
            string newStatusName = null;

            OrderBatchItem orderBatchItem = await GetOrderBatchItemByOrderBatchItemId(orderBatchItemId);
            if (orderBatchItem != null)
            {
                orderBatchItem.IsPicked = true;
                orderBatchItem.ProductId = productId;

                // Get the current BatchItemStatus  
                BatchItemStatus currentStatus = orderBatchItem.BatchItemStatus;

                // Calculate the increment based on the BatchType  
                int increment = 1;

                // Find the next BatchItemStatus with the same Department and incremented ExecutionSequence  
                BatchItemStatus nextStatus = await GetNextBatchItemStatusByDepartmentAndExecutionSequence(currentStatus.DepartmentId, currentStatus.ExecutionSequence + increment);
                if (nextStatus != null && nextStatus.StatusName == "Picked" && orderBatchItem.IsPicked)
                {
                    orderBatchItem.BatchItemStatusId = nextStatus.BatchItemStatusId;
                }
                await UpdateOrderBatchItem(orderBatchItem);
                newStatusName = nextStatus?.StatusName ?? "Completed";
            }

            return newStatusName;
        }

        public async Task<Product> GetProductByOrderBatchItemId(int orderBatchItemId)
        {
            var orderBatchItem = await _unitOfWork.OrderBatchItem.FilterOneAsync(x => x.OrderBatchItemId == orderBatchItemId, p => p.Product);
            return orderBatchItem?.Product;
        }

        #endregion

        #region Order Removal

        // Not in use right now, but we may re-implement batch item removal, and this code would come in handy there.
        public async Task<bool> RemoveOrders(int cwaOrderId, int orderBatchId)
        {
            bool result = false;

            _ = await _unitOfWork.OrderBatch.ExecuteTransactionAsync(async () =>
            {
                List<OrderBatchItem> orderBatchItems = await _unitOfWork.OrderBatch.GetOrderBatchItemsByOrderBatchId(orderBatchId);
                var itemsToRemove = orderBatchItems.Where(item => item.ERPOrderId == cwaOrderId).ToList();

                if (itemsToRemove.Count > 0)
                {
                    foreach (var item in itemsToRemove)
                    {
                        if (item.IsPicked)
                        {
                            // Undo the transfer  
                            await UndoTransfer(item);
                        }
                    }

                    await _unitOfWork.OrderBatchItem.RemoveRangeAsync(itemsToRemove);
                    _ = await _unitOfWork.SaveChangesAsync();
                    result = true;
                }
            });

            return result;
        }

        public async Task UndoTransfer(OrderBatchItem item)
        {
            // Retrieve the product to get the SKU using the ProductId  
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                // Handle the case where the product is not found  
                return;
            }

            var sku = product.Sku;

            // Retrieve the MoveStockHistory entries for the corresponding SKU, quantity, and ActionType.Transfer  
            var moveStockHistories = await _unitOfWork.MoveStockHistories.GetListByFilterAsync(
                msh => msh.Sku == sku && msh.Quantity == item.Quantity && msh.Type == ActionType.Transfer
            );

            // Get the most recent entry based on DateTime  
            var moveStockHistory = moveStockHistories.OrderByDescending(msh => msh.DateTime).FirstOrDefault();

            if (moveStockHistory != null)
            {
                // Update the stock levels  
                var fromStock = await _unitOfWork.Stocks.GetByIdAsync(moveStockHistory.FromStockId.Value);
                var toStock = await _unitOfWork.Stocks.GetByIdAsync(moveStockHistory.ToStockId.Value);

                if (fromStock != null && toStock != null)
                {
                    fromStock.TotalAvailable += moveStockHistory.Quantity;
                    toStock.TotalAvailable -= moveStockHistory.Quantity;

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> AnyItemsPickedAsync(int orderBatchId, int cwaOrderId)
        {
            var orderBatchItems = await _unitOfWork.OrderBatchItem.GetListByFilterAsync(
                ob => ob.OrderBatchId == orderBatchId && ob.ERPOrderId == cwaOrderId
            );

            return orderBatchItems.Any(item => item.IsPicked);
        }

        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchId(int orderBatchId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchItemsByOrderBatchId(orderBatchId);
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(int orderBatchId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(orderBatchId);
        }
        public async Task<OrderBatchItem> GetOrderBatchItemByOrderBatchItemId(int orderBatchItemId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchItemByOrderBatchItemId(orderBatchItemId);
        }

        public async Task<BatchItemStatus> GetNextBatchItemStatusByDepartmentAndExecutionSequence(int departmentId, int executionSequence)
        {
            return await _unitOfWork.OrderBatch.GetNextBatchItemStatusByDepartmentAndExecutionSequence(departmentId, executionSequence);
        }
        public async Task<OrderBatchItem> UpdateOrderBatchItem(OrderBatchItem orderBatchItem)
        {
            return await _unitOfWork.OrderBatch.UpdateOrderBatchItem(orderBatchItem);
        }

        #endregion

        public async Task<List<Order>> GetOrdersWithProductsByERPOrderIdsAsync(List<int> cwaOrderIds)
        {
            return await _unitOfWork.OrderBatch.GetOrdersWithProductsByERPOrderIdsAsync(cwaOrderIds);
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            return _unitOfWork.Products.GetListByFilter(p => p.IsActive).ToList();
        }
        public async Task<OrderBatchItem> GetOrderBatchItemByERPOrderId(int cwaOrderId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchItemByERPOrderId(cwaOrderId);
        }

        public async Task<List<SimplifiedInventoryPickList>> GetSimplifiedPickListDetailsByBatchNumberAsync(string batchNumber, bool includeImages)
        {
            var pickListDetails = await _unitOfWork.OrderBatch.GetSimplifiedPickListDetailsByBatchNumberAsync(batchNumber, includeImages);

            return pickListDetails;
        }
        public async Task<List<ExpandedPickList>> GetExpandedPickListDetailsByOrderBatchIdAsync(int orderBatchId)
        {
            var pickListDetails = await _unitOfWork.OrderBatch.GetExpandedPickListDetailsByOrderBatchIdAsync(orderBatchId);
            return pickListDetails;
        }

        public async Task<string> GetCompleteBatchNumberByBatchNumberAsync(string batchNumber)
        {
            return await _unitOfWork.OrderBatch.GetCompleteBatchNumberByBatchNumberAsync(batchNumber);
        }

        public async Task<List<DesignBatchItemViewModel>> GetDesignBatchItemsAsync(int orderBatchId)
        {
            var orderBatchItems = await _unitOfWork.OrderBatchItem.QueryFilter(
                query => query
                    .Where(obi => obi.OrderBatchId == orderBatchId)
                    .Include(obi => obi.Product)
                        .ThenInclude(p => p.Departments)
                        .Include(obi => obi.BatchItemStatus)
                        .Include(obi => obi.Order) 
                        .Where(obi => obi.Order.orderStatus != Order.OrderStatus.cancelled)
            ).ToListAsync();

            var viewModelList = orderBatchItems.Select(item => new DesignBatchItemViewModel
            {
                ProductId = item.ProductId,
                Sku = item.Product.Sku,
                Quantity = item.Quantity,
                Description = item.Product.Description,
                Status = item.BatchItemStatus,
                OrderBatchId = orderBatchId,
                OrderBatchItemId = item.OrderBatchItemId,
                BatchItem = item,
                ERPOrderId = item.ERPOrderId,
                DepartmentName = string.Join(", ", item.Product.Departments.Select(d => d.DepartmentName))
            }).ToList();

            return await HandleDesignSkuSets(viewModelList);
        }

        public async Task<bool> IsValidStock(int productId)
        {
            return await _unitOfWork.Stocks.IsExistsAsync(s => s.ProductId == productId);
        }

        public async Task<BatchItemStatus> GetProductStatusByOrderBatchItemIdAsync(int orderBatchItemId)
        {
            var productStatus = await _unitOfWork.OrderBatch.GetProductStatusByOrderBatchItemIdAsync(orderBatchItemId);

            return productStatus;
        }
        public async Task<List<int>> GetDepartmentIdsByProductIdAsync(int productId)
        {
            // Use FilterOneAsync with includes to retrieve the product along with its departments  
            var product = await _unitOfWork.Products.FilterOneAsync(
                p => p.ProductId == productId,
                includes: new Expression<Func<Product, object>>[] { p => p.Departments }
            );

            if (product != null && product.Departments != null)
            {
                return product.Departments.Select(d => d.DepartmentId).ToList();
            }
            return new List<int>();
        }

        public async Task<OrderBatchItem> UpdateOrderBatchProgressAsync(int orderBatchItemId)
        {
            // Update the OrderBatchItem status  
            OrderBatchItem Obi = await UpdateOrderBatchItemStatusAsync(orderBatchItemId);

            // Check if all OrderBatchItems are completed and update the OrderBatchStatus  
            await CheckAndUpdateOrderBatchStatusAsync(Obi.OrderBatchId);
            return Obi;
        }
        // Overload to handle changing batch status.
        public async Task<OrderBatchItem> UpdateOrderBatchProgressAsync(int orderBatchItemId, int? desiredBatchStatusId)
        {
            // Update the OrderBatchItem status  
            OrderBatchItem obi = await UpdateOrderBatchItemStatusAsync(orderBatchItemId, desiredBatchStatusId);

            // Check if all OrderBatchItems are completed and update the OrderBatchStatus  
            await CheckAndUpdateOrderBatchStatusAsync(obi.OrderBatchId);
            return obi;
        }

        public async Task<OrderBatchItem> UpdateOrderBatchItemStatusAsync(int orderBatchItemId, int? desiredBatchStatusId = null)
        {
            try
            {
                // Get the OrderBatchItem from the database  
                var orderBatchItem = await _unitOfWork.OrderBatch.GetOrderBatchItemByIdAsync(orderBatchItemId);

                // If a desiredBatchStatusId is provided, we'll use it to update the BatchItemStatusId  
                if (desiredBatchStatusId.HasValue)
                {
                    var desiredBatchStatus = await _unitOfWork.OrderBatch.GetBatchItemStatusByIdAsync(desiredBatchStatusId.Value);
                    if (desiredBatchStatus == null)
                    {
                        throw new InvalidOperationException("The specified BatchItemStatusId does not exist.");
                    }
                    orderBatchItem.BatchItemStatusId = desiredBatchStatusId.Value;
                    orderBatchItem.IsPicked = false;
                    orderBatchItem.IsCompleted = false;
                }
                else
                {
                    // Default status update logic
                    int currentBatchItemStatusId = orderBatchItem.BatchItemStatusId;
                    var currentBatchItemStatus = await _unitOfWork.OrderBatch.GetBatchItemStatusByIdAsync(currentBatchItemStatusId);
                    int departmentId = currentBatchItemStatus.DepartmentId;
                    int currentExecutionSequence = currentBatchItemStatus.ExecutionSequence;
                    var nextBatchItemStatus = await _unitOfWork.OrderBatch.GetNextBatchItemStatusAsync(departmentId, currentExecutionSequence);
                    var nextNextBatchItemStatus = await _unitOfWork.OrderBatch.GetNextBatchItemStatusAsync(departmentId, currentExecutionSequence + 1);

                    if (nextBatchItemStatus != null)
                    {
                        if (nextNextBatchItemStatus != null && nextNextBatchItemStatus.StatusName == "Picked" && orderBatchItem.IsPicked)
                        {
                            orderBatchItem.BatchItemStatusId = nextNextBatchItemStatus.BatchItemStatusId;
                        }
                        else if (nextNextBatchItemStatus == null)
                        {
                            if (orderBatchItem.IsPicked)
                            {
                                orderBatchItem.IsCompleted = true;
                                orderBatchItem.BatchItemStatusId = nextBatchItemStatus.BatchItemStatusId;
                            }
                            else
                            {
                                throw new InvalidOperationException("Unable to complete the order because IsPicked is false.");
                            }
                        }
                        else
                        {
                            orderBatchItem.BatchItemStatusId = nextBatchItemStatus.BatchItemStatusId;
                        }
                    }
                    else
                    {
                        orderBatchItem.IsCompleted = true;
                        if (nextBatchItemStatus == null)
                        {
                            return orderBatchItem;
                        }
                        orderBatchItem.BatchItemStatusId = nextBatchItemStatus.BatchItemStatusId;
                    }
                }

                // Save changes to the database  
                await _unitOfWork.OrderBatch.UpdateOrderBatchItemAsync(orderBatchItem);
                // Return the OrderBatchId  
                return orderBatchItem;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while updating the OrderBatchItem status: {ex}");
            }
        }
        public async Task UpdateOrderBatchItemStatusAsync(int orderBatchItemId, int batchItemStatusId)
        {
            var orderBatchItem = await _unitOfWork.OrderBatchItem.FilterOneAsync(obi => obi.OrderBatchItemId == orderBatchItemId);
            if (orderBatchItem != null)
            {
                orderBatchItem.BatchItemStatusId = batchItemStatusId;
                _unitOfWork.OrderBatchItem.Update(orderBatchItem);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task CheckAndUpdateOrderBatchStatusAsync(int orderBatchId)
        {
            // Check if all OrderBatchItems of the current OrderBatchId have IsCompleted set to true    
            bool allItemsCompleted = await _unitOfWork.OrderBatch.AllItemsCompletedForOrderBatchAsync(orderBatchId);

            // Update the OrderBatchStatus if all items are completed    
            if (allItemsCompleted)
            {
                var orderBatch = await GetAsync(ob => ob.OrderBatchId == orderBatchId);
                orderBatch.Status = OrderBatchStatus.Completed;
                await UpdateAsync(orderBatch);
            }
        }

        public async Task<OrderBatchItem> GetBatchItemByOrderBatchItemIdAsync(int orderBatchItemId)
        {
            return await _unitOfWork.OrderBatch.GetBatchItemByOrderBatchItemIdAsync(orderBatchItemId);
        }

        public async Task<List<Product>> GetAllProductsWithAltItemNumbersAndStockAsync()
        {
            var productsWithAltItemNumbers = await _unitOfWork.Products.GetListByFilterAsync(
                p => p.AltItemNumber != null && p.AltItemNumber != "NULL" && p.IsActive
            );

            var availableStocks = await _unitOfWork.Stocks.GetListByFilterAsync(s => s.TotalAvailable > 0);

            // Making sure that we're only grabbing PickOnly stocks. 
            var validLocations = await _unitOfWork.Locations.GetListByFilterAsync(l => l.Type == LocationType.PickOnly);

            var result = productsWithAltItemNumbers.Join(
                availableStocks,
                product => product.ProductId,
                stock => stock.ProductId,
                (product, stock) => new { product, stock }
            )
            .Join(
                validLocations,
                ps => ps.stock.LocationId,
                location => location.LocationId,
                (ps, location) => ps.product
            )
            .Distinct()
            .ToList();

            return result;
        }

        public async Task<string> GetSkuByProductId(int productId)
        {
            return await _unitOfWork.OrderBatch.GetSkuByProductId(productId);
        }
        public async Task<List<DepartmentStatusLineViewModel>> GetDepartmentStatusLinesAsync(int orderBatchId)
        {
            // Get the order batch items associated with the orderBatchId  
            var orderBatchItems = await _unitOfWork.OrderBatchItem.QueryFilter(
                query => query
                    .Where(obi => obi.OrderBatchId == orderBatchId)
                    .Include(obi => obi.Product)
                        .ThenInclude(p => p.Departments)
            ).ToListAsync();

            // Extract the distinct departments from the order batch items  
            var departments = orderBatchItems
                .SelectMany(obi => obi.Product.Departments)
                .Distinct()
                .ToList();

            // Create a list to store the department status lines  
            var departmentStatusLines = new List<DepartmentStatusLineViewModel>();

            // Iterate through each department and generate the status lines  
            foreach (var department in departments)
            {
                // Get the BatchItemStatus records for the current department  
                var statuses = await _unitOfWork.BatchItemStatus.QueryFilter(
                    query => query
                        .Where(bis => bis.DepartmentId == department.DepartmentId)
                        .OrderBy(bis => bis.ExecutionSequence)
                ).ToListAsync();

                // Add the department and its statuses to the list  
                departmentStatusLines.Add(new DepartmentStatusLineViewModel
                {
                    Department = department,
                    Statuses = statuses
                });
            }

            return departmentStatusLines;
        }
        public async Task UpdateOrderBatchItemsStatusToCompletedAsync(string orderNumber)
        {
            var orderBatchItems = await _unitOfWork.OrderBatchItem.QueryFilter(
                query => query
                    .Where(obi => obi.OrderNumber == orderNumber)
            ).ToListAsync();

            HashSet<int> uniqueBatchIds = new HashSet<int>();

            foreach (var orderBatchItem in orderBatchItems)
            {
                int currentBatchItemStatusId = orderBatchItem.BatchItemStatusId;
                var currentBatchItemStatus = await _unitOfWork.OrderBatch.GetBatchItemStatusByIdAsync(currentBatchItemStatusId);
                int departmentId = currentBatchItemStatus.DepartmentId;
                int currentExecutionSequence = currentBatchItemStatus.ExecutionSequence;
                var completedStatus = await _unitOfWork.OrderBatch.GetLastBatchItemStatusAsync(departmentId);

                if (completedStatus != null)
                {
                    orderBatchItem.BatchItemStatusId = completedStatus.BatchItemStatusId;
                    orderBatchItem.IsCompleted = true;
                }

                uniqueBatchIds.Add(orderBatchItem.OrderBatchId);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (int batchId in uniqueBatchIds)
            {
                await CheckAndUpdateOrderBatchStatusAsync(batchId);
            }
        }
        public async Task<Department> GetDepartmentByIdAsync(int departmentId)
        {
            return await _unitOfWork.OrderBatch.GetDepartmentForBatchItemByIdAsync(departmentId);
        }
        public async Task<List<Department>> GetActiveDepartmentsAsync()
        {
            return await _unitOfWork.Departments.GetListByFilterAsync(d => d.IsActive);
        }
        public async Task<List<OrderBatch>> GetFilteredOrderBatchesAsync()
        {
            return await _unitOfWork.OrderBatch.GetFilteredOrderBatchesAsync();
        }
        public async Task<List<OrderBatch>> GetOrderBatchesWithoutPickedItems()
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchesWithoutPickedItems();
        }
        public async Task<List<DepartmentStatusDto>> GetDepartmentStatusesAsync()
        {
            var batchItemStatuses = await _unitOfWork.BatchItemStatus.QueryFilter(
                query => query.Include(b => b.Department)
            ).ToListAsync();

            var departmentStatuses = batchItemStatuses
                .GroupBy(b => b.Department)
                .Select(group => new DepartmentStatusDto
                {
                    DepartmentId = group.Key.DepartmentId,
                    DepartmentName = group.Key.DepartmentName,
                    Statuses = group.Select(status => new BatchItemStatusDto
                    {
                        BatchItemStatusId = status.BatchItemStatusId,
                        StatusName = status.StatusName,
                        ExecutionSequence = status.ExecutionSequence
                    }).ToList()
                }).ToList();

            return departmentStatuses;
        }
        public async Task<BatchOperationResult> AddOrdersToBatchAsync(int batchId, List<int> cwaOrderIds, List<AssignedDepartment> assignedDepartments = null, List<ReplacementSku> replacementSkus = null)
        {
            var result = new BatchOperationResult();

            try
            {
                // Fetch the orders to be added to the batch  
                var orders = await _unitOfWork.Orders.GetListByFilterAsync(
                    o => cwaOrderIds.Contains(o.ERPOrderId),
                    includes: [o => o.items]);

                if (orders == null || !orders.Any())
                {
                    result.Success = false;
                    result.Message = "No valid orders found.";
                    return result;
                }

                // Generate the pick lists  
                List<InventoryPickList> inventoryPickLists = new List<InventoryPickList>();
                foreach (Order order in orders)
                {
                    foreach (OrderItem item in order.items)
                    {
                        if (item.name == "Discount" || item.adjustment == true || item.unitPrice < 0) continue;

                        var replacement = replacementSkus?.FirstOrDefault(r => r.OriginalSku == item.sku);
                        var assignedDepartment = assignedDepartments?.FirstOrDefault(ad => ad.OrderItemId == item.orderItemId);
                        if (assignedDepartment == null)
                        {
                            assignedDepartment = assignedDepartments?.FirstOrDefault(ad => ad.OrderItemId == item.ERPOrderItemId);
                        }

                        if (item.Bundle != null)
                        {
                            foreach (var bundleItem in item.Bundle.BundleItems)
                            {
                                InventoryPickList pickList = await CreatePickListItem(bundleItem.Product.Sku, bundleItem.Product.Description, bundleItem.Quantity, bundleItem.ProductId, item.ERPOrderId, order, replacement, assignedDepartment, item);
                                inventoryPickLists.Add(pickList);
                            }
                        }
                        else
                        {
                            InventoryPickList pickList = await CreatePickListItem(item.sku, item.name, item.quantity, item.ERPProductId ?? 0, item.ERPOrderItemId, order, replacement, assignedDepartment, item);
                            inventoryPickLists.Add(pickList);
                        }
                    }
                }

                // Check for missing SKUs  
                result.MissingSkus = await GetMissingSkusListAsync(inventoryPickLists);
                if (result.MissingSkus.Count > 0)
                {
                    result.Success = false;
                    result.Message = "Missing SKUs found.";
                    return result;
                }

                // Check for unassigned departments  
                result.UnassignedDepartments = await GetUnassignableDepartments(inventoryPickLists);
                if (result.UnassignedDepartments.Count > 0)
                {
                    result.Success = false;
                    result.Message = "Unassigned departments found.";
                    return result;
                }

                // Create OrderBatchItems using the existing function  
                await _unitOfWork.OrderBatch.CreateOrderBatchItems(batchId, inventoryPickLists, true);

                result.Success = true;
                result.Message = "Orders added to batch successfully.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<Dictionary<int, string>> GetOrderBatchNumbersByOrderIds(List<int> orderIds)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchNumbersByOrderIds(orderIds);
        }
        public async Task<Dictionary<int, List<string>>> GetOrderBatchNumberByOrderId(int orderId)
        {
            return await _unitOfWork.OrderBatch.GetOrderBatchNumberByOrderId(orderId);
        }

        public async Task<bool> UpdateOrderBatchPurchaseOrderDetailsAsync(int purchaseOrderId, List<int> batchIds)
        {
            // Check if all OrderBatchItems of the current OrderBatchId have IsCompleted set to true    
            bool succesfullyUpdated = await _unitOfWork.OrderBatch.UpdateOrderBatchPurchaseOrderDetails( purchaseOrderId,  batchIds);

            // Update the OrderBatchStatus if all items are completed    
            return succesfullyUpdated;
        }

        public async Task<bool> UndoBatchPOIdAssignmentAsync(int purchaseOrderId, List<int> batchIds)
        {
            bool succesfullyUpdated = await _unitOfWork.OrderBatch.UndoBatchPOIdAssignment(purchaseOrderId, batchIds);

            return succesfullyUpdated;
        }

        public async Task<bool> SetRequiresPoAsync(int orderBatchId)
        {
            var orderBatch = await _unitOfWork.OrderBatch.GetByIdAsync(orderBatchId);
            if (orderBatch == null)
            {
                return false;
            }

            orderBatch.RequiresPO = true;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
