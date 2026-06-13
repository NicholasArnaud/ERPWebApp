using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Company.Security
{
    public class AccessPoint
    {
        [Key]
        public int AccessPointId { get; set; }
        [Required]
        [Display(Name = "Location")]
        public string AccessPointLocation { get; set; }
        [Required]
        [Display(Name = "IP Address")]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", ErrorMessage = "Must be a valid IPv4 Address")]
        public string IpAddress { get; set; }
        [Required]
        [Display(Name = "MAC Address")]
        [RegularExpression(@"([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}", ErrorMessage = "Must be a valid MAC Address")]
        public string MacAddress { get; set; }
        [Required]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }
        [Required]
        public AccessPointStatus Status { get; set; }
        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
    }
    public enum AccessPointStatus
    {
        Closed,
        Open
    }
}
