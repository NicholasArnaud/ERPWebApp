using ERPWebApp.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
    public class ProductFilesMappings
    {
        [Key]
        public int ProductFilesMappingId { get; set; }
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        [Display(Name = "Files")]
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
        [Display(Name = "Detailed Image")]
        public bool IsDetailedImage { get; set; }
        [Display(Name = "Thumbnail Image")]
        public bool IsThumbnail { get; set; }
    }
}
