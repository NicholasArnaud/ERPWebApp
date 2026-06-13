using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class LocationFixtures
    {
        public static List<Location> GetTestLocations() =>
        [
            new Location { LocationId = 1, SiteId = 1, LocationName = "Warehouse A", LocationDescription = "Main warehouse building", Type = LocationType.Normal, IsActive = true, IsExternal = false,Sites = SiteFixtures.GetTestSites().First() },
            new Location { LocationId = 2, SiteId = 1, LocationName = "Receiving Dock", LocationDescription = "Dock for receiving incoming shipments", Type = LocationType.ReceiveOnly, IsActive = true, IsExternal = false,Sites = SiteFixtures.GetTestSites().First() },
            new Location { LocationId = 3, SiteId = 2, LocationName = "Storage Trailer 1", LocationDescription = "Trailer for temporary storage of excess inventory", Type = LocationType.Normal, IsActive = false, IsExternal = false,Sites = SiteFixtures.GetTestSites().First() },
            new Location { LocationId = 4, SiteId = 2, LocationName = "Vendor Warehouse", LocationDescription = "Warehouse owned by vendor for storage of their own inventory", Type = LocationType.Normal, IsActive = true, IsExternal = true ,Sites = SiteFixtures.GetTestSites().First()}
        ];
    }
}