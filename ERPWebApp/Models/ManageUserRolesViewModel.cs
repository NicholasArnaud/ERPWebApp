using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Models
{
    [Index(nameof(RoleName), IsUnique = true)]
    public class ManageUserRolesViewModel
    {
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
