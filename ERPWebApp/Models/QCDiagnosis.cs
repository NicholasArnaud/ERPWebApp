using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class QCDiagnosis
    {
        [Key]
        public int QCDiagnosisId { get; set; }
        [Display(Name = "QC Diagnosis")]
        [Required]
        public string QCDiagnosisName { get; set; }
        public bool IsActive { get; set; }
    }
}