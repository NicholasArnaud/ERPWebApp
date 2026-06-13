using ERPWebApp.Data.DTOModels.LocationDtos;
using ERPWebApp.Data.Extensions;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Extensions;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories;

public class LocationRepository(ApplicationDbContext context) : Repository<Location>(context), ILocationRepository
{
    public async Task<(List<LocationList>, int)> GetLocationsAsync(
        bool includeInactive,
        bool? isExternal,
        int? siteId,
        string permission,
        SearchParameters searchParameters
    )
    {
        
        var searchValue = searchParameters.SearchValue;
        var sortColumn = searchParameters.SortBy;
        var query = _context.Location
            .WhereIf(isExternal.HasValue,x => x.IsExternal == isExternal)
            .WhereIf(!includeInactive, x => x.IsActive)
            .WhereIf(siteId is > 0, x => x.SiteId == siteId);

        
                //filters through a user search string (massive for amount of columns to search through normally done behind the scenes)
                if (!string.IsNullOrEmpty(searchValue))
                {
                    var filteredEnumTypes = System.Enum.GetValues<LocationType>()
                        .Where(value => value.ToString().ToLower().Contains(searchValue))
                        .ToList();
                    var filteredBoolIsActive = "";
                    if ("yes".Contains(searchValue) || "Yes".Contains(searchValue))
                    {
                        filteredBoolIsActive = "true";
                    }
                    else if ("no".Contains(searchValue) || "No".Contains(searchValue))
                    {
                        filteredBoolIsActive = "false";
                    }
                    else
                    {
                        filteredBoolIsActive = "~~~~~";
                    }

                    query = query.Where(
                        x =>
                            x.Sites.SiteName.Contains(searchValue)
                            || x.LocationName.ToLower().Contains(searchValue)
                            || x.LocationDescription.ToLower().Contains(searchValue)
                            || filteredEnumTypes.Contains(x.Type)
                            || x.IsActive.ToString().ToLower().Contains(filteredBoolIsActive)
                    );

                }
                
                            //column sort direction
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (!searchParameters.IsDescending)
                {
                    query = sortColumn switch
                    {
                        "SiteName" => query.OrderBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName),
                        "LocationName" => query.OrderBy(x => x.LocationName)
                            .ThenBy(x => x.Sites.SiteName),
                        "LocationDescription" => query.OrderBy(x => x.LocationDescription)
                            .ThenBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName),
                        "Type" => query.OrderBy(x => x.Type)
                            .ThenBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName),
                        "IsActive" => query.OrderBy(x => x.IsActive)
                            .ThenBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName),
                        "IsExternal" => query.OrderBy(x => x.IsExternal)
                            .ThenBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName),
                        _ => query.OrderBy(x => x.Sites.SiteName)
                            .ThenBy(x => x.LocationName)
                    };
                }
                else
                {
                    query = sortColumn switch
                    {
                        "SiteName" => query.OrderByDescending(x => x.Sites.SiteName)
                            .ThenByDescending(x => x.LocationName),
                        "LocationName" => query.OrderByDescending(x => x.LocationName)
                            .ThenByDescending(x => x.Sites.SiteName),
                        "LocationDescription" => query.OrderByDescending(x => x.LocationDescription)
                            .ThenByDescending(x => x.Sites.SiteName)
                            .ThenByDescending(x => x.LocationName),
                        "Type" => query.OrderByDescending(x => x.Type)
                            .ThenByDescending(x => x.Sites.SiteName)
                            .ThenByDescending(x => x.LocationName),
                        "IsActive" => query.OrderByDescending(x => x.IsActive)
                            .ThenByDescending(x => x.Sites.SiteName)
                            .ThenByDescending(x => x.LocationName),
                        _ => query.OrderByDescending(x => x.Sites.SiteName)
                            .ThenByDescending(x => x.LocationName)
                    };
                }
            }
            
            var count = await query.AsNoTracking().CountAsync();
            var length = searchParameters.PageSize is null or < 0 ? count : searchParameters.PageSize.Value;
            var skip = searchParameters.Start is null or < 0 ? 0 : searchParameters.Start.Value;

            var results = await query.Select(x => new LocationList(
                    x.LocationId,
                    x.LocationName,
                    x.Sites.SiteName,
                    x.LocationDescription,
                    x.Type.GetDisplayName(),
                    x.IsActive,
                    x.IsExternal,
                    permission
                )).Skip(skip)
                .Take(length)
                .AsNoTracking()
                .ToListAsync();

            return (results, count);
    }
}