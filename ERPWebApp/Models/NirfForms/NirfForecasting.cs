using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfForecasting
    {
        [Key]
        [Display(Name = "NirfForcastingId")]
        public int NirfForecastingId { get; set; }
        [Display(Name = "Lead Time")]
        public int LeadTime { get; set; }
        [Display(Name = "Min Max Level")]
        [StringLength(50)]
        public string MinMaxLevel { get; set; }
        [Display(Name = "Count")]
        [StringLength(50)]
        public string Count { get; set; }
        [Display(Name = "Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }
        [Display(Name = "Signed On")]
        public DateTime SignedOn { get; set; }
        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }
        [Display(Name = "Nirf Form")]
        public int NirfFormId { get; set; }

        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
        [Display(Name = "Comments")]
        [StringLength(250)]
        public string Comments { get; set; }
    }
}
