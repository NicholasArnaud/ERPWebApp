using static ERPWebApp.Controllers.PurchaseOrders.PurchaseOrdersController;

namespace ERPWebApp.Models.PurchaseOrders.StockLocationEdit
{
    public class Result
    {
        public string Text { get; set; }
        public List<ResultChildren> Children { get; set; }

    }
}
