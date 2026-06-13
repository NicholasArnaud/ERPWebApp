using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
    public class DepartmentRoleMapping
    {
        [Key]
        [Display(Name = "Department Role Mapping")]
        public int DepartmentRoleId { get; set; }
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        [Display(Name = "User Role")]
        public string UserRoleId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        [ForeignKey("UserRoleId")]
        public virtual IdentityRole Role { get; set; }

    }
}
