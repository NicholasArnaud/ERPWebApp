using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERPWebApp.Models
{
    public class CycleCount
    {
        [Key]
        public int CycleCountId { get; set; }
        [Display(Name = "Stock")]
        [Required]
        public int StockId { get; set; }

        [ForeignKey("StockId")]
        public virtual Inventory.Stock Stock { get; set; }

        [Display(Name = "Entered Sku")]
        public string EnteredSku { get; set; }

        [Display(Name = "Entered Quantity")]
        public int? EnteredQuantity { get; set; }

        [Display(Name = "Expected Quantity")]
        public int ExpectedQuantity { get; set; }

        [Display(Name = "Entered By")]
        public int? EnteredById { get; set; }

        [ForeignKey("EnteredById")]
        public virtual Employee EnteredByEmployee { get; set; }

        [Display(Name = "Entered On")]
        public DateTime? EnteredOn { get; set; }

        [Display(Name = "Verified By")]
        public String VerifiedBy { get; set; }

        [Display(Name = "Verified On")]
        public DateTime? VerifiedOn { get; set; }
        [Display(Name = "Finished")]
        public Boolean Finished { get; set; }
    }
}
