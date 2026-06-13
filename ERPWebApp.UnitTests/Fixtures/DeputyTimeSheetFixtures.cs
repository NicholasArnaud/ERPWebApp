namespace ERPWebApp.UnitTests.Fixtures
{
    public static class DeputyTimeSheetFixtures
    {
        public static List<DeputyTimeSheet> GetTestDeputyTimeSheets() => [
            new DeputyTimeSheet
            {
                DeputyTimeSheetId = 1,
                DeputyId = 123,
                DeputyEmployeeId = 456,
                EmployeeHistory = 789,
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John D",
                Department = "Jewelry",
                StartTime = 900, // 9:00 AM
                StartTimeLocalized = DateTime.Now.Date.AddHours(9), // Today at 9:00 AM
                EndTime = 1700, // 5:00 PM
                EndTimeLocalized = DateTime.Now.Date.AddHours(17), // Today at 5:00 PM
                IsInProgress = true,
                IsDiscarded = false,
                Date = DateTime.Now.Date,
                MealBreak = DateTime.Now.Date.AddMinutes(30), // Today at 12:30 PM
                TotalTime = 7.5f, // 7.5 hours
                TotalTimeInv = 0,
                Created = DateTime.Now.AddDays(-1), // Yesterday
                Modified = DateTime.Now // Today
            },
            new DeputyTimeSheet
            {
                DeputyTimeSheetId = 2,
                DeputyId = 234,
                DeputyEmployeeId = 567,
                EmployeeHistory = 890,
                FirstName = "Jane",
                LastName = "Smith",
                DisplayName = "Jane S",
                Department = "Embroidery",
                StartTime = 830, // 8:30 AM
                StartTimeLocalized = DateTime.Now.Date.AddHours(8).AddMinutes(30), // Today at 8:30 AM
                EndTime = 1730, // 5:30 PM
                EndTimeLocalized = DateTime.Now.Date.AddHours(17).AddMinutes(30), // Today at 5:30 PM
                IsInProgress = true,
                IsDiscarded = false,
                Date = DateTime.Now.Date,
                MealBreak = DateTime.Now.Date.AddMinutes(60), // Today at 1:00 PM
                TotalTime = 8.0f, // 8.0 hours
                TotalTimeInv = 0,
                Created = DateTime.Now.AddDays(-2), // Two days ago
                Modified = DateTime.Now.AddDays(-1) // Yesterday
            }
        ];
    }
}