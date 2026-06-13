using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using Microsoft.EntityFrameworkCore;
using static ERPWebApp.Models.Orders.Order;
using System.Text.RegularExpressions;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class OrderBatchRepository : Repository<OrderBatch>, IOrderBatchRepository
    {
        private readonly DateTime _currentDateTime;
        public OrderBatchRepository(ApplicationDbContext context) : base(context)
        {

        }

        #region Batch Creation Related
        public static DateTime ConvertUtcToCentralTime(DateTime utcTime)
        {
            TimeZoneInfo cstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, cstTimeZone);

            return cstTime;
        }

        public async Task<OrderBatch> CreateOrderBatch(string batchNumber, string User, BatchType? batchType = null, bool IsDeductible = true)
        {
            OrderBatch orderBatch = new()
            {
                CreateDate = ConvertUtcToCentralTime(DateTime.UtcNow),
                CreateBy = User,
                Status = OrderBatchStatus.Open,
                BatchNumber = batchNumber,
                Type = batchType,
                IsDeductible = IsDeductible
            };
            _ = await AddAsync(orderBatch);
            _ = await _context.SaveChangesAsync();

            orderBatch.BatchNumber += " " + orderBatch.OrderBatchId;
            _ = await _context.SaveChangesAsync();

            return orderBatch;
        }

        public async Task<List<OrderBatchItem>> CreateOrderBatchItems(int orderBatchId, List<InventoryPickList> inventoryPickList, bool isDeductible)
        {
            if (inventoryPickList == null || !inventoryPickList.Any())
            {
                //If no pick list items are passed in, there's clearly an issue and this needs to fail.
                throw new InvalidOperationException("Pick list is empty. Please try again.");
            }

            List<OrderBatchItem> orderBatchItems = new();
            var uniqueERPOrderIds = inventoryPickList.Select(item => item.ERPOrderId).Distinct().ToList();

            var orderItems = await _context.OrderItem
                .Where(oi => uniqueERPOrderIds.Any(id => id == oi.ERPOrderId))
                .Include(OrderItem => OrderItem.Product)
                .Include(OrderItem => OrderItem.Bundle)
                .ThenInclude(Bundle => Bundle.BundleItems)
                .ToListAsync();

            foreach (InventoryPickList item in inventoryPickList)
            {
                // Get the DepartmentId using the ERPOrderItemId  
                var orderItemId = item.ERPOrderItemId;
                int departmentId = item.Department;

                OrderItem orderItem = orderItems.FirstOrDefault(oi => oi.ERPOrderItemId == orderItemId);

                // Find the BatchItemStatus entry with the same DepartmentId and ExecutionSequence set to 1  
                var batchItemStatus = await _context.BatchItemStatus
                    .FirstOrDefaultAsync(bis => bis.DepartmentId == departmentId && bis.ExecutionSequence == 1);
                int batchItemStatusId = batchItemStatus?.BatchItemStatusId ?? 0;

                var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == item.ERPProductId);
                if (product.Departments == null || !product.Departments.Any())
                {
                    var department = await _context.Department.FindAsync(departmentId);
                    if (department != null)
                    {
                        product.Departments = new List<Department> { department };
                    }
                }

                // Initialize the adjusted quantity to the original quantity  
                int adjustedQuantity = item.Quantity;

                orderBatchItems.Add(new OrderBatchItem
                {
                    OrderBatchId = orderBatchId,
                    ProductId = item.ERPProductId,
                    Product = product,
                    ERPOrderItemId = item.ERPOrderItemId,
                    ERPOrderId = item.ERPOrderId,
                    OrderNumber = item.OrderNumber,
                    Quantity = adjustedQuantity,
                    OrderItem = orderItem,
                    BatchItemStatusId = batchItemStatusId,
                    IsPicked = !isDeductible
                });

                if (orderItem.Product == null && orderItem.ERPOrderItemId == item.ERPOrderItemId && orderItem.ERPBundleId == null && !orderItem.adjustment)
                {
                    orderItem.Product = product;
                    orderItem.ERPProductId = product.ProductId;
                    _context.Update(orderItem);
                }

            }

            if (orderBatchItems == null || !orderBatchItems.Any())
            {
                //If not batch items were created, there's clearly an issue and this needs to fail.
                throw new InvalidOperationException("Batch items were not created. Please try again.");
            }

            await _context.OrderBatchItem.AddRangeAsync(orderBatchItems);
            await _context.SaveChangesAsync();
            return orderBatchItems;
        }
        #endregion

        #region Batch Dropdown Related
        public async Task<List<Product>> GetFilteredProducts(string skuPrefix)
        {
            return await _context.Product
                .Where(p => p.Sku.StartsWith(skuPrefix) && p.IsActive && !string.IsNullOrEmpty(p.AltItemNumber))
                .ToListAsync();
        }
        #endregion
        #region Post-Table Creation
        public bool IsAltItemCheck(string sku)
        {
            // First, try to find the product in the Product table  
            Product product = _context.Product.FirstOrDefault(p => p.Sku == sku);

            // If the product is not found in the Product table, search for it in the OrderItem table  
            if (product == null)
            {
                // Find the most recent non-null ProductId for the given SKU in the OrderItem table  
                int? mostRecentProductId = _context.OrderItem
                    .Where(oi => oi.sku == sku && oi.ERPProductId != null)
                    .OrderByDescending(oi => oi.orderItemId) // Assuming OrderItemId is an auto-incrementing ID  
                    .Select(oi => oi.ERPProductId)
                    .FirstOrDefault();

                // If a ProductId is found, look for the product in the Product table  
                if (mostRecentProductId != null)
                {
                    product = _context.Product.FirstOrDefault(p => p.ProductId == mostRecentProductId.Value);
                }
            }

            // If the product is still not found or its AltItemNumber is null, return false  
            return product != null && product.AltItemNumber != "NULL";
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchId(int orderBatchId)
        {
            var query = _context.OrderBatchItem
                                .Include(obi => obi.OrderBatch)
                                .Include(obi => obi.Product)
                                .Include(obi => obi.BatchItemStatus)
                                .Include(obi => obi.Order)
                                    .ThenInclude(order => order.shipTo)
                                .Include(obi => obi.OrderItem)
                                .Where(obi => obi.OrderBatchId == orderBatchId);

            var result = await query.ToListAsync();

            return result;
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByOrderBatchIdWithoutItemInfo(int orderBatchId)
        {
            var query = _context.OrderBatchItem
                                .Include(obi => obi.Product)
                                .Include(obi => obi.BatchItemStatus)
                                .Include(obi => obi.Order)
                                .Where(obi => obi.OrderBatchId == orderBatchId);

            var result = await query.ToListAsync();

            return result;
        }
        public async Task<OrderBatchItem> GetOrderBatchItemByOrderBatchItemId(int orderBatchItemId)
        {
            return await _context.OrderBatchItem
                .Include(obi => obi.Product)
                .Include(obi => obi.BatchItemStatus)
                .FirstOrDefaultAsync(obi => obi.OrderBatchItemId == orderBatchItemId);
        }

        public async Task<BatchItemStatus> GetNextBatchItemStatusByDepartmentAndExecutionSequence(int departmentId, int executionSequence)
        {
            return await _context.BatchItemStatus
                .FirstOrDefaultAsync(bis => bis.DepartmentId == departmentId && bis.ExecutionSequence == executionSequence);
        }

        public async Task<OrderBatchItem> UpdateOrderBatchItem(OrderBatchItem orderBatchItem)
        {
            _context.Entry(orderBatchItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return orderBatchItem;
        }

        #endregion

        #region Location Info Related
        public async Task<List<Stock>> GetStocksBySku(string sku)
        {
            List<Stock> stocks = await _context.Stock
                .Where(x => x.Products.Sku == sku && x.TotalAvailable != null)
                .ToListAsync();

            return stocks;
        }
        public async Task<List<Location>> GetLocationsByStocks(List<Stock> stocks)
        {
            List<int> locationIds = stocks.Select(x => x.LocationId).Distinct().ToList();
            List<Location> locations = await _context.Location
                .Where(l => locationIds.Contains(l.LocationId) && l.IsActive == true)
                .ToListAsync();

            return locations;
        }
        public async Task<List<Location>> GetReceiveOnlyLocations()
        {
            List<Location> receiveOnlyLocations = await _context.Location
                .Where(l => l.Type == LocationType.ReceiveOnly)
                .ToListAsync();

            return receiveOnlyLocations;
        }
        #endregion
        #region Transfer Related
        public async Task<Stock> GetStockByLocationIdAndProductId(int locationId, int productId)
        {
            return await _context.Stock
                .Include(s => s.Products)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(s => s.LocationId == locationId && s.ProductId == productId);
        }
        public async Task<string> GetSkuByProductId(int productId)
        {
            Product product = await _context.Product.FirstOrDefaultAsync(x => x.ProductId == productId);
            return product.Sku;
        }
        #endregion
        #region Transactions
        public async Task<bool> ExecuteTransactionAsync(Func<Task> action)
        {
            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
            try
            {
                await action();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        #endregion

        public async Task<List<Order>> GetOrdersWithProductsByERPOrderIdsAsync(List<int> cwaOrderIds)
        {
            var orders = _context.Orders
                .Include(o => o.items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(d => d.Departments)
                .Include(a => a.advancedOptions)
                .Include(o => o.items)
                    .ThenInclude(i => i.Bundle)
                        .ThenInclude(b => b.BundleItems)
                            .ThenInclude(bi => bi.Product)
                                .ThenInclude(d => d.Departments)
                .Include(o => o.Sources)
                .Where(o => cwaOrderIds.Contains(o.ERPOrderId) && o.orderStatus != OrderStatus.cancelled && o.orderStatus != OrderStatus.shipped);

            return await orders.ToListAsync();
        }

        public async Task<OrderBatchItem> GetOrderBatchItemByERPOrderId(int cwaOrderId)
        {
            return await _context.OrderBatchItem
                        .Include(obi => obi.OrderBatch)
                        .FirstOrDefaultAsync(m => m.ERPOrderId == cwaOrderId);
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByERPOrderIds(List<int> ERPOrderIds)
        {
            return await _context.OrderBatchItem
                .Include(obi => obi.OrderBatch)
                .Where(obi => ERPOrderIds.Contains(obi.ERPOrderId.Value))
                .ToListAsync();
        }

        public async Task<List<SimplifiedInventoryPickList>> GetSimplifiedPickListDetailsByBatchNumberAsync(string batchNumber, bool includeImages)
        {
            // Get the OrderBatch Id by the batch number  
            var orderBatchId = await _context.OrderBatch
                .Where(ob => ob.BatchNumber == batchNumber)
                .Select(ob => ob.OrderBatchId)
                .FirstOrDefaultAsync();

            // Get the pick list details from the OrderBatchItem table  
            var pickListDetails = await _context.OrderBatchItem
                .Where(obi => obi.OrderBatchId == orderBatchId && obi.Order.orderStatus != OrderStatus.cancelled)
                .Include(obi => obi.Product)
                .ThenInclude(obi => obi.ProductImages)
                .GroupBy(obi => new { obi.Product.Sku, obi.Product.Description })
                .Select(group => new
                {
                    Sku = group.Key.Sku,
                    Quantity = group.Sum(obi => obi.Quantity),
                    Description = group.Key.Description,
                    ProductId = group.First().ProductId,
                    ImageUrl = group.First().Product.ProductImages
                        .Where(pi => pi.IsDefault) // Getting the default image, but leaving a comment here in case we need to instead grab a specific one.  
                        .Select(pi => pi.FileUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var simplifiedPickList = new List<SimplifiedInventoryPickList>();
            foreach (var item in pickListDetails)
            {
                // Get the location with the most stock for the current productId and is of type PickOnly  
                var locationWithMostStock = await _context.Stock
                    .Where(s => s.ProductId == item.ProductId && s.TotalAvailable > 0 && s.Location.Type == LocationType.PickOnly)
                    .OrderByDescending(s => s.TotalAvailable)
                    .Select(s => s.Location.LocationName)
                    .FirstOrDefaultAsync();

                simplifiedPickList.Add(new SimplifiedInventoryPickList
                {
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    Description = item.Description,
                    FromLocation = locationWithMostStock,
                    ImageUrl =  includeImages ? item.ImageUrl : null
                });
            }

            // Need to sort the simplifiedPickList by Row [Ascending], Bay [Descending], Shelf [Descending], and then by sku alphabetically.  
            var sortedPickList = simplifiedPickList
                .Select(item => new
                {
                    item,
                    ParsedLocation = ParseLocation(item.FromLocation)
                })
                .OrderBy(x => x.ParsedLocation.Row)
                .ThenByDescending(x => x.ParsedLocation.Bay)
                .ThenByDescending(x => x.ParsedLocation.Shelf)
                .ThenBy(x => x.item.FromLocation == null ? 1 : 0) // Making sure items with no location are sorted to the end.
                .ThenBy(x => x.item.Sku)
                .Select(x => x.item)
                .ToList();

            return sortedPickList;
        }

        public async Task<List<ExpandedPickList>> GetExpandedPickListDetailsByOrderBatchIdAsync(int orderBatchId)
        {
            // Get the batch number using the orderBatchId  
            var batchNumber = await _context.OrderBatch
                .Where(ob => ob.OrderBatchId == orderBatchId)
                .Select(ob => ob.BatchNumber)
                .FirstOrDefaultAsync();

            // Get the pick list details from the OrderBatchItem table  
            var pickListDetails = await _context.OrderBatchItem
                .Where(obi => obi.OrderBatchId == orderBatchId && obi.Order.orderStatus != OrderStatus.cancelled)
                .Include(obi => obi.Product)
                .Select(obi => new
                {
                    obi.Product.Sku,
                    obi.Quantity,
                    obi.Product.Description,
                    obi.OrderBatchItemId,
                    obi.ProductId
                })
                .ToListAsync();

            var expandedPickList = new List<ExpandedPickList>();
            foreach (var item in pickListDetails)
            {
                // Get the location with the most stock for the current productId and is of type PickOnly  
                var locationWithMostStock = await _context.Stock
                    .Where(s => s.ProductId == item.ProductId && s.TotalAvailable > 0 && s.Location.Type == LocationType.PickOnly)
                    .OrderByDescending(s => s.TotalAvailable)
                    .Select(s => s.Location.LocationName)
                    .FirstOrDefaultAsync();

                expandedPickList.Add(new ExpandedPickList
                {
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    Description = item.Description,
                    OrderBatchItemId = item.OrderBatchItemId,
                    OrderBatchId = orderBatchId,
                    BatchNumber = batchNumber,
                    FromLocation = locationWithMostStock
                });
            }

            // Need to sort the simplifiedPickList by Row [Ascending], Bay [Descending], Shelf [Descending], to make things easier for membrane. 
            var sortedPickList = expandedPickList
                .Select(item => new
                {
                    item,
                    ParsedLocation = ParseLocation(item.FromLocation)
                })
                .OrderBy(x => x.ParsedLocation.Row)
                .ThenByDescending(x => x.ParsedLocation.Bay)
                .ThenByDescending(x => x.ParsedLocation.Shelf)
                .Select(x => x.item)
                .ToList();

            return sortedPickList;
        }

        private (int Row, int Bay, int Shelf) ParseLocation(string locationName)
        {
            if (string.IsNullOrEmpty(locationName))
            {
                // Return default values if the locationName is null or empty. Important if we have no stock for a given product.
                return (0, 0, 0);
            }

            // According to the ticket, the location name format is MR{Row}.B{Bay}.S{Shelf}  
            var rowMatch = Regex.Match(locationName, @"MR(\d+)");
            var bayMatch = Regex.Match(locationName, @"B(\d+)");
            var shelfMatch = Regex.Match(locationName, @"S(\d+)");

            int row = rowMatch.Success ? int.Parse(rowMatch.Groups[1].Value) : 0;
            int bay = bayMatch.Success ? int.Parse(bayMatch.Groups[1].Value) : 0;
            int shelf = shelfMatch.Success ? int.Parse(shelfMatch.Groups[1].Value) : 0;

            return (row, bay, shelf);
        }

        public async Task<string> GetCompleteBatchNumberByBatchNumberAsync(string batchNumber)
        {
            var orderBatch = await _context.OrderBatch
                .Where(ob => ob.BatchNumber.StartsWith(batchNumber))
                .OrderByDescending(ob => ob.OrderBatchId)
                .FirstOrDefaultAsync();

            if (orderBatch != null)
            {
                return orderBatch.BatchNumber;
            }
            else
            {
                return null;
            }
        }
        public async Task<OrderBatchItem> GetOrderBatchItemByIdAsync(int orderBatchItemId)
        {
            var orderBatchItem = await _context.OrderBatchItem.FindAsync(orderBatchItemId);
            return orderBatchItem;
        }
        public async Task<BatchItemStatus> GetBatchItemStatusByIdAsync(int batchItemStatusId)
        {
            var batchItemStatus = await _context.BatchItemStatus.FindAsync(batchItemStatusId);
            return batchItemStatus;
        }
        public async Task<BatchItemStatus> GetNextBatchItemStatusAsync(int departmentId, int currentExecutionSequence)
        {
            var nextBatchItemStatus = await _context.BatchItemStatus
                .Where(b => b.DepartmentId == departmentId && b.ExecutionSequence == currentExecutionSequence + 1)
                .FirstOrDefaultAsync();

            return nextBatchItemStatus;
        }
        public async Task UpdateOrderBatchItemAsync(OrderBatchItem orderBatchItem)
        {
            _context.Entry(orderBatchItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<BatchItemStatus> GetProductStatusByOrderBatchItemIdAsync(int orderBatchItemId)
        {
            var orderBatchItem = await _context.OrderBatchItem
                .Include(obi => obi.BatchItemStatus)
                .Where(obi => obi.OrderBatchItemId == orderBatchItemId)
                .SingleOrDefaultAsync();

            return orderBatchItem.BatchItemStatus;
        }
        public async Task<OrderBatchItem> GetBatchItemByOrderBatchItemIdAsync(int orderBatchItemId)
        {
            return await _context.OrderBatchItem.SingleOrDefaultAsync(item => item.OrderBatchItemId == orderBatchItemId);
        }

        public async Task<bool> AllItemsCompletedForOrderBatchAsync(int orderBatchId)
        {
            return await _context.OrderBatchItem
                .Where(item => item.OrderBatchId == orderBatchId)
                .AllAsync(item => item.IsCompleted);
        }
        public async Task<BatchItemStatus> GetLastBatchItemStatusAsync(int departmentId)
        {
            return await _context.BatchItemStatus
                                 .Where(s => s.DepartmentId == departmentId)
                                 .OrderByDescending(s => s.ExecutionSequence)
                                 .FirstOrDefaultAsync();
        }
        public async Task<List<OrderBatchItem>> GetOrderBatchItemsByStatusIdAsync(int statusId)
        {
            return await _context.OrderBatchItem
                .Where(ob => ob.BatchItemStatusId == statusId)
                .ToListAsync();
        }
        public async Task<Department> GetDepartmentForBatchItemByIdAsync(int departmentId)
        {
            return await _context.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }
        public async Task<List<OrderBatch>> GetFilteredOrderBatchesAsync()
        {
            var orderBatches = await _context.OrderBatch
                .Where(ob => ob.Status != OrderBatchStatus.Completed)
                .ToListAsync();

            var filteredBatches = orderBatches
                .Where(batch => _context.OrderBatchItem
                    .Any(item => item.OrderBatchId == batch.OrderBatchId && !item.IsPicked && _context.Stock.Any(stock => stock.ProductId == item.ProductId))) // Ensuring here that at least one item is not picked. Also, if everything is picked except items with invalid stock, then the batch won't show up.
                .ToList();

            return filteredBatches;
        }

        public async Task<List<OrderBatch>> GetOrderBatchesWithoutPickedItems()
        {
            var filteredBatches = await _context.OrderBatch
                .Where(ob => ob.Status != OrderBatchStatus.Completed &&
                             _context.OrderBatchItem
                                .Where(item => item.OrderBatchId == ob.OrderBatchId)
                                .All(item => !item.IsPicked))
                .ToListAsync();

            return filteredBatches;
        }

        public async Task<Dictionary<int, string>> GetOrderBatchNumbersByOrderIds(List<int> orderIds)
        {
            return await _context.OrderBatchItem.Where(x => orderIds.Contains(x.Order.ERPOrderId))
                 .Include(x => x.Order)
                 .Include(x => x.OrderBatch)
                 .Distinct()
                 .GroupBy(x => x.Order.ERPOrderId) 
                 .Select(g => g.First())
                 .ToDictionaryAsync(x => x.Order.ERPOrderId, x => x.OrderBatch.BatchNumber);
        }

        public async Task<Dictionary<int, List<string>>> GetOrderBatchNumberByOrderId(int orderId)
        {
            var batchNumbers = await _context.OrderBatchItem
                .Where(x => x.Order.ERPOrderId == orderId)
                .Include(x => x.OrderBatch)
                .Select(x => x.OrderBatch.BatchNumber)
                .Distinct()
                .ToListAsync();

            return new Dictionary<int, List<string>> { { orderId, batchNumbers } };
        }


        public async Task<bool>UpdateOrderBatchPurchaseOrderDetails(int purchaseOrderId,List<int> batchIds)
        { bool isSuccesfull = false;
            try
            {
                await _context.OrderBatch
                         .Where(p => batchIds.Contains(p.OrderBatchId))
                         .ExecuteUpdateAsync(s => s
                             .SetProperty(p => p.PurchaseOrderId, p => purchaseOrderId));

                return true;
            }
            catch (Exception) {
                throw;
            }
        }

        public async Task<bool> UndoBatchPOIdAssignment(int purchaseOrderId, List<int> batchIds)
        {
            bool isSuccesfull = false;
            try
            {
                await _context.OrderBatch
                         .Where(p => batchIds.Contains(p.OrderBatchId))
                         .ExecuteUpdateAsync(s => s
                             .SetProperty(p => p.PurchaseOrderId, p => null));

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Additional Classes
        public class OrderItemInfo
        {
            public string Sku { get; set; }
            public int ERPOrderId { get; set; }
            public int RequiredAmount { get; set; }
            public string OrderNumber { get; set; }
            public bool IsAltItem { get; set; }
            public int Multiplier { get; set; } = 1;
        }
        #endregion
    }
}
