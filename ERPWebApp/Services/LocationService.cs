using ERPWebApp.Data.DTOModels.LocationDtos;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services;

public class LocationService(IUnitOfWork unitOfWork) : Service<Location>(unitOfWork), ILocationService
{
    private readonly IUnitOfWork _unitOfWork1 = unitOfWork;

    public Task<(List<LocationList>, int)> GetLocationsAsync(
        bool includeInactive,
        bool? isExternal,
        int? siteId,
        string permission,
        SearchParameters searchParameters
    ) => _unitOfWork1.Locations.GetLocationsAsync(includeInactive, isExternal, siteId, permission, searchParameters);
}