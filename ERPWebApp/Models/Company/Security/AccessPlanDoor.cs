using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company.Security
{
    public class AccessPlanDoor
    {
        [Key]
        public int AccessPlanDoorId { get; set; }
        [ForeignKey("AccessPlanId")]
        public virtual AccessPlan AccessPlan { get; set; }
        [Display(Name = "Access Plan")]
        public int AccessPlanId { get; set; }
        [ForeignKey("AccessPointId")]
        public virtual AccessPoint AccessPoint { get; set; }
        [Display(Name = "Access Point")]
        public int AccessPointId { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
    }
}
