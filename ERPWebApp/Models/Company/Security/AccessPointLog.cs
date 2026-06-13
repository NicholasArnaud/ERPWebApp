using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company.Security
{
    public class AccessPointLog
    {
        [Key]
        public int AccessPointLogId { get; set; }
        [ForeignKey("AccessCardId")]
        public virtual AccessCard AccessCard { get; set; }
        [Display(Name = "Access Card")]
        public int AccessCardId { get; set; }
        [ForeignKey("AccessPointId")]
        public virtual AccessPoint AccessPoint { get; set; }
        [Display(Name = "Access Point")]
        public int AccessPointId { get; set; }

        [Display(Name = "Recieved Uid")]
        public string RecievedUID { get; set; }

        [Display(Name = "Recieved Key")]
        public string RecievedKey { get; set; }

        [Display(Name = "Recieved Password")]
        public string RecievedPassword { get; set; }

        [Display(Name = "Recieved Mac Address")]
        public string RecievedMacAddress { get; set; }

        [Display(Name = "Recieved Ip Address")]
        public string RecievedIpAddress { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }
        public bool IsSuccess { get; set; }
    }
}
