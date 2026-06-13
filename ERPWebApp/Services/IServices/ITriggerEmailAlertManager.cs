namespace ERPWebApp.Services.IServices
{
    public interface ITriggerEmailAlertManager
    {
        bool TryGetValue(string alertType, int itemId, out bool alertSent);
        void Update(string alertType, int itemId, bool alertSent);
    }
}
