using ERPWebApp.Models;

namespace ERPWebApp.Data.DTOModels
{
    public class ShipStationOrderedHistoryTableDTO
    {
        public List<ShipStationOrderedHistory> Data { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }
}
