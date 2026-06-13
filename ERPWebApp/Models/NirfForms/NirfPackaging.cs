using ERPWebApp.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfPackaging
    {
        [Key]
        [Display(Name = "NirfPackagingId")]
        public int NirfPackagingId { get; set; }
        [Display(Name = "Box Size")]
        [StringLength(50)]
        public string BoxSize { get; set; }
        [Display(Name = "Bag")]
        [StringLength(50)]
        public string Bag { get; set; }
        [Display(Name = "Foam Wrap")]
        [StringLength(50)]
        public string FoamWrap { get; set; }
        [Display(Name = "Bubble Sleeve")]
        [StringLength(50)]
        public string BubbleSleeve { get; set; }
        [Display(Name = "Unites Per Box")]
        public int? UnitsPerBox { get; set; }
        [Display(Name = "Units Per Bag")]
        public int? UnitsPerBag { get; set; }
        [Display(Name = "Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }
        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }
        [Display(Name = "Signed On")]
        public DateTime SignedOn { get; set; }
        [Display(Name = "Container Height")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Height { get; set; }
        [Display(Name = "Container Width")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Width { get; set; }
        [Display(Name = "Container Length")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Length { get; set; }
        [Display(Name = "Container Size")]
        public ContainerDiminsions ContainerDiminsion { get; set; }
        [Display(Name = "Unites Per Container")]
        public int? UnitsPerContainer { get; set; }
        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }
        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
        [ForeignKey("Comments")]
        [StringLength(250)]
        public string Comments { get; set; }
    }
}
