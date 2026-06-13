using ERPWebApp.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfVendorMapping
    {
        [Key]
        [ForeignKey("NirfVendorMapping")]
        public int NirfVendorMappingId { get; set; }
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        [ForeignKey("VendorId")]
        public virtual Vendor Vendor { get; set; }
        [ForeignKey("Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }
        [ForeignKey("Signed on")]
        public DateTime SignedOn { get; set; }
        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }

        [ForeignKey("Nirf Form")]
        public int NirfFormId { get; set; }
        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
    }
}
