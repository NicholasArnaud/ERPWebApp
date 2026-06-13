using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class MiscProductRepository : Repository<MiscProduct>, IMiscProductRepository
    {
        public MiscProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<List<MiscProduct>> GetMiscProductsByPurchaseOrderId(int purchaseOrderId)
        {
            var miscItems = await _context.Set<MiscProduct>()
                .Where(mp => mp.PurchaseOrderId == purchaseOrderId && mp.IsActive)
                .ToListAsync();

            return miscItems;
        }
        public async Task DeleteMiscProductAsync(int id, string modifiedByUser)
        {
            var miscProduct = await GetByIdAsync(id);
            if (miscProduct != null)
            {
                miscProduct.IsActive = false;
                miscProduct.ModifyDate = DateTime.UtcNow;
                miscProduct.ModifyByUser = modifiedByUser;

                _context.Set<MiscProduct>().Update(miscProduct);
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateMiscProducts(List<MiscProduct> miscProducts)
        {
            foreach (var item in miscProducts)
            {
                if (item.MiscProductId > 0)
                {
                    var existingItem = await GetByIdAsync(item.MiscProductId);

                    if (existingItem != null)
                    {
                        _context.Entry(existingItem).CurrentValues.SetValues(item);
                        existingItem.ModifyDate = DateTime.UtcNow;
                        existingItem.ModifyByUser = item.ModifyByUser;
                        existingItem.IsActive = true;

                        _context.Set<MiscProduct>().Update(existingItem);
                    }
                }
                else
                {
                    item.ModifyDate = DateTime.UtcNow;
                    item.IsActive = true;

                    await _context.Set<MiscProduct>().AddAsync(item);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
