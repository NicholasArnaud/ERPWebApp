using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.DTOModels
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public string CarrierType { get; set; }
        public DateTime UploadDate { get; set; }
        public string FileName { get; set; }
        public string UploadedBy { get; set; } = "Unknown";
        public string FileUrl { get; set; } = "#";
        public string FormattedUploadDate => UploadDate.ToString("yyyy-MM-dd");
        public decimal TotalCost { get; set; }
    }
}
