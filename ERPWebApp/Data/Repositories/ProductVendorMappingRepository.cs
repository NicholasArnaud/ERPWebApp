using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class ProductVendorMappingRepository(ApplicationDbContext context)
    : Repository<ProductVendorMapping>(context), IProductVendorMappingRepository
    {
        public async Task<IEnumerable<Vendor>> GetVendorsAsync()
        {
            var venders = await _context.ProductVendorMapping
                .Where(v => v.IsActive && v.Vendor.IsActive)
                .Include(v => v.Vendor)
                .GroupBy(v => v.Vendor)
                .Select(g => g.Key)
                .ToListAsync();

            return venders;
        }
    }
}