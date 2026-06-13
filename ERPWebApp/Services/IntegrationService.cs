using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ERPWebApp.Services
{
    public class IntegrationService : Service<Integration>, IIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IntegrationService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Integration>> GetIntegrationsWithValidAccessTokenAsync()
        {
            return await _unitOfWork.Integrations.GetListByFilterAsync(i => !string.IsNullOrEmpty(i.AccessToken));
        }
    }
}
