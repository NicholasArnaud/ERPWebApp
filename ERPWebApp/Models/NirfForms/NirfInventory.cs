using ERPWebApp.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfInventory
    {
        [Key]
        [Display(Name = "NirfInventoryId")]
        public int NirfInventoryId { get; set; }

        [Display(Name = "Membrane Site")]
        public int MembraneSiteId { get; set; }

        [Display(Name = "Main Site")]
        public int MainSiteId { get; set; }

        [Display(Name = "Membrane Location")]
        public int MembraneLocationId { get; set; }

        [ForeignKey("MembraneLocationId")]
        public virtual Location MembraneLocation { get; set; }

        [Display(Name = "Main Location")]
        public int MainLocationId { get; set; }

        [ForeignKey("MainLocationId")]
        public virtual Location MainLocation { get; set; }

        [Display(Name = "Alt Main Location")]
        public int AltMainLocationId { get; set; }
        [ForeignKey("AltMainLocationId")]
        public virtual Location AltMainLocation { get; set; }

        [Display(Name = "Alt Membrane Location")]
        public int AltMembraneLocationId { get; set; }
        [ForeignKey("AltMembraneLocationId")]
        public virtual Location AltMembraneLocation { get; set; }

        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }
        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }

        [Display(Name = "Comments")]
        [StringLength(250)]
        public string Comments { get; set; }
        [Display(Name = "Signed On")]
        public DateTime SignedOn { get; set; }
        [Display(Name = "Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }
        [Display(Name = "Asp User")]
        [StringLength(50)]
        public string AspUserId { get; set; }
    }
}
