using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IOrderItemRepository: IRepository<OrderItem>
    {
        public Task<OrderItem> GetLastOrderItem();
    }
}