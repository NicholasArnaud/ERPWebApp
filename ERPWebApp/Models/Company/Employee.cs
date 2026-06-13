using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company
{
    [Index(nameof(FullName), IsUnique = true)]
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        [StringLength(50)]
        public string MiddleName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [ForeignKey("UserRolesViewModelId")]
        public virtual UserRolesViewModel UserRolesViewModel { get; set; }
        [Display(Name = "Select Username and email to set ASPUser ID")]
        public string UserRolesViewModelId { get; set; }
        public string Position { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Personal Email")]
        [EmailAddress]
        public string PersonalEmail { get; set; }
        [Display(Name = "Company Email")]
        [EmailAddress]
        public string CompanyEmail { get; set; }
        [Display(Name = "Job Status")]
        public JobStatus JobStatus { get; set; }
        [Display(Name = "Income Per Hour")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomePerHour { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifyBy { get; set; }
        [Display(Name = "Stamp Number")]
        public string EmployeeReferenceNumber { get; set; }
        public string ApsuId { get; set; }

    }


    public enum JobStatus
    {
        [Display(Name = "Part Time")]
        PartTime,
        [Display(Name = "Full Time")]
        FullTime,
        Salary,
        Seasonal,
        Terminated,
        Intern
    }


}
