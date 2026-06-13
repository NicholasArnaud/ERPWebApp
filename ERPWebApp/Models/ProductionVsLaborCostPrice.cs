using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class ProductionVsLaborCostPrice
    {
        [Key]
        public int ProductionVsLaborCostPriceId { get; set; }
        [Display(Name = "Electroplating Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal ElectroplatingItemCost { get; set; }
        [Display(Name = "Embroidery Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EmbroideryItemCost { get; set; }
        [Display(Name = "Engraving Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EngravingItemCost { get; set; }
        [Display(Name = "Metal Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal MetalItemCost { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        [Display(Name = "UV Cost")]
        public decimal UVItemCost { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifyByUser { get; set; }
    }
}
