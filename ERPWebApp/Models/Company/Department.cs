using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Company
{
    [Index(nameof(DepartmentName), IsUnique = true)]
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }
        [Display(Name = "Department Color")]
        public string DepartmentColor { get; set; }
        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        public bool IsProduction { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
