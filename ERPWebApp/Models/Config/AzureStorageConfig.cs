namespace ERPWebApp.Models.Config
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; }
        public string ImageContainer { get; set; }
        public string ThumbnailContainer { get; set; }
        public string DocumentContainer { get; set; }
        public string DHLInvoiceContainer { get; set; }
        public string UPSInvoiceContainer { get; set; }
        public string StampsUSPSInvoiceContainer { get; set; }
        public string EasyPostInvoiceContainer { get; set; }
        public string DefaultContainer { get; set; }
        public string SkulabsImportContainer { get; set; }
        public string ShippingManifests { get; set; }
    }
}
