using System.ComponentModel;

namespace ERPWebApp.Data.Enum
{
    public enum ReportTypes
    {
        [Description("Average Shipment Cost by SKU and Date Range")]
        AverageShipmentCostBySkuAndDateRange = 1,

        [Description("Average Shipment Cost by Service and Date Range")]
        AverageShipmentCostByServiceAndDateRange = 2,

        [Description("All Item Amounts Shipped by Date Range")]
        AllItemAmountsShippedByDateRange = 3,

        [Description("Amount an Item Has Been Shipped by Date Range")]
        AmountAnItemHasBeenShippedByDateRange = 4,

        [Description("Get Sum of Order Sales by Date Range")]
        GetSumOfOrderSalesByDateRange = 5,

        [Description("Get On-Hand by Sites")]
        GetOnHandBySites = 6,

        [Description("Inventory Balance")]
        InventoryBalance = 7,

        [Description("Stock History Report")]
        StockHistoryReport = 8,

        [Description("Product Stock Report")]
        ProductStockReport = 9,

        [Description("Cycle Count Report")]
        CycleCountReport = 10,

        [Description("Stock History Report with Additions/Subtractions")]
        StockHistoryReportWithAdditionsSubtractions = 11,

        [Description("Weekly Gross Profit")]
        WeeklyGrossProfit = 12,

        [Description("Yearly Sold Product Report")]
        YearlySoldProductReport = 13

    }
}
