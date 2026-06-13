namespace ERPWebApp.UnitTests.Fixtures
{
    public class SpeedOMeterGoalFixtures
    {
        public static List<SpeedOMeterGoal> GetTestSpeedOMeterGoal() =>
        [
            new SpeedOMeterGoal()
            {
                SpeedOMeterGoalId = 1,
                ElectroplatingGoal = 10,
                EmbroideryGoal = 15,
                EngravingGoal = 20,
                MetalGoal = 25,
                UVGoal = 30,
                ModifyDate = DateTime.Now,
                ModifyByUser = "John Doe"
            },
            new SpeedOMeterGoal()
            {
                SpeedOMeterGoalId = 2,
                ElectroplatingGoal = 20,
                EmbroideryGoal = 25,
                EngravingGoal = 30,
                MetalGoal = 20,
                UVGoal = 40,
                ModifyDate = DateTime.Now,
                ModifyByUser = "Stefano"
            }
        ];
    }
}