using ERPWebApp.Data.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories;

public class OrderItemRepository(ApplicationDbContext context) : Repository<OrderItem>(context), IOrderItemRepository
{
    public async Task<OrderItem> GetLastOrderItem()
    {
        return await _context.OrderItem.OrderByDescending(x => x.orderItemId).Take(1).FirstAsync();
    }
}