using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.NirfForms

{
    public class Fonts
    {
        [Key]
        public int FontId { get; set; }
        [Display(Name = "Font Title")]
        [StringLength(150)]
        public string FontTitle { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }

        [Display(Name = "Modified By User")]
        [StringLength(50)]
        public string ModifyByUser { get; set; }

        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

    }
}
