using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class InventoryRequestFormFixtures
    {
        public static List<InventoryRequestForm> GetTestFiles() =>
         [
            new InventoryRequestForm
            {
                InventoryRequestFormId = 1,
                ProductId = 1,
                Products = ProductFixtures.GetTestProducts().First(),
                QuantityNeeded = 50,
                RequestedByUser = "John",
                RequestedByEmployeeId = 1,
                RequestedEmployee = EmployeeFixtures.GetTestEmployeeices().First(),
                CreatedDate = DateTime.Now,
                PickReason = "Restocking",
                IsPicked = false,
                PickedByUser = "Mary",
                PickedByEmployeeId = 1,
                PickedEmployee = EmployeeFixtures.GetTestEmployeeices().First(),
                PickedDate = DateTime.Now.AddDays(1),
                IsFromExtrasLocation = false,
                StockId = 234,
                Stocks = StockFixtures.GetTestStocks().First(),
                FromLocation = "Warehouse",
                IsReceived = false,
                ReceivedDate = DateTime.Now.AddDays(2),
                OrderNumber = "12345"
            }
         ];
    }
}