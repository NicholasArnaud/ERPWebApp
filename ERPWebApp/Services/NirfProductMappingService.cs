using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Services
{
    public class NirfProductMappingService : Service<NirfProductMapping>, INirfProductMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NirfProductMappingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Product>> GetVariantProducts(int nirfFormId)
        {
            var products = await _unitOfWork.NirfProductMapping.GetListByQueryAsync(
                  (q) => q.Where(x => x.NirfFormId == nirfFormId)
                          .Include(x => x.Product)
                          .Select(x => new Product
                          {
                              ProductId = x.ProductId.Value,
                              Sku = x.Product.Sku,
                              Description = x.Product.Description
                          })
              );

            return products;
        }
    }
}