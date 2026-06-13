using ERPWebApp.Models.Orders;
using System.Linq.Expressions;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Services.IServices
{
    public interface IOrderFulfillmentService : IService<OrderFulfillment>
    {
        Task<OrderFulfillment> GetFulfillmentAsync(
            Expression<Func<OrderFulfillment, bool>> expression,
            params Expression<Func<OrderFulfillment, object>>[] includes
        );
        Task<List<OrderFulfillment>> GetFulfillmentListAsync(
            Expression<Func<OrderFulfillment, bool>> expression,
            Expression<Func<OrderFulfillment, string>>[] orderSelectors = null,
            params Expression<Func<OrderFulfillment, object>>[] includes
        );

        List<OrderFulfillment> GetFulfillmentList(
           Expression<Func<OrderFulfillment, bool>> expression,
           Expression<Func<OrderFulfillment, string>>[] orderSelectors = null,
           Expression<Func<OrderFulfillment, object>>[] includes = null
       );
        void OnUpdateFulfillment(OrderFulfillment orderFulfillment);
        void OnBulkUpdateFulfillments(List<OrderFulfillment> orderFulfillmentList);
        Task<Order> VoidAsync(int orderId, OrderFulfillment row);
    }
}