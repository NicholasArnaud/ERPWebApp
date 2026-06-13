using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class BarcodeScanCount
    {
        [Display(Name = "Scan Hour")]
        public DateTime ScanDateTimeHour { get; set; }
        public int Count { get; set; }
    }
}
