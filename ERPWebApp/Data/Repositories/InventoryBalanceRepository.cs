using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class InventoryBalanceRepository : Repository<InventoryBalance>, IInventoryBalanceRepository
    {
        public InventoryBalanceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public List<Report> GetReport(int ProductId)
        {
            var query = from a in _context.InventoryBalance
                        join p in _context.Product on a.Sku equals p.Sku
                        where ProductId <= 0 || p.ProductId == ProductId
                        select new Report
                        {
                            Sku = a.Sku,
                            Description = a.Description,
                            TotalAvailable = a.TotalAvailable,
                            ShipStationOrders = a.PendingShipStationOrders,
                            OrderDifference = a.OrderDifference
                        };

            return query.ToList();


        }
    }
}
