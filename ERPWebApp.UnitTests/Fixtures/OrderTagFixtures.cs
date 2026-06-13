using ERPWebApp.Models.Orders;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class OrderTagFixtures
    {

        public static List<Order.OrderTag> GetTestOrderTags()
        {
            return [
                new()
                {
                    OrderTagId = 1,
                    tagId = 1,
                    name = "tag 1",
                    color = "#fff"
                },
                new()
                {
                    OrderTagId = 2,
                    tagId = 2,
                    name = "tag 2",
                    color = "#fff"
                },
                new()
                {
                    OrderTagId = 3,
                    tagId = 3,
                    name = "tag 3",
                    color = "#fff"
                }
            ];
        }

    }
}
