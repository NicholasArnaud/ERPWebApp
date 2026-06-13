using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class WebhookBatchService : Service<WebHookBatch>, IWebhookBatchService
    {
        public WebhookBatchService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
