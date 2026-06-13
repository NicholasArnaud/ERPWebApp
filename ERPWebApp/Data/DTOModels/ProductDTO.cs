using System.ComponentModel.DataAnnotations;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.DTOModels
{
    /// <summary>
    /// Represents the Product
    /// </summary>
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public int? SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        [Display(Name = "Fulfillment Cost")]
        public decimal FulfillmentCost { get; set; }
        [Display(Name = "USA cost")]
        public decimal Cost { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Overseas Cost")]
        public decimal OverseasCost { get; set; }
        [Display(Name = "Labor Cost")]
        public decimal LaborCost { get; set; }
        [Display(Name = "Alt Item Number")]
        public string AltItemNumber { get; set; }
        [Display(Name = "Alternate Product")]
        public Product? AlternateProduct { get; set; }
        public int? AlternateProductId { get; set; }
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
        public decimal WeightAmount { get; set; }
        [Display(Name = "Weight Unit")]
        public WeightUnit WeightUnit { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifyDate { get; set; }
        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }
        public string ModifySource { get; set; }
        public List<int> DepartmentList { get; set; }
        public List<Department> Departments { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        [Display(Name = "Dimensional Unit")]
        public DimensionalUnit DimensionalUnit { get; set; }
        [Display(Name = "External Site Product")]
        public bool IsExternalProduct { get; set; }
        [Display(Name = "Minimum Inventory")]
        public int MinInventory { get; set; }
        [Display(Name = "Maximum Inventory")]
        public int MaxInventory { get; set; }
        [Display(Name = "Total Inventory")]
        public int StockTotalAvailable { get; set; }
        public int StockTotalAvailableFilter { get; set; }
        public string ImageSrc { get; set; }
        public string ImageSrcDtl { get; set; }
        public string Permission { get; set; }
        public string Costpermission { get; set; }
        public List<PurchaseOrder> PurchaseOrders { get; set; }
        public List<ProductVendorMapping> ProductVendors { get; set; }
        public List<ProductTagsRegistry> ProductTags { get; set; }
        public decimal ShippingWeightAmount { get; set; }
        public WeightUnit ShippingWeightUnit { get; set; }
        public bool IsShippingContainer { get; set; }
        public decimal ExpectedShipmentCost { get; set; }
        public decimal ShippingHeight { get; set; }
        public decimal ShippingWidth { get; set; }
        public decimal ShippingLength { get; set; }
    }
}
