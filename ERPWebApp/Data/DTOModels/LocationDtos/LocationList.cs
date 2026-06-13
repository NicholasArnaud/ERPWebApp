namespace ERPWebApp.Data.DTOModels.LocationDtos;

public record LocationList(
    int LocationId,
    string LocationName,
    string SiteName,
    string LocationDescription,
    string LocationType,
    bool IsActive,
    bool IsExternal,
    string Permission
);