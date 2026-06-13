namespace ERPWebApp.UnitTests.Fixtures
{
    public class SubCategoryFixtures
    {
        public static List<SubCategory> GetTestSubCategories() =>
    [
        new SubCategory()
        {
            SubCategoryId = 1, Description = "Electronics", IsActive = true
        },
        new SubCategory()
        {
            SubCategoryId = 2, Description = "Furniture", IsActive = true
        },
        new SubCategory()
        {
            SubCategoryId = 3, Description = "Clothing", IsActive = true
        },
        new SubCategory()
        {
            SubCategoryId = 4, Description = "Books", IsActive = false
        },
        new SubCategory()
        {
            SubCategoryId = 5, Description = "Sports", IsActive = true
        },
        new SubCategory()
        {
            SubCategoryId = 6, Description = "Toys", IsActive = false
        }
    ];
    }
}
