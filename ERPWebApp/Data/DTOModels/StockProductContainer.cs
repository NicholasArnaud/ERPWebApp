using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.DTOModels
{
    public class StockProductContainer
    {
        public Stock Stock { get; set; }
        public ProductContainer ProductContainer { get; set; }
    }
}