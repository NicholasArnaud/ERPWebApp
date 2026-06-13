using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Models
{
    public class ShipStationStore
    {
        [Key]
        public int ShipStationStoreId { get; set; }
        [Required]
        public int StoreId { get; set; }
        [Required]
        [Display(Name = "Store Name")]
        [StringLength(100)]
        public string StoreName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Public Email")]
        public string PublicEmail { get; set; }
        public bool IsActive { get; set; }
        [Display(Name = "Increased Pricing")]
        public bool HasIncreasedPricing { get; set; }
        [Display(Name = "Store Type")]
        public StoreType StoreType { get; set; }
        public List<ShipStationStoreFile> StoreFiles { get; set; }
        [Display(Name = "Files")]
        [NotMapped]
        //It Can be moved to ViewModel or DTO in future.
        public List<IFormFile> RawFiles { get; set; }

        [Display(Name ="Contact Name")]
        [StringLength(250)]
        public string ContactName { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Fax Number")]
        [StringLength(15)]
        public string FaxNumber { get; set; }
        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(250)]
        public string Notes { get; set; }

        public ICollection<Stock> Stocks {get;set;} = new List<Stock>();
    }

    public enum StoreType
    {
        [Display(Name = "Etsy")]
        ETSY,
        [Display(Name = "Shopify")]
        SHOPIFY,
        [Display(Name = "Amazon")]
        AMAZON,
        [Display(Name = "Custom")]
        CUSTOM
    }
}