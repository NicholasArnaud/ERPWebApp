using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Company
{
    public class UserImage
    {
        [Key]
        public int UserImageId { get; set; }
        [StringLength(255)]
        public string UserId { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
        [Display(Name = "File Url")]
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
     
    }
}
