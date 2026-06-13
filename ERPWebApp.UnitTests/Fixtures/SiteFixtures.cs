using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class SiteFixtures
    {
        public static List<Site> GetTestSites() =>
        [
            new Site()
            {
                SiteId = 1,
                SiteName = "Site 1",
                SiteDescription = "Site 1 Description",
                IsActive = true,
                SiteVolume = 1000.00m,
                IsRestricted = false,
                IsExternal = false
            },
            new Site()
            {
                SiteId = 2,
                SiteName = "Site 2",
                SiteDescription = "Site 2 Description",
                IsActive = true,
                SiteVolume = 2000.00m,
                IsRestricted = true,
                IsExternal = false
            },
            new Site()
            {
                SiteId = 48,
                SiteName = "Site 48",
                SiteDescription = "Site 48 Description",
                IsActive = true,
                SiteVolume = 5000.00m,
                IsRestricted = true,
                IsExternal = true
            },
            new Site()
            {
                SiteId = 49,
                SiteName = "Site 49",
                SiteDescription = "Site 49 Description",
                IsActive = true,
                SiteVolume = 7000.00m,
                IsRestricted = true,
                IsExternal = true
            }
        ];
    }
}