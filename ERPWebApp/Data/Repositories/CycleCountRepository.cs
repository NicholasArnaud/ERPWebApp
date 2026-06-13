using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class CycleCountRepository : Repository<CycleCount>, ICycleCountRepository
    {
        public CycleCountRepository(ApplicationDbContext context) : base(context)
        {
        }


        public List<Report> GetCycleCountReport(DateTime startDate, DateTime endDate, int locationId)
        {
            var query = from cc in _context.CycleCount
                        join s in _context.Stock on cc.StockId equals s.StockId
                        join l in _context.Location on s.LocationId equals l.LocationId
                        where s.LastCounted >= startDate 
                            && s.LastCounted <= endDate 
                            && (locationId<= 0 || s.LocationId == locationId)
                            && l.Type != LocationType.ReceiveOnly
                        group cc by new { cc.EnteredSku, cc.VerifiedBy } into data
                        select new Report
                        {
                            Sku = data.Key.EnteredSku,
                            User = data.Key.VerifiedBy,
                            Date = data.Max(x => x.VerifiedOn) != null ? data.Max(x=>x.VerifiedOn).Value.ToString("MM/dd/yyyy") : null
                        };

            return query.ToList();
        }

        public async Task<List<CycleCount>> PrepareCycleCountAsync(List<int> stockIds)
        {
            var query = from A in _context.Stock
                        join B in _context.Product on A.ProductId equals B.ProductId
                        where stockIds.Contains(A.StockId)
                        select new CycleCount
                        {
                            StockId = A.StockId,
                            EnteredSku = B.Sku,
                            ExpectedQuantity = A.TotalAvailable
                        };
            return await query.ToListAsync();
        }

        public async Task<CycleCount> PrepareCycleCountAsync(int stockId)
        {
            var query = from A in _context.Stock
                        join B in _context.Product on A.ProductId equals B.ProductId
                        where A.StockId == stockId
                        select new CycleCount
                        {
                            StockId = A.StockId,
                            EnteredSku = B.Sku,
                            ExpectedQuantity = A.TotalAvailable
                        };
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<CycleCount>> PrepareCycleCountForSiteAsync(int siteId, CycleCountFrequency frequency)
        {
            var today = DateTime.Now;
            var query = from A in _context.Stock
                        join B in _context.Product on A.ProductId equals B.ProductId
                        where A.Location.SiteId == siteId
                            && A.BeingCounted == false
                            && (
                                EF.Functions.DateDiffDay(A.LastCounted, today) > frequency.BaseDays && A.TotalAvailable > 0
                                || EF.Functions.DateDiffDay(A.LastCounted, today) > frequency.Over1000 && A.TotalAvailable > 1000
                                || A.Products.Cost > 10 && EF.Functions.DateDiffDay(A.LastCounted, today) > frequency.Cost10 && A.TotalAvailable > 0
                            )
                        select new CycleCount
                        {
                            StockId = A.StockId,
                            EnteredSku = B.Sku,
                            ExpectedQuantity = A.TotalAvailable
                        };
            return await query.ToListAsync();
        }
    }
}