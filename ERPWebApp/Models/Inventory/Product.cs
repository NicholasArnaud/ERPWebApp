using ERPWebApp.Attributes;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Mappings;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(Sku), IsUnique = true)]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public int? SubCategoryId { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }

        private string _sku;
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string Sku
        {
            get => _sku;
            set => _sku = value?.Trim().ToUpperInvariant();
        }

        private string _description;

        [Required]
        [StringLength(400)]
        [StringSanitizationAttributes]
        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_description)
                 ? string.Empty
                 : WebUtility.HtmlDecode(_description);
            }
            set
            {
                _description = value;
            }
        }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fulfillment Cost")]
        public decimal FulfillmentCost { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "USA cost")]
        public decimal Cost { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Labor Cost")]
        public decimal LaborCost { get; set; }

        [Display(Name = "Alt Item Number")]
        public string AltItemNumber { get; set; }

        [Display(Name = "Alternate Product")]
        public int? AlternateProductId { get; set; }
        [ForeignKey("AlternateProductId")]
        public virtual Product AlternateProduct { get; set; }

        [Display(Name = "On Order")]
        public int OnOrder { get; set; }

        [Display(Name = "Embroidery")]
        public bool IsEmbroidery { get; set; }

        [Display(Name = "Engraving")]
        public bool IsEngraving { get; set; }

        [Display(Name = "Metal")]
        public bool IsMetal { get; set; }

        [Display(Name = "UV")]
        public bool IsUv { get; set; }

        [Display(Name = "Lead Time")]
        public int LeadTime { get; set; }

        [Display(Name = "Weight Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightAmount { get; set; }

        [Display(Name = "Weight Unit")]
        public WeightUnit WeightUnit { get; set; }

        [Display(Name = "Active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }

        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }
        public string ModifySource { get; set; }

        public virtual List<Department> Departments { get; set; }
        public virtual List<Stock> Stocks { get; set; }

        private List<int> _departmentList;
        [NotMapped]
        public List<int> DepartmentList
        {
            get
            {
                if (_departmentList == null && Departments != null)
                {
                    _departmentList = Departments.Select(d => d.DepartmentId).ToList();
                }
                return _departmentList;
            }
            set => _departmentList = value;
        }

        public virtual List<ProductVendorMapping> ProductVendorMappings {get;set;}

        public virtual List<ProductImage> ProductImages { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Height field must be a non-negative decimal.")]
        public decimal Height { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Width field must be a non-negative decimal.")]
        public decimal Width { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Length field must be a non-negative decimal.")]
        public decimal Length { get; set; }

        [Display(Name = "Dimensional Unit")]
        public DimensionalUnit DimensionalUnit { get; set; }

        [Display(Name = "External Site Product")]
        [DefaultValue(false)]
        public bool IsExternalProduct { get; set; }

        [Display(Name = "Minimum Inventory")]
        [Range(0, int.MaxValue, ErrorMessage = "The Minimum Inventory field must be a non-zero positive integer.")]
        public int MinInventory { get; set; }

        [Display(Name = "Maximum Inventory")]
        [Range(0, int.MaxValue, ErrorMessage = "The Maximum Inventory field must be a non-zero positive integer.")]
        public int MaxInventory { get; set; }

        [NotMapped]
        [Display(Name = "Total Inventory")]
        public int StockTotalAvailable { get; set; }

        [NotMapped]
        public int StockTotalAvailableFilter { get; set; }

        [NotMapped]
        public string ImageSrc { get; set; }

        [NotMapped]
        public string ImageSrcDtl { get; set; }

        [NotMapped]
        public string permission { get; set; }

        [NotMapped]
        public string costpermission { get; set; }

        [Display(Name = "ProductTags")]
        public ICollection<ProductTag>? ProductTags { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,4)")]
        [Display(Name = "Overseas Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "The Overseas Cost field must be a non-negative decimal.")]
        public decimal OverseasCost { get; set; }
        [Display(Name = "Shipping Weight Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingWeightAmount { get; set; }
        [Display(Name = "Shipping Weight Unit")]
        public WeightUnit ShippingWeightUnit { get; set; }
        [Display(Name = "Is Shipping Container")]
        public bool IsShippingContainer { get; set; }
        [Display(Name = "Expected Shipping Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedShipmentCost { get; set; }
        [Display(Name = "Shipping Height")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Height field must be a non-negative decimal.")]
        public decimal ShippingHeight { get; set; }
        [Display(Name = "Shipping Width")]

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Width field must be a non-negative decimal.")]
        public decimal ShippingWidth { get; set; }
        [Display(Name = "Shipping Length")]

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "The Length field must be a non-negative decimal.")]
        public decimal ShippingLength { get; set; }
    }

    public enum WeightUnit
    {
        [Display(Name = "Ounce(s)")]
        Ounce,

        [Display(Name = "Pound(s)")]
        Pound
    }

    public enum DimensionalUnit
    {
        Inches,
        Feet,
        Centimeters,
        Meters
    }
}
