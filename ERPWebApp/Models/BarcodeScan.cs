using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class BarcodeScan
    {
        [Key]
        public int BarcodeScanId { get; set; }
        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 5)]
        public string BarcodeScanCode { get; set; }
        public DateTime ModifyDate { get; set; }
        [StringLength(maximumLength: 250)]
        public string ShipStationOrderId { get; set; }
    }
}
