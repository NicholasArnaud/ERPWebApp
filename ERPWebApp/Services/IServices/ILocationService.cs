using ERPWebApp.Data.DTOModels.LocationDtos;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices;

public interface ILocationService : IService<Location>
{
    public Task<(List<LocationList>, int)> GetLocationsAsync(
        bool includeInactive,
        bool? isExternal,
        int? siteId,
        string permission,
        SearchParameters searchParameters
    );
}