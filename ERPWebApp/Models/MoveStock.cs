namespace ERPWebApp.Models
{
    public class MoveStock
    {
        public Inventory.Stock FromStock { get; set; }
        public Inventory.Stock ToStock { get; set; }
        public int quantity { get; set; }
        public DateTime DateTime { get; set; }
        public string ModifiedBy { get; set; }
        public List<MoveStockHistory> StockHistory { get; set; }
    }
}
