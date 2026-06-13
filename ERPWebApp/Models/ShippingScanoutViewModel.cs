
using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels.ShippingScanout;

namespace ERPWebApp.Models
{
    public class ShippingScanoutViewModel
    {
        public ShippingScanout CurrentScan { get; set; }
        public string DeleteScan { get; set; }
        public List<ShippingScanout> HistoricalScans { get; set; } = [];
        public List<Tuple<string, int, int>> CarrierDayTotal { get; set; } = [];
        public ScanActionType ActionType { get; set; }
        public bool AudioAlerts { get; set;} = true;
        public string TrackingNumber { get; set; }
        public ShippingScanout SearchedScanned { get; set; }
    }
    
}