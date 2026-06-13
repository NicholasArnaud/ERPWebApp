using ERPWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPWebApp.Services.IServices
{
    public interface IIntegrationService : IService<Integration>
    {
        Task<List<Integration>> GetIntegrationsWithValidAccessTokenAsync();
    }
}
