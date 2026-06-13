using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace ERPWebApp.Models.NirfForms
{
    public class NirfForm
    {
        [Key]
        [Display(Name = "NirfFormId")]
        public int NirfFormId { get; set; }
        [Display(Name = "Seller Sku")]
        [StringLength(50)]
        [Required]
        public string SellersProductSku { get; set; }
        [Display(Name = "Orientation")]
        [StringLength(50)]
        public string Orientation { get; set; }
        [Display(Name = "Design Placement")]
        [StringLength(50)]
        public string DesignPlacement { get; set; }
        [Display(Name = "Design Alignment")]
        [StringLength(150)]
        public string DesignAlignment { get; set; }
        [Display(Name = "Special Instructions")]
        [StringLength(50)]
        public string SpecialInstructions { get; set; }
        [Display(Name = "Created By")]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartedDate { get; set; }
        [Display(Name = "Completed Date")]
        public DateTime CompletedDate { get; set; }
        [Display(Name = "Status")]
        public Status NirfStatus { get; set; }
        public enum Status
        {
            InProgress,
            Cancelled,
            Completed
        }
        [DefaultValue(false)]
        public bool IsLoopCount { get; set; }
        [DefaultValue(false)]
        public bool IsSpeed { get; set; }
        [DefaultValue(false)]
        public bool IsCurrent { get; set; }
        [DefaultValue(false)]
        public bool IsFrequency { get; set; }
        [DefaultValue(false)]
        public bool IsSizingX { get; set; }
        [DefaultValue(false)]
        public bool IsSizingY { get; set; }
        [DefaultValue(false)]
        public bool IsWhiteLayer { get; set; }
        [DefaultValue(false)]
        public bool IsColorLayer { get; set; }
        [DefaultValue(false)]
        public bool IsUVPType { get; set; }
        [DefaultValue(false)]
        public bool IsTemperature { get; set; }
        [DefaultValue(false)]
        public bool IsFont { get; set; }
        [DefaultValue(false)]
        public bool IsThreadColor { get; set; }
    }
}
