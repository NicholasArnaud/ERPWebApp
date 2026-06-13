using ERPWebApp.Models;

namespace ERPWebApp.Data.DTOModels
{
    public class MoveStockDOT
    {
        public ActionType Type { get; set; }
        public string Sku { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int Quantity { get; set; }
        public DateTime DateTime { get; set; }
        public string EmployeeName { get; set; }
    }
}
