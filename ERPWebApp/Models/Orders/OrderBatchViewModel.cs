using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;


namespace ERPWebApp.Models.Orders
{
    public class OrderBatchViewModel
    {
        public int OrderBatchId { get; set; }

        [Display(Name = "Batch Number")]
        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; }

        [Display(Name = "Status")]
        [Required]
        public OrderBatchStatus Status { get; set; }

        [Display(Name = "ShipStation Orders")]
        [Required]
        public List<long> ShipStationOrderIds { get; set; }

        [Display(Name = "Products")]
        [Required]
        public List<ProductMappingViewModel> ProductMappings { get; set; }

        public OrderBatchViewModel()
        {
            ShipStationOrderIds = new List<long>();
            ProductMappings = new List<ProductMappingViewModel>();
        }
    }

    public class ProductMappingViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Sku")]
        [StringLength(30)]
        public string Sku { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Site Name")]
        [StringLength(30)]
        public string SiteName { get; set; }

        [Display(Name = "Received")]
        public bool Received { get; set; }
    }
    public class DesignBatchProductMappingViewModel
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string DepartmentName { get; set; }
        public ProductStatus Status { get; set; }
        public string ImageSrc { get; set; }
        public int OrderBatchId { get; set; }
        public int OrderBatchproductMappingId { get; set; }
    }

    public class BatchItemViewModel
    {
        public OrderBatchItem BatchItem { get; set; }
        public string Sku { get; set; }
        public List<LocationInfo> LocationInfo { get; set; }
        public List<string> FilteredProductSkus { get; set; }
        public bool IsAltItem { get; set; }
        public int Multiplier { get; set; } = 1;
    }

    public class DesignBatchItemViewModel
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string DepartmentName { get; set; }
        public BatchItemStatus Status { get; set; }
        public OrderBatchItem BatchItem { get; set; }
        public string ImageSrc { get; set; }
        public int OrderBatchId { get; set; }
        public int OrderBatchItemId { get; set; }
        public List<int> OrderBatchItemIdList { get; set; }
        public bool IsValidStock { get; set; }
        public bool RequiresPO { get; set; }
        public int? ERPOrderId { get; set; }
        public List<LocationInfo> PickOnlyLocations { get; set; }
        public List<LocationInfo> ReceiveOnlyLocations { get; set; }
    }
    public class DepartmentStatusLineViewModel
    {
        public Department Department { get; set; }
        public List<BatchItemStatus> Statuses { get; set; }
    }

}
