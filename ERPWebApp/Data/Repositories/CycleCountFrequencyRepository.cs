using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class CycleCountFrequencyRepository : Repository<CycleCountFrequency>, ICycleCountFrequencyRepository
    {
        public CycleCountFrequencyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task GenerateFrequenciesAsync()
        {
            var query = from A in _context.Site
                        where A.IsActive == true
                            && !(_context.CycleCountFrequency.Any(B => B.SiteId == A.SiteId))
                        select new CycleCountFrequency
                        {
                            SiteId = A.SiteId,
                            BaseDays = 30,
                            Over1000 = 15,
                            Cost10 = 4,
                            ModifyByUser = "System",
                            ModifyDate = DateTime.Now
                        };
            var results = await query.ToListAsync();
            await _context.AddRangeAsync(results);
        }

        public async Task<CycleCountFrequency> GetLatestFrequencyAsync(int siteId)
        {
            var query = from A in _context.CycleCountFrequency
                        where A.SiteId == siteId
                        orderby A.ModifyDate descending
                        select A;
            return await query.FirstOrDefaultAsync();                   
        }
    }
}