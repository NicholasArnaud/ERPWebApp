using ERPWebApp.Models.Company;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERPWebApp.Models.Inventory
{
    public class InventoryRequestForm
    {
        [Key]
        public int InventoryRequestFormId { get; set; }

        //ON PRODUCTION CREATE ONLY
        [Display(Name = "Product")]
        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Products { get; set; }
        [Display(Name = "Quantity Needed")]
        [Required]
        [Range(1, 100, ErrorMessage = "Please enter valid quantity")]
        public int QuantityNeeded { get; set; }
        public string RequestedByUser { get; set; }
        [Display(Name = "Requested By")]
        [Required]
        public int RequestedByEmployeeId { get; set; }
        [ForeignKey("RequestedByEmployeeId")]
        public virtual Employee RequestedEmployee { get; set; }
        [Display(Name = "Created Date")]
        [Required]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Pick Reason")]
        [Required]
        public string PickReason { get; set; }

        [Display(Name = "Reason")]
        [MaxLength(250)]
        public string ReasonExplanation { get; set; }
        
        [Required]
        [Display(Name = "To Location")]
        public int ToLocationId { get; set; }
        [ForeignKey("ToLocationId")]
        public virtual Location ToLocation { get; set; }

        //ON MEMBRANE EDIT ONLY
        [Display(Name = "Picked?")]
        [DefaultValue(false)]
        public bool IsPicked { get; set; }
        public string PickedByUser { get; set; }
        [Display(Name = "Picked By")]
        public int? PickedByEmployeeId { get; set; }
        [ForeignKey("PickedByEmployeeId")]
        public virtual Employee PickedEmployee { get; set; }
        [Display(Name = "Picked Date")]
        public DateTime PickedDate { get; set; }
        [Display(Name = "From Extras?")]
        public bool IsFromExtrasLocation { get; set; }
        [Display(Name = "From Stock")]
        public int? StockId { get; set; }
        [ForeignKey("StockId")]
        public virtual Stock Stocks { get; set; }
        [Display(Name = "From Location")]
        public string FromLocation { get; set; }
        [NotMapped]
        public string Initials { get; set; }

        //ON FINALIZATION
        [Display(Name = "Received?")]
        [DefaultValue(false)]
        public bool IsReceived { get; set; }
        [Display(Name = "Received Date")]
        public DateTime ReceivedDate { get; set; }
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }
        
    }
}
