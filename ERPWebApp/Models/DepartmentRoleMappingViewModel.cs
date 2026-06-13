using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class DepartmentRoleMappingViewModel
    {
        public int DepartmentRoleId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<string> UserRoleIds { get; set; }
        public List<IdentityRole> Roles { get; set; }

        public DepartmentRoleMappingViewModel()
        {
            UserRoleIds = new List<string>();
        }
    }
}
