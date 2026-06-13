using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company.Security
{
    public class AccessPlanUser
    {
        [Key]
        public int AccessPlanUserId { get; set; }
        [ForeignKey("AccessCardId")]
        public virtual AccessCard AccessCard { get; set; }
        [Display(Name = "Access Card")]
        public int AccessCardId { get; set; }
        [ForeignKey("AccessPlanId")]
        public virtual AccessPlan AccessPlan { get; set; }
        [Display(Name = "Access Plan")]
        public int AccessPlanId { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
    }
}
