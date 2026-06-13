using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfInventoryFixtures
    {
        public static List<NirfInventory> GetTestList() => [
            new NirfInventory
            {
                NirfInventoryId = 1,
                MembraneLocationId = 2,
                MembraneLocation = new Location { LocationId = 2, LocationName = "Test Membrane Location" },
                MainLocationId = 3,
                MainLocation = new Location { LocationId = 3, LocationName = "Test Main Location" },
                AltMainLocationId = 4,
                AltMainLocation = new Location { LocationId = 4, LocationName = "Test Alt Main Location" },
                AltMembraneLocationId = 5,
                AltMembraneLocation = new Location { LocationId = 5, LocationName = "Test Alt Membrane Location" },
                NirfFormId = 6,
                NirfForm = NirfFormFixtures.GetTestList().First(),
                Comments = "Test Comments",
                SignedOn = DateTime.Now,
                SignedBy = "Test User",
                AspUserId = "Test ASP User"
            }
         ];
    }
}