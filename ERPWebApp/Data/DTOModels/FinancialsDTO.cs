using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.DTOModels
{
    public class ProductSalesInfoDto
    {
        public string ProductSku { get; set; }
        public string Description { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class TrendsInfoDto
    {
        public DateTime? date { get; set; }
        public decimal? fulfillmentCost { get; set; }
        public string departmentName { get; set; }
        public Dictionary<string, int> departmentsOrders { get; set; }
        public Dictionary<string, decimal> departmentsFulfillmentCost { get; set; }
        public Dictionary<string, decimal> departmentsProductProfit { get; set; }
        public Dictionary<string, decimal> departmentsProductCost { get; set; }
        public string orderSku { get; set; }
        public int orderId { get; set; }
        public bool hasIncreasedPricing { get; set; }
        public decimal productCost { get; set; }
        public decimal? productProfit { get; set; }
    }

    public class FulfillmentInfoDto
    {
        public DateTime OrderDate { get; set; }
        public bool HasIncreasedPricing { get; set; }
        public string StoreName { get; set; }
        public string ProductSku { get; set; }
        public decimal? ProductFulfillmentCost { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentColor { get; set; }
        public Dictionary<string, decimal> StoreFulfillmentCost { get; set; }
        public decimal ProductCost { get; set; }
        public decimal? ProductProfit { get; set; }
    }

    public class YearlyProfitInfoDto
    {
        public DateTime Date { get; set; }
        public decimal Profits { get; set; }
        public decimal ShipStationSales { get; set; }
        public int Year { get; set; }
        public int ItemsSold { get; set; }
    }  
}
