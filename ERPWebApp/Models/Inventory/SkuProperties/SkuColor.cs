using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory.SkuProperties
{
    public class SkuColor
    {
        [Key]
        public int SkuColorId { get; set; }

        [Required]
        [MaxLength(25), MinLength(3)]
        public string Color { get; set; }

        [MaxLength(25), MinLength(3)]
        public string Attribute { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime LastModified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
