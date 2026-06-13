using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Extensions;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class MoveStockHistoryFixtures
    {
        public static List<MoveStockHistory> GetTestMoveStockHistory() =>
         [
            new MoveStockHistory()
            {
                MoveStockHistoryId=1,
                Sku= "SKU1234",
                ToStockId= 2,
                FromStockId=1,
                EmployeeName="John Doe",
                Quantity=10,
                DateTime=DateTime.Parse("2022-02-15"),
                Type= ActionType.Transfer
            },
            new MoveStockHistory()
            {
                MoveStockHistoryId= 2,
                Sku= "SKU5678",
                ToStockId= 3,
                FromStockId= 2,
                EmployeeName= "Jane Smith",
                Quantity= 5,
                DateTime= DateTime.Parse("2022-02-17"),
                Type= ActionType.Transfer
            },
             new MoveStockHistory()
            {
               MoveStockHistoryId= 3,
               Sku= "SKU1234",
               ToStockId= 1,
               FromStockId= 2,
               EmployeeName= "John Doe",
               Quantity= 5,
               DateTime= DateTime.Parse("2022-02-20"),
               Type= ActionType.Transfer
            }
         ];

        public static List<StockMovementHistory> GetStockMovementHistory() => GetTestMoveStockHistory()
            .Select(static x => new StockMovementHistory(
                x.EmployeeName,
                x.DateTime,
                x.Sku,
                x.Type.GetDisplayName(),
                "From",
                0,
                "To",
                0,
                x.Quantity,
                "Action"
            )).ToList();
    }
}