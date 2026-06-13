using ERPWebApp.Models.Inventory.SkuProperties;

namespace ERPWebApp.UnitTests.Fixtures
{
    internal class SkuUnitOfMeasureFixtures
    {
        public static List<SkuUnitOfMeasure> GetTestUnitOfMeasures() =>
        [
            new SkuUnitOfMeasure {
                SkuUnitOfMeasureId = 1,
                UnitOfMeasure = "Meter",
                Attribute = "Length",
                LastModified = DateTime.Now,
                IsActive = true
            },
            new SkuUnitOfMeasure {
                SkuUnitOfMeasureId = 2,
                UnitOfMeasure = "Gram",
                Attribute = "Weight",
                LastModified = DateTime.Now,
                IsActive = true
            }
        ];
    }
}
