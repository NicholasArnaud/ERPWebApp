using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class WebhookBatchRepository : Repository<WebHookBatch>, IWebhookBatchRepository
    {
        public WebhookBatchRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
