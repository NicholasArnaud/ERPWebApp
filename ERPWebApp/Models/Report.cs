namespace ERPWebApp.Models
{
    public class Report
    {

        public decimal Average { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public string CarrierCode { get; set; }
        public string Service { get; set; }
        public int ProductId { get; set; }
        public string LocationName { get; set; }
        public int OnHand { get; set; }
        public decimal TotalCost { get; set; }
        public int Amount { get; set; }
        public string Department { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public string Location { get; set; }
        public int TotalAvailable { get; set; }
        public string SiteName { get; set; }
        public int ShipStationOrders { get; set; }
        public int OrderDifference { get; set; }

        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public string Action { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int MaxInventoryAmount { get; set; }
        public string PrimaryVendorName { get; set; }
    }

    public class ReportMetaData
    {

        public int TotalRecords { get; set; }
        public List<Report> ReportItemsList { get; set; }

    }

}

