using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(SiteName), IsUnique = true)]
    public class Site
    {
        [Key]
        public int SiteId { get; set; }

        [Display(Name = "Site")]
        public string SiteName { get; set; }

        [Display(Name = "Site Description")]
        public string SiteDescription { get; set; }

        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Display(Name = "Total Site Volume")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SiteVolume { get; set; }

        [Display(Name = "Restricted")]
        [DefaultValue(false)]
        public bool IsRestricted { get; set; }

        [Display(Name = "External")]
        [DefaultValue(false)]
        public bool IsExternal { get; set; }
    }
}
