using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ERPWebApp.Models.Mappings
{
    public class UserEmailAlertMapping
    {
        [Key]
        public int UserEmailAlertMappingId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser IdentityUser { get; set; }

        public int EmailAlertId { get; set; }

        [ForeignKey("EmailAlertId")]
        public virtual EmailAlert EmailAlert { get; set; }

        [Required]
        [StringLength(256)]
        public string UserEmail { get; set; }
    }
}