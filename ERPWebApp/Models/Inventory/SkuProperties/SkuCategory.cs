using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory.SkuProperties
{
    public class SkuCategory
    {
        [Key]
        public int SkuCategoryId { get; set; }
        [Required]
        [MaxLength(50), MinLength(3)]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9\s]*$", ErrorMessage = "The field must start with an alphanumeric character and can only contain alphanumeric characters and spaces.")]
        public string Category { get; set; }

        [Required]
        [MaxLength(25), MinLength(3)]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9\s]*$", ErrorMessage = "The field must start with an alphanumeric character and can only contain alphanumeric characters and spaces.")]
        public string Attribute { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime LastModified { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
