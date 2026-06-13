namespace ERPWebApp.Data.DTOModels
{
    public class MovedProductsDto
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductCycleCountDto
    {
        public int StockQuantity { get; set; }
        public string ProductSku { get; set; }
        public string SiteName { get; set; }
        public int DaysSinceLastVerification { get; set; }
    }


    public class RequestedProductsDto
    {
        public string ProductSku { get; set; }
        public int SkuCount { get; set; }
        public int QuantityNeeded { get; set; }
        public double Percentage { get; set; }
    }

    public class RequestedReasonDto
    {
        public string PickReason { get; set; }
        public int ReasonCount { get; set; }
        public double Percentage { get; set; }
    }

    public class VolumetricsDto
    {
        public int VolumeTally { get; set;}
        public int VolumeTarget { get; set; }
        public double VolRatio { get; set; }
        public string GradToColors { get; set; }
        public string SiteNames { get; set; }
        public int SiteId {get; set;}
        public decimal SiteVolume { get; set;}
    }
}
