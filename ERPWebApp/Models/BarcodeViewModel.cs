namespace ERPWebApp.Models
{
    public class BarcodeViewModel
    {
        public List<BarcodeScanCount> BarcodeScanCounts { get; set; } = [];
        public BarcodeScan BarcodeScan { get; set; } = new();
        public List<int> TagIds {get;set;} = [];
    }
}


