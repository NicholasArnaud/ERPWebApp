using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(BundleId),nameof(ProductId),IsUnique = true)]
    public class BundleItem
    {
        [Key]
        public int BundleItemId { get; set; }
        [ForeignKey("BundleId")]
        public Bundle Bundle { get; set; }
        [Display(Name = "Bundle")]
        public int BundleId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The quantity must be greater than 0.")]
        public int Quantity { get; set; }
    }
}
