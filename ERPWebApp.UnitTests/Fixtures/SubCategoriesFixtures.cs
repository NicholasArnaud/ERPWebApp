namespace ERPWebApp.UnitTests.Fixtures
{
    public class SubCategoriesFixtures
    {
        public static List<SubCategory> GetTestSubCategories() =>
        [
            new SubCategory { SubCategoryId = 1, Description = "Shirts", IsActive = true },
            new SubCategory { SubCategoryId = 2, Description = "Pants", IsActive = true },
            new SubCategory { SubCategoryId = 3, Description = "Sweaters", IsActive = false },
            new SubCategory { SubCategoryId = 4, Description = "Dresses", IsActive = true }
        ];
    }
}