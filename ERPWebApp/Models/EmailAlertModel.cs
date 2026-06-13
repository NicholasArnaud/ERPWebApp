using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Models
{
    public class EmailAlert
    {
        [Key]
        public int EmailAlertId { get; set; }

        [Required]
        [StringLength(256)]
        public string Subject { get; set; }

        [Required]
        [StringLength(400)]
        public string Body { get; set; }
        public AlertType AlertType { get; set; }
        public Frequency? Frequency { get; set; }

        public int? AlertTemplateId { get; set; }

        [ForeignKey("AlertTemplateId")]
        public virtual AlertTriggerTemplateMappings AlertTriggerTemplate { get; set; }

        public DateTime ScheduledTime { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; }
    }

    public enum AlertType
    {
        TimeBased,
        TriggerBased
    }

    public enum Frequency
    {
        Daily,
        Weekly,
        Monthly
    }
}
