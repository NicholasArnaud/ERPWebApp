using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory.SkuProperties
{
    public class SkuUnitOfMeasure
    {
        [Key]
        public int SkuUnitOfMeasureId { get; set; }

        [Display(Name = "Measurements")]
        [Required]
        [MaxLength(25), MinLength(3)]
        public string UnitOfMeasure { get; set; }

        [MaxLength(25), MinLength(3)]
        public string Attribute { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime LastModified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
