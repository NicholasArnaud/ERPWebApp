using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetNonProductionProducts()
        {
            var products = await _context.Product
            .Where(p => p.Departments.Any(d => d.IsProduction == false)) 
            .ToListAsync();

            return products;

        }
    }
}