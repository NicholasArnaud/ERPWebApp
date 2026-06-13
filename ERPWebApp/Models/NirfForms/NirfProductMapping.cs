using ERPWebApp.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERPWebApp.Models.NirfForms
{
    public class NirfProductMapping
    {
        [Key]
        [Display(Name = "NirfProductMappingId")]
        public int NirfProductMappingId { get; set; }
        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }
        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

    }
}
