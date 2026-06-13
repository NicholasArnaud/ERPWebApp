namespace ERPWebApp.Providers.Interfaces
{
    public interface IUserProvider
    {
        Guid GetCurrentUserId();
        void SetCurrentUserId(Guid userId);
    }
}
