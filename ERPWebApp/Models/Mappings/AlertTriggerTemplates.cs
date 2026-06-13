using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Mappings
{
    public class AlertTriggerTemplateMappings
    {
        [Key]
        public int AlertTemplateId { get; set; }

        [Required]
        [StringLength(256)]
        public string TriggerName { get; set; }

        [Required]
        [StringLength(400)]
        public string MessageContents { get; set; }
    }
}
