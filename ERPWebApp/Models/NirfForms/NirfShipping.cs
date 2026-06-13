using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfShipping
    {
        [Key]
        [Display(Name = "NirfShippingId")]
        public int NirfShippingId { get; set; }

        [Display(Name = "Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }

        [Display(Name = "Signed On")]
        public DateTime SignedOn { get; set; }

        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }

        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }

        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }

        [Display(Name = "Comments")]
        [StringLength(250)]
        public string Comments { get; set; }

        public virtual ICollection<NirfShippingProdivder> NirfShippingProvider { get; set; }

    }
}
