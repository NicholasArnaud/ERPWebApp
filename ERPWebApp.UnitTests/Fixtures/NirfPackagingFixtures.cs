using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfPackagingFixtures
    {
        public static List<NirfPackaging> GetTestList() => [
            new NirfPackaging
            {
                NirfPackagingId = 1,
                BoxSize = "Small",
                Bag = "Yes",
                FoamWrap = "No",
                BubbleSleeve = "Yes",
                UnitsPerBox = 10,
                UnitsPerBag = 5,
                SignedBy = "John Doe",
                AspUserId = "johndoe123",
                SignedOn = DateTime.Now,
                Height = 10.5m,
                Width = 8.2m,
                Length = 12.7m,
                ContainerDiminsion = ContainerDiminsions.Inches,
                UnitsPerContainer = 100,
                NirfFormId = 5,
                Comments = "Fragile items, handle with care"
            },
            new NirfPackaging
            {
                NirfPackagingId = 2,
                BoxSize = "Large",
                Bag = "Paper",
                FoamWrap = "2 inches",
                BubbleSleeve = "1 inch",
                UnitsPerBox = 50,
                UnitsPerBag = 5,
                SignedBy = "Jane Smith",
                AspUserId = "janesmith456",
                SignedOn = new DateTime(2023, 04, 23),
                Height = 40.5m,
                Width = 15.2m,
                Length = 35.7m,
                ContainerDiminsion = ContainerDiminsions.Feet,
                UnitsPerContainer = 500,
                NirfFormId = 3,
                Comments = "Fragile items"
            }
         ];
    }
}