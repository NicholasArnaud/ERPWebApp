using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class UserRolesViewModel
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
