using ERPWebApp.Models.Common;
using ERPWebApp.Models.Shipping;

namespace ERPWebApp.Data.Repositories.Interface;

public interface IShippingManifestRepository : IRepository<ShippingManifest>
{
    public Task<(IReadOnlyCollection<ShippingManifest>, int)> GetShippingManifestsAsync(
        string carrierId,
        string warehouseId,
        DateTime? shipDate,
        SearchParameters search
    );
}