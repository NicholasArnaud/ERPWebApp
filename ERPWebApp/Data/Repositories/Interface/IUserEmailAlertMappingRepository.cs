namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IUserEmailAlertMappingRepository : IRepository<UserEmailAlertMappingRepository>
    {
        Task SaveChangesAsync();
    }
}