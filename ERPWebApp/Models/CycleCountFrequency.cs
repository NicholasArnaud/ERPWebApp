using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERPWebApp.Models
{
    public class CycleCountFrequency
    {
        [Key]
        public int CycleCountFrequencyId { get; set; }

        [Display(Name = "Base Days")]
        public int BaseDays { get; set; }
        [Display(Name = "Over 1000 Quantity")]
        public int Over1000 { get; set; }
        [Display(Name = "Costs over $10")]
        public int Cost10 { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifyByUser { get; set; }
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Inventory.Site Sites { get; set; }
    }
}
