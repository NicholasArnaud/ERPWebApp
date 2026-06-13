using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Models.Mappings
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }
        [StringLength(255)]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
        [Display(Name = "File Url")]
        public string FileUrl { get; set; }
        [Display(Name = "Thumbnail Url")]
        public string ThumbnailUrl { get; set; }
        public bool IsDefault { get; set; }
    }
}