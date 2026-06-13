using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfParametersFixtures
    {
        public static List<NirfParameters> GetTestList() => [
            new NirfParameters
            {
                NirfParametersId = 1,
                LoopCount = 10,
                Speed = 100,
                Current = 50,
                Frequency = 60,
                SizingX = 500,
                SizingY = 400,
                WhiteLayers = 2,
                ColorLayers = 4,
                SignedBy = "John Doe",
                SignedOn = new DateTime(2023, 4, 24),
                AspUserId = "ASP123",
                NirfFormId = 2,
                Comments = "Test comment",
                UVPTypes = UVPTypes.Printed,
                ThreadTypes = ThreadType.Hex,
                TimeToComplete = new DateTime(2023, 4, 25, 10, 0, 0),
                Temperature = 25,
                IsFahrenheit = false,
                ThreadColor = "Red",
                ThreadHex = "#FF0000",
                ThreadCode = 12.345m,
                FontId = 1,
            },
            new NirfParameters
            {
                NirfParametersId = 2,
                LoopCount = 10,
                Speed = 100,
                Current = 50,
                Frequency = 60,
                SizingX = 200,
                SizingY = 300,
                WhiteLayers = 2,
                ColorLayers = 4,
                SignedBy = "John Smith",
                SignedOn = DateTime.Now,
                AspUserId = "1234",
                NirfFormId = 1,
                Comments = "This is a test comment",
                UVPTypes = UVPTypes.Printed,
                ThreadTypes = ThreadType.Hex,
                TimeToComplete = DateTime.Now.AddDays(5),
                Temperature = 25,
                IsFahrenheit = false,
                ThreadColor = "Red",
                ThreadHex = "#FF0000",
                ThreadCode = 123.456m,
                FontId = 1
            }
         ];
    }
}