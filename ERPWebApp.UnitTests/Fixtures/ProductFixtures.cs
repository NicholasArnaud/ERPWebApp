using ERPWebApp.Models.Inventory;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class ProductFixtures
    {
        public static List<Product> GetTestProducts() =>
         [
            new Product()
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
                Departments = DepartmentsFixtures.GetTestDepartments().FindAll(static x=>x.DepartmentId == 1 || x.DepartmentId == 2),
                ProductImages = ProductImageFixtures.GetProductImageFixtures(),
                StockTotalAvailable = 10,
                IsActive = true,
                IsExternalProduct =true,
            },
            new Product()
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
                Departments = DepartmentsFixtures.GetTestDepartments().FindAll(static x=>x.DepartmentId == 3 || x.DepartmentId == 4),
                ProductImages = ProductImageFixtures.GetProductImageFixtures(),
                StockTotalAvailable = 20,
                IsActive = true,
                IsExternalProduct =false,
            }
         ];
    }
}