using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class NirfProductMappingFixtures
    {
        public static List<NirfProductMapping> GetTestList() => [
            new NirfProductMapping
            {
                NirfProductMappingId = 1,
                NirfFormId = 1001,
                ProductId = 2001,
                Product = ProductFixtures.GetTestProducts().FirstOrDefault(static x=>x.ProductId == 1),
                 NirfForm = NirfFormFixtures.GetTestList().FirstOrDefault(static x=>x.NirfFormId == 1)
            },
            new NirfProductMapping
            {
                NirfProductMappingId = 2,
                NirfFormId = 1002,
                ProductId = 2002,
                Product = ProductFixtures.GetTestProducts().FirstOrDefault(static x=>x.ProductId == 2),
                 NirfForm = NirfFormFixtures.GetTestList().FirstOrDefault(static x=>x.NirfFormId == 1)
            },
            new NirfProductMapping
            {
                NirfProductMappingId = 3,
                NirfFormId = 1003,
                ProductId = 2003,
                Product = ProductFixtures.GetTestProducts().FirstOrDefault(static x=>x.ProductId == 1),
                NirfForm = NirfFormFixtures.GetTestList().FirstOrDefault(static x=>x.NirfFormId == 1)
            }
         ];
    }
}