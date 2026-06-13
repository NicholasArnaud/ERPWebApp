using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class StockFixtures
    {
        public static List<Stock> GetTestStocks() =>
         [
            new  Stock()
            {
                StockId =1,
                ProductId =1,
                Products = new Product()
                {
                     ProductId = 1,
                    SubCategoryId =1,
                    SubCategory = new Models.SubCategory()
                    {
                        SubCategoryId =1,
                        Description ="Sub_01"
                     },
                    Sku ="100",
                    Description = "Product_01",
                    IsUv =true,
                    Departments = DepartmentsFixtures.GetTestDepartments()
                },
                IsExternal =true,
                LocationId =1,
                Location = new Location()
                {
                    LocationId =1,
                    SiteId =1,
                    Sites = new Site()
                    {
                        SiteId =1,
                        SiteName = "Site01"
                    }
                },
                TotalAvailable = 10
            },
            new  Stock()
            {
                StockId =2,
                ProductId =2,
                Products = new Product()
                {
                    ProductId = 2,
                    SubCategoryId =2,
                    SubCategory = new Models.SubCategory()
                    {
                        SubCategoryId =2,
                        Description ="Sub_02"
                    },
                    Sku ="100",
                    Description = "Product_02",
                    Departments = DepartmentsFixtures.GetTestDepartments()
                },
                IsExternal =false,
                LocationId =1,
                Location = new Location()
                {
                    LocationId =1,
                    SiteId =1,
                    Sites = new Site()
                    {
                        SiteId =1,
                        SiteName = "Site01"
                    }
                },
                TotalAvailable = 20
            }
         ];
    }
}