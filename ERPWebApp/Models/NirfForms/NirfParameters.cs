using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfParameters
    {
        [Key]
        [Display(Name = "NirfParametersId")]
        public int NirfParametersId { get; set; }
        [Display(Name = "Loop Count")]
        public int? LoopCount { get; set; }
        [Display(Name = "Speed")]
        public int? Speed { get; set; }
        [Display(Name = "Current")]
        public int? Current { get; set; }
        [Display(Name = "Frequency")]
        public int? Frequency { get; set; }
        [Display(Name = "Sizing X")]
        public int? SizingX { get; set; }
        [Display(Name = "Sizing Y")]
        public int SizingY { get; set; }
        [Display(Name = "White Layers")]
        public int? WhiteLayers { get; set; }
        [Display(Name = "Color Layers")]
        public int? ColorLayers { get; set; }
        [Display(Name = "Signed By")]
        [StringLength(50)]
        public string SignedBy { get; set; }
        [Display(Name = "Signed On")]
        public DateTime SignedOn { get; set; }
        [Display(Name = "Asp Id")]
        [StringLength(50)]
        public string AspUserId { get; set; }

        [Display(Name = "NirfForm")]
        public int NirfFormId { get; set; }

        [ForeignKey("NirfFormId")]
        public virtual NirfForm NirfForm { get; set; }
        [Display(Name = "Comments")]
        [StringLength(250)]
        public string Comments { get; set; }
        [Display(Name = "UVP Type")]
        public UVPTypes UVPTypes { get; set; }
        [Display(Name = "Thread Type")]
        public ThreadType ThreadTypes { get; set; }
        [Display(Name = "Time To Complete")]
        public DateTime TimeToComplete { get; set; }
        [Display(Name = "Temperature")]
        public int? Temperature { get; set; }
        [Display(Name = "Temperature Scale")]
        public bool? IsFahrenheit { get; set; }
        //thread colors
        [Display(Name = "Thread Color")]
        [StringLength(50)]
        public string ThreadColor { get; set; }
        [Display(Name = "Thread Hex Value")]
        [StringLength(50)]
        public string ThreadHex { get; set; }
        [Display(Name = "Thread Code")]
        [Column(TypeName = "decimal(18,5)")]
        public decimal? ThreadCode { get; set; }
        [ForeignKey("FontId")]
        public Fonts Font { get; set; }
        [Display(Name = "Fonts")]
        [StringLength(50)]
        public int? FontId { get; set; }
    }

    public enum UVPTypes
    {
        Printed,
        Stained
    }

    public enum ThreadType
    {
        Hex,
        Code
    }
}
