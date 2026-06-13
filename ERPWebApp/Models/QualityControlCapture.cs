
using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class QualityControlCapture
    {
        [Key]
        public int QualityControlCaptureId { get; set; }

        [Required]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }

        [Display(Name = "Location")]
        [ForeignKey("QCStationLocationId")]
        public int? QCStationLocationId { get; set; }
        public virtual QCStationLocation Locations { get; set; }
        [ForeignKey("DepartmentId")]
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public virtual Department Departments { get; set; }

        [Required]
        [Display(Name = "SKU Number")]
        public string SkuNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Order Date")]
        public DateTime? OrderDate { get; set; }
        [ForeignKey("QCDiagnosisId")]
        [Display(Name = "Diagnosis")]
        public int? QCDiagnosisId { get; set; }
        public virtual QCDiagnosis Diagnoses { get; set; }


        [ForeignKey("EmployeeId")]
        [Display(Name = "Employee")]
        public int? EmployeeId { get; set; }
        public virtual Employee Employees { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Capture Date")]
        public DateTime? CaptureDate { get; set; }

        [Display(Name = "Q.C. Person")]
        public string QCPerson { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; }

        public string Notes { get; set; }


    }
}