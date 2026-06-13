using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(LocationName), IsUnique = true)]
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Display(Name = "Site")]
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Sites { get; set; }

        [Display(Name = "Location")]
        public string LocationName { get; set; }
        [Display(Name = "Location Description")]
        public string LocationDescription { get; set; } = "";
        [Display(Name = "Location Type")]
        public LocationType Type { get; set; }
        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [Display(Name = "External Location")]
        [DefaultValue(false)]
        public bool IsExternal { get; set; }
        [NotMapped]
        public string permission { get; set; }
    }
    public enum LocationType
    {
        [Display(Name = "Normal")]
        Normal,
        [Display(Name = "Pick Only")]
        PickOnly,
        [Display(Name = "Receive Only")]
        ReceiveOnly,
        [Display(Name = "Repair")]
        Repair
    }

}
