using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class MessageEmployee
    {
        [Key]
        [Display(Name = "Message Employee")]
        public int MessageEmployeeId { get; set; }
        [MinLength(4, ErrorMessage = "The {0} value cannot be less than {1} characters. ")]
        [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string Message { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
        [Display(Name = "Sent To Employee")]
        public int EmployeeId { get; set; }
        [Display(Name = "Sent From Employee")]
        public string SentFromEmployee { get; set; }
        [Display(Name = "Sent Time")]
        public DateTime SentTime { get; set; }

    }
}
