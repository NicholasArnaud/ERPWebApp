using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class SpeedOMeterGoal
    {
        [Key]
        public int SpeedOMeterGoalId { get; set; }
        [Display(Name = "Electroplating Goal")]
        public int ElectroplatingGoal { get; set; }
        [Display(Name = "Embroidery Goal")]
        public int EmbroideryGoal { get; set; }
        [Display(Name = "Engraving Goal")]
        public int EngravingGoal { get; set; }
        [Display(Name = "Metal Goal")]
        public int MetalGoal { get; set; }
        [Display(Name = "UV Goal")]
        public int UVGoal { get; set; }
        [Display(Name = "Sublimation Goal")]
        public int SublimationGoal { get; set; }
        [Display(Name = "Plant Goal")]
        public int PlantGoal { get; set; }
        [Display(Name = "Wood Goal")]
        public int WoodGoal { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifyByUser { get; set; }
    }
}
