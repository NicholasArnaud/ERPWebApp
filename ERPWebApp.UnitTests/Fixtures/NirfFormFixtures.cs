using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class NirfFormFixtures
    {
        public static List<NirfForm> GetTestList() => [
           new NirfForm
            {
                NirfFormId = 1,
                SellersProductSku = "SKU123",
                Orientation = "Landscape",
                DesignPlacement = "Front",
                DesignAlignment = "Center",
                SpecialInstructions = "No special instructions",
                CreatedBy = "John Smith",
                AspUserId = "ASP456",
                StartedDate = new DateTime(2022, 03, 15),
                CompletedDate = new DateTime(2022, 03, 20),
                NirfStatus = NirfForm.Status.Completed,
                IsLoopCount = true,
                IsSpeed = false,
                IsCurrent = true,
                IsFrequency = false,
                IsSizingX = false,
                IsSizingY = true,
                IsWhiteLayer = true,
                IsColorLayer = true,
                IsUVPType = false,
                IsTemperature = false,
                IsFont = true,
                IsThreadColor = false
            },
            new NirfForm
            {
                NirfFormId = 2,
                SellersProductSku = "SKU234",
                Orientation = "Portrait",
                DesignPlacement = "Back",
                DesignAlignment = "Bottom",
                SpecialInstructions = "Add logo",
                CreatedBy = "Jane Doe",
                AspUserId = "ASP789",
                StartedDate = new DateTime(2022, 04, 01),
                CompletedDate = new DateTime(2022, 04, 05),
                NirfStatus = NirfForm.Status.InProgress,
                IsLoopCount = false,
                IsSpeed = true,
                IsCurrent = false,
                IsFrequency = true,
                IsSizingX = true,
                IsSizingY = false,
                IsWhiteLayer = false,
                IsColorLayer = true,
                IsUVPType = true,
                IsTemperature = true,
                IsFont = false,
                IsThreadColor = true
            }
        ];
    }
}