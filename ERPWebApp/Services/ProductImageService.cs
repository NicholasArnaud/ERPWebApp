using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductImageService : Service<ProductImage>, IProductImageService
    {
        public ProductImageService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}