using ERPWebApp.Models.PurchaseOrders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
    public class PurchaseOrderFilesMapping
    {
        [Key]
        public int PurchaseOrderFilesMappingId { get; set; }
        [Display(Name = "PurchaseOrder")]
        public int PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        [Display(Name = "Files")]
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
    }
}
