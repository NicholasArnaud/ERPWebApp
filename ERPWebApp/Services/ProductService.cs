using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Services
{
    public class ProductService : Service<Product>, IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Product>> GetNonProductionProducts()
        {
           var products = await _unitOfWork.Products.GetNonProductionProducts();
            return products;
        }

        public async Task<List<Product>> GetProductsByMinInventoryAsync(int minInventory)
        {
            return await _unitOfWork.Products.FindAsync(p => p.IsActive && p.StockTotalAvailable <= minInventory);
        }
    }
}