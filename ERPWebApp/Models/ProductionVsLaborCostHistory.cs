using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class ProductionVsLaborCostHistory
    {
        [Key]
        public int ProductionVsLaborCostHistoryId { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Display(Name = "Electroplating Item Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal ElectroplatingItemCost { get; set; }
        [Display(Name = "Embroidery Item Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EmbroideryItemCost { get; set; }
        [Display(Name = "Engraving Item Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EngravingItemCost { get; set; }
        [Display(Name = "Metal Item Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal MetalTotalItemCost { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        [Display(Name = "UVP Item Cost")]
        public decimal UVPTotalItemCost { get; set; }

        [Display(Name = "Electroplating Production Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal ElectroplatingProdCost { get; set; }
        [Display(Name = "Embroidery Production Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EmbroideryProdCost { get; set; }
        [Display(Name = "Engraving Production Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal EngravingProdCost { get; set; }
        [Display(Name = "Metal Production Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal MetalTotalProdCost { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        [Display(Name = "UVP Production Cost")]
        public decimal UVPTotalProdCost { get; set; }

    }
}
