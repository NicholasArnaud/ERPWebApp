using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class SubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required]
        [MaxLength(50), MinLength(3)]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
