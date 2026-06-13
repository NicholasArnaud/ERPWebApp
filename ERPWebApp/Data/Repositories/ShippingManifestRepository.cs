using ERPWebApp.Data.Extensions;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Shipping;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories;

public class ShippingManifestRepository(ApplicationDbContext context)
    : Repository<ShippingManifest>(context), IShippingManifestRepository
{
    public async Task<(IReadOnlyCollection<ShippingManifest>, int)> GetShippingManifestsAsync(
        string carrierId,
        string warehouseId,
        DateTime? shipDate,
        SearchParameters search
    )
    {
        var query = _context.ShippingManifests
            .WhereIf(!string.IsNullOrWhiteSpace(carrierId), x => x.CarrierId == carrierId)
            .WhereIf(!string.IsNullOrWhiteSpace(warehouseId), x => x.WarehouseId == warehouseId)
            .WhereIf(shipDate != null, x => x.ShipDate == shipDate)
            .OrderByDescending(x => x.ShipDate);
        
        var count = await query.CountAsync();
        search.PageSize = search.PageSize is null or < 0 ? count : search.PageSize;
        
        var results = await query.SmartPaging(search.Start, search.PageSize)
            .AsNoTracking()
            .ToListAsync();
        
        return (results, count);
    }
}