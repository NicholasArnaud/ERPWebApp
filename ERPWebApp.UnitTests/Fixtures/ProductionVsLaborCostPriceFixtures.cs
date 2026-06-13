namespace ERPWebApp.UnitTests.Fixtures
{
    public static class ProductionVsLaborCostPriceFixtures
    {
        public static List<ProductionVsLaborCostPrice> GetTestProductionVsLaborCostPrices() => [
            new ProductionVsLaborCostPrice
            {
                ProductionVsLaborCostPriceId = 1,
                ElectroplatingItemCost = 50.00m,
                EmbroideryItemCost = 30.00m,
                EngravingItemCost = 20.00m,
                MetalItemCost = 40.00m,
                UVItemCost = 25.00m,
                ModifyDate = new DateTime(2022, 12, 31),
                ModifyByUser = "JohnDoe"
            },
            new ProductionVsLaborCostPrice
            {
                ProductionVsLaborCostPriceId = 2,
                ElectroplatingItemCost = 60.00m,
                EmbroideryItemCost = 35.00m,
                EngravingItemCost = 25.00m,
                MetalItemCost = 45.00m,
                UVItemCost = 30.00m,
                ModifyDate = new DateTime(2023, 3, 15),
                ModifyByUser = "JaneSmith"
            }
        ];
    }
}