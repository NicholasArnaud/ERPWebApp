using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory
{
    public class ProductTagsRegistry
    {
        [Key]
        public int TagId { get; set; }
        [StringLength(25)]
        public string Description { get; set; }
        [StringLength(20)]
        public string Color { get; set; }
    }
}