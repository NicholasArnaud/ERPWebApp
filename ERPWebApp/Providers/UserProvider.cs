using ERPWebApp.Providers.Interfaces;

namespace ERPWebApp.Providers
{
    public class UserProvider : IUserProvider
    {
        private Guid _userId { get; set; }
        public Guid GetCurrentUserId()
        {
            return _userId;
        }

        public void SetCurrentUserId(Guid userId)
        {
            this._userId = userId;
        }
    }
}
