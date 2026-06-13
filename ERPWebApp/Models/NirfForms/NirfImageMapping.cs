using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfImageMapping
    {
        [Key]
        [Display(Name = "NerfImageMappingId")]
        public int NirfImageMappingId { get; set; }
        [Display(Name = "File")]
        public int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }

        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
        [Display(Name = "Is Thumbnail")]
        [DefaultValue(false)]
        public bool IsThumbnail { get; set; }
    }
}
