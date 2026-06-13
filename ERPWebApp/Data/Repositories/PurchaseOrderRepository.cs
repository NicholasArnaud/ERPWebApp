using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<PurchaseOrder>> GetActivePurchaseOrdersByProductAsync(int productId)
        {
            var query = from A in _context.ProductVendorMapping
                        join B in _context.ProductPurchaseOrder
                            on new { A.ProductId, A.ProductVendorMappingId } equals new { ProductId = productId, B.ProductVendorMappingId }
                        join C in _context.PurchaseOrder
                            on B.PurchaseOrderId equals C.PurchaseOrderId
                        where A.IsActive &&
                            (C.POStatus == Status.Draft
                            || C.POStatus == Status.InProgress
                            || C.POStatus == Status.OpenIssued)
                        select C;

            return await query.ToListAsync();
        }

        public async Task<int> GetProductOnOrderQtyAsync(int productId)
        {
            int onOrder = await (from a in _context.ProductPurchaseOrder
                           join b in _context.ProductVendorMapping on a.ProductVendorMappingId equals b.ProductVendorMappingId
                           join aa in _context.PurchaseOrder on a.PurchaseOrderId equals aa.PurchaseOrderId
                           join c in _context.Product on b.ProductId equals c.ProductId
                           where c.ProductId == productId && aa.POStatus != Status.Cancelled && aa.POStatus != Status.Close && aa.POStatus != Status.FullyReceived
                           group a by c.ProductId into grouped
                           select grouped.Sum(x => x.TotalOrdered - x.TotalRecieved)
                        ).FirstOrDefaultAsync();

            return onOrder;
        }
    }
}