using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class NirfForecastingFixtures
    {
        public static List<NirfForecasting> GetTestList() => [
            new NirfForecasting
            {
                NirfForecastingId = 1,
                LeadTime = 10,
                MinMaxLevel = "10-50",
                Count = "20",
                SignedBy = "John Doe",
                SignedOn = DateTime.Now,
                AspUserId = "asp123",
                NirfFormId = 1,
                NirfForm = NirfFormFixtures.GetTestList().First(),
                Comments = "This is a test comment"
            },
            new NirfForecasting
            {
                NirfForecastingId = 1,
                LeadTime = 30,
                MinMaxLevel = "50-100",
                Count = "100",
                SignedBy = "John Smith",
                SignedOn = DateTime.Now,
                AspUserId = "ASP123",
                NirfFormId = 1,
                NirfForm = NirfFormFixtures.GetTestList().Last(),
                Comments = "Test comments"
            }
         ];
    }
}