using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class MoveStockHistory
    {
        [Key]
        public int MoveStockHistoryId { get; set; }

        [Display(Name = "Product Sku")]
        public String Sku { get; set; }

        [Display(Name = "To Stock")]
        public int? ToStockId { get; set; }
        [ForeignKey("ToStockId")]
        public virtual Inventory.Stock ToStock { get; set; }

        [Display(Name = "From Stock")]
        public int? FromStockId { get; set; }
        [ForeignKey("FromStockId")]
        public virtual Inventory.Stock FromStock { get; set; }

        [Display(Name = "Employee Name")]
        public String EmployeeName { get; set; }

        [Display(Name = "Product Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Action Date")]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        [Display(Name = "Action Type")]
        public ActionType Type { get; set; }

    }
    public enum ActionType
    {
        [Display(Name ="Add")]
        Add,
        [Display(Name ="Remove")]
        Remove,
        [Display(Name ="Transfer")]
        Transfer,
        [Display(Name ="Received")]
        Received,
        [Display(Name ="CycleCount")]
        CycleCount
    }
}
