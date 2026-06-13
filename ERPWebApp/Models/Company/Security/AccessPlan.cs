using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company.Security
{
    public class AccessPlan
    {
        [Key]
        public int AccessPlanId { get; set; }
        [Required]
        [Display(Name = "Access Plan Name")]
        public string AccessPlanName { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        public DateTime ModifyDate { get; set; }
        public string ModifyByUser { get; set; }
        //Add Allowed clock in and out time values here
        [Display(Name = "24-hour Access")]
        [DefaultValue("true")]
        public bool Has24HourAccess { get; set; }
        [Column(TypeName = "time")]
        public TimeSpan EarliestCheckInTime { get; set; }
        [Column(TypeName = "time")]
        public TimeSpan LatestCheckInTime { get; set; }

    }
}
