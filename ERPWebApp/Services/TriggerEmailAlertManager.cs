using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class TriggerEmailAlertManager : ITriggerEmailAlertManager
    {
        private Dictionary<string, Dictionary<int, bool>> _emailAlerts;

        public TriggerEmailAlertManager()
        {
            _emailAlerts = new Dictionary<string, Dictionary<int, bool>>();
        }

        public bool TryGetValue(string alertType, int itemId, out bool alertSent)
        {
            if (_emailAlerts.TryGetValue(alertType, out var itemAlerts))
            {
                return itemAlerts.TryGetValue(itemId, out alertSent);
            }

            alertSent = false;
            return false;
        }

        public void Update(string alertType, int itemId, bool alertSent)
        {
            if (!_emailAlerts.TryGetValue(alertType, out var itemAlerts))
            {
                itemAlerts = new Dictionary<int, bool>();
                _emailAlerts[alertType] = itemAlerts;
            }

            itemAlerts[itemId] = alertSent;
        }
    }
}
