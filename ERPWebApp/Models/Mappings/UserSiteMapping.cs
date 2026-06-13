namespace ERPWebApp.Models.Mappings;

using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Index(nameof(UserId), nameof(SiteId), IsUnique = true)]
public class UserSiteMapping
{
    [Key]
    [Display(Name = "User Site Mapping")]
    public int UserSiteMappingId { get; set; }
    [Required]
    [StringLength(450)]
    [Display(Name = "User")]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    [Display(Name = "Identity User")]
    public virtual IdentityUser IdentityUser { get; set; }
    [Display(Name = "Site")]
    public int SiteId { get; set; }
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; }
}
