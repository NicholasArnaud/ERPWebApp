using static ERPWebApp.Models.BatchViewModel;
using ERPWebApp.Data.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class BatchViewRepository : Repository<BatchView>, IBatchViewRepository
    {
        public BatchViewRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<BatchView>> GetAllBatches(string sku = null, int? departmentId = null)
        {
            var query = await (from ob in _context.OrderBatch
                               join obi in _context.OrderBatchItem on ob.OrderBatchId equals obi.OrderBatchId
                               join p in _context.Product on obi.ProductId equals p.ProductId
                               let cwaOrderIds = (from obItem in _context.OrderBatchItem
                                                  where ob.OrderBatchId == obItem.OrderBatchId
                                                  select obItem.ERPOrderId).Distinct().ToList()
                               let hasUvpSku = (from oi in _context.OrderItem
                                                where cwaOrderIds.Contains(oi.ERPOrderId) && oi.sku.EndsWith("UVP")
                                                select oi).Any()
                               where (string.IsNullOrEmpty(sku) || p.Sku == sku) &&
                                     (!departmentId.HasValue || p.Departments.Any(d => d.DepartmentId == departmentId.Value)) &&
                                     (departmentId != 3 || !hasUvpSku) &&
                                     obi.Order.orderStatus != Order.OrderStatus.cancelled
                               group obi by new { ob.BatchNumber, ob.Status, ob.CreateDate, hasUvpSku } into bg
                               orderby bg.Key.CreateDate descending
                               select new
                               {
                                   bg.Key.BatchNumber,
                                   bg.Key.Status,
                                   bg.Key.CreateDate,
                                   TotalQuantity = bg.Sum(x => x.Quantity),
                                   Departments = bg.Select(x => x.Product.Departments).FirstOrDefault(),
                                   bg.Key.hasUvpSku
                               }).ToListAsync();

            var result = query.Select(batch => new BatchView
            {
                batchNumber = batch.BatchNumber,
                totalQuantity = batch.TotalQuantity,
                status = batch.Status.ToString(),
                createDate = batch.CreateDate.ToString("MM-dd-yyyy; hh:mm tt"),
                departments = batch.hasUvpSku ? new List<Department> { new Department { DepartmentName = "UVP" } } : batch.Departments
            }).ToList();

            return result;
        }

        public async Task<List<ProductDetail>> GetProductDetailsForBatch(int orderBatchId)
        {
            var productDetails = await (from ob in _context.OrderBatch
                                        join obi in _context.OrderBatchItem on ob.OrderBatchId equals obi.OrderBatchId
                                        join p in _context.Product on obi.ProductId equals p.ProductId
                                        where ob.OrderBatchId == orderBatchId && obi.Order.orderStatus != Order.OrderStatus.cancelled
                                        select new ProductDetail
                                        {
                                            productSku = p.Sku,
                                            quantity = obi.Quantity
                                        }).ToListAsync();

            return productDetails;
        }

        public async Task<List<ProductDetailWithOrderBatchItem>> GetProductDetailsWithOrderBatchtemForBatch(int orderBatchId)
        {

            var productDetails = await (from ob in _context.OrderBatch
                                         join obi in _context.OrderBatchItem on ob.OrderBatchId equals obi.OrderBatchId
                                         join p in _context.Product on obi.ProductId equals p.ProductId

                                        where ob.OrderBatchId == orderBatchId && obi.Order.orderStatus != Order.OrderStatus.cancelled
                                        select new ProductDetailWithOrderBatchItem
                                         {   productId=p.ProductId,
                                             productSku = p.Sku,
                                             quantity = obi.Quantity,
                                             orderBatchItem = obi.OrderBatchItemId,
                                             cost = p.Cost,
                                             customCost = p.Cost
                                         }).ToListAsync();

            return productDetails;
        }

        public async Task<List<Product>> GetAllActiveProducts()
        {
            var productList = await _context.Product.Where(p => p.IsActive).ToListAsync();
            return productList;
        }
        public async Task<List<OrderDetail>> GetOrderDetailsForBatch(int orderBatchId)
        {
            var orderDetails = await (from obi in _context.OrderBatchItem
                                      where obi.OrderBatchId == orderBatchId && obi.Order.orderStatus != Order.OrderStatus.cancelled
                                      group obi by new { obi.OrderNumber, obi.ERPOrderId } into og
                                      select new OrderDetail
                                      {
                                          orderNumber = og.Key.OrderNumber,
                                          ERPOrderId = og.Key.ERPOrderId ?? 0
                                      }).ToListAsync();

            return orderDetails;
        }

        public async Task<List<Department>> GetAllActiveDepartments()
        {
            return await _context.Department.Where(d => d.IsActive).ToListAsync();
        }

    }
}
