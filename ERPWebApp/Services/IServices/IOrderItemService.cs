using ERPWebApp.Data.DTOModels;
using static ERPWebApp.Data.DTOModels.ZazzleDTO.ZazzleRequest.Response.Result;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Services.IServices;

public interface IOrderItemService : IService<OrderItem>
{
    public OrderItem ConvertShopifyItemToOrderItem(LineItem li);
    public OrderItem ConvertZazzleItemToOrderItem(ZazzleOrder.ZazzleLineItem zazzleLineItem);
    public Task<OrderItem> ConvertAttributeToProductSku(OrderItem orderItem);
    public Task<List<OrderItem>> AssignProductIds(List<OrderItem> items);
    public Task<List<OrderItem>> CustomProductSkuConversion(OrderItem orderItem);
}
