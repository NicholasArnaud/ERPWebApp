namespace ERPWebApp.Models.Company.Security
{
    public class SecurityAccess
    {
        public List<AccessPlan> AccessPlans { get; set; }
        public List<AccessPoint> AccessPoints { get; set; }
        public List<AccessCard> AccessCards { get; set; }
        public List<AccessPlanDoor> AccessPlanDoors { get; set; }
        public List<AccessPlanUser> AccessPlanUsers { get; set; }
    }
}
