
using ERPWebApp.Models.Mappings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Inventory
{
    public class ProductContainer
    {
        [Key]
        public int ContainerId { get; set; }

        [Display(Name = "ProductVendorMapping")]
        public int ProductVendorMappingId { get; set; }
        [ForeignKey("ProductVendorMappingId")]
        public virtual ProductVendorMapping ProductVendorMappings { get; set; }

        [Required]
        [Display(Name = "Container Quantity")]
        public int ContainerQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Container Length")]
        public decimal Length { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Container Width")]
        public decimal Width { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Container Height")]
        public decimal Height { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Dimensional Unit")]
        public ContainerDiminsions ContainerDiminsions { get; set; }

        [Required]
        [Display(Name = "Container Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ContainerCost { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }



    }
    public enum ContainerDiminsions
    {
        Inches,
        Feet,
        Centimeters,
        Meters
    }
}
