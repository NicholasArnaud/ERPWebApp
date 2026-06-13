using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;

namespace ERPWebApp.Models
{
    public class InventoryViewModel
    {
        public List<MovedProductsDto> MovedProductsInfo { get; set; }
        public List<ProductCycleCountDto> ProductCycleCountInfo { get; set; }
        public List<RequestedProductsDto> ProductRequestInfo { get; set; }
        public List<RequestedReasonDto> RequestedReasonInfo { get; set; }
        public List<VolumetricsDto> VolumetricsInfo { get; set; }
        public List<DashboardLayout> DashboardLayouts { get; set; }
        public string movedProductsData { get; set; }
        public string productCycleCountData { get; set; }
        public string productRequestData { get; set; }
        public string requestedReasonData { get; set; }
        public string volumetricsData { get; set; }
        public bool SiteVolumetrics { get; set; }
        public bool ProductCyleCount { get; set; }
        public bool TopRequestedProducts { get; set; }
        public bool TopMovedProducts { get; set; }
        public bool TopReasonRequest { get; set; }

        public InventoryViewModel()
        {
            MovedProductsInfo = new List<MovedProductsDto>();
            ProductCycleCountInfo = new List<ProductCycleCountDto>();
            ProductRequestInfo = new List<RequestedProductsDto>();
            RequestedReasonInfo = new List<RequestedReasonDto>();
            VolumetricsInfo = new List<VolumetricsDto>();
            DashboardLayouts = new List<DashboardLayout>();
        }
    }
}