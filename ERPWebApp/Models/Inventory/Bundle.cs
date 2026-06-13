using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(BundleName), IsUnique = true)]
    public class Bundle
    {
        [Key]
        public int BundleId { get; set; }
        [MaxLength(50),MinLength(6),Required]
        public string BundleName { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fulfillment Cost"), DisplayFormat(DataFormatString = "{0:C}")]
        public decimal FulfillmentCost { get; set; }
        public ICollection<BundleItem>BundleItems { get; set; }
    }
}
