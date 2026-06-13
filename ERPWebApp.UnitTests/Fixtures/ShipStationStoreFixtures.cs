namespace ERPWebApp.UnitTests.Fixtures
{
    public class ShipStationStoreFixtures
    {

        public static List<ShipStationStore> GetTestShipStationStores()
        {
            return [
                new() {
                    ShipStationStoreId = 1,
                    StoreId = 1,
                    StoreName = "Test 1",
                    IsActive = true,
                },
                new() {
                    ShipStationStoreId = 2,
                    StoreId = 2,
                    StoreName = "Test 2",
                    IsActive = true,
                },
                new() {
                    ShipStationStoreId = 3,
                    StoreId = 3,
                    StoreName = "Test 3",
                    IsActive = true,
                }
            ];
        }

    }
}
