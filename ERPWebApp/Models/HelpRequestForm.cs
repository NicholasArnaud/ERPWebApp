using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class HelpRequestForm
    {
        [Key]
        public int HelpRequestFormId { get; set; }
        //On Create
        [Required]
        [MaxLength(50, ErrorMessage = "Too many characters"), MinLength(5, ErrorMessage = "Please enter a subject")]
        public string Subject { get; set; }

        [Display(Name = "Requesting User/Station")]
        public string RequestedByUser { get; set; }

        [ForeignKey("RequestedByEmployeeId")]
        [Display(Name = "Requested By")]
        [Required]
        public int RequestingEmployeeId { get; set; }

        [Display(Name = "Requested By")]
        public virtual Employee RequestingEmployee { get; set; }

        [Display(Name = "Problem Description")]
        [Required]
        [MaxLength(250, ErrorMessage = "Too many characters"), MinLength(5, ErrorMessage = "Please enter a detailed description of the problem")]
        public string Description { get; set; }

        public string Urgency { get; set; }

        [Required]
        [Display(Name = "Helper Employee")]
        public int HelperEmployeeId { get; set; }

        [Display(Name = "Helper Employee")]
        public virtual Employee HelperEmployee { get; set; }

        [Display(Name = "Created Date")]
        [Required]
        public DateTime CreatedDate { get; set; }

        //On Edit(Managers+ only)
        public int? Priority { get; set; }
        [Display(Name = "Denied?")]
        public bool IsDenied { get; set; }
        [Display(Name = "Complete?")]
        public bool IsComplete { get; set; }

        [Display(Name = "Completed Date")]
        [Required]
        public DateTime CompletedDate { get; set; }




    }
}
