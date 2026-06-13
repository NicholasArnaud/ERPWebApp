using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class QCStationLocation
    {
        [Key]
        public int QCStationLocationId { get; set; }
        [Display(Name = "Station Location")]
        [Required]
        public string QCStationLocationName { get; set; }
        [ForeignKey("DepartmentId")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public virtual Department Departments { get; set; }

        public bool IsActive { get; set; }
    }
}