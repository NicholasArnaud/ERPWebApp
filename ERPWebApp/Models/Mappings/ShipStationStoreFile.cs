using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
    public class ShipStationStoreFile
    {
        [Key]
        public int StoreFileId { get; set; }
        public int ShipStationStoreId { get; set; }
        [ForeignKey("ShipStationStoreId")]
        public ShipStationStore Store { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files Files { get; set; }
    }
}