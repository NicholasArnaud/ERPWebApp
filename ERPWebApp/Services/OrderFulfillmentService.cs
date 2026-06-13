using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using System.Linq.Expressions;

namespace ERPWebApp.Services
{
    public class OrderFulfillmentService : Service<OrderFulfillment>, IOrderFulfillmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IWebhooks _webhooks;
        private readonly DateTime _now;
        private readonly ILogger<OrderShippingService> _logger;
        public OrderFulfillmentService(IUnitOfWork unitOfWork,
            IOrderService orderService,
            IWebhooks webhooks,
            ILogger<OrderShippingService> logger) : base(unitOfWork)
        {
            _webhooks = webhooks;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _now = TimeZoneInfo.ConvertTime(
                DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            );
        }
        public async Task<OrderFulfillment> GetFulfillmentAsync(Expression<Func<OrderFulfillment, bool>> expression, params Expression<Func<OrderFulfillment, object>>[] includes)
        {
            return await _unitOfWork.OrderFulfillments.FilterOneAsync(expression, includes);
        }

        public async Task<List<OrderFulfillment>> GetFulfillmentListAsync(Expression<Func<OrderFulfillment, bool>> expression, Expression<Func<OrderFulfillment, string>>[] orderSelectors = null, params Expression<Func<OrderFulfillment, object>>[] includes)
        {
            return await _unitOfWork.OrderFulfillments.GetListByFilterAsync(expression, orderSelectors, includes);
        }

        public List<OrderFulfillment> GetFulfillmentList(Expression<Func<OrderFulfillment, bool>> expression, Expression<Func<OrderFulfillment, string>>[] orderSelectors = null, Expression<Func<OrderFulfillment, object>>[] includes = null)
        {
            return _unitOfWork.OrderFulfillments.GetListByFilter(expression, orderSelectors, includes);
        }

        public void OnUpdateFulfillment(OrderFulfillment orderFulfillment)
        {
            _unitOfWork.OrderFulfillments.Update(orderFulfillment);
            _unitOfWork.SaveChanges();
        }

        public void OnBulkUpdateFulfillments(List<OrderFulfillment> orderFulfillmentList)
        {
            _unitOfWork.OrderFulfillments.UpdateRange(orderFulfillmentList);
            _unitOfWork.SaveChanges();
        }
        public async Task<Order> VoidAsync(int orderId, OrderFulfillment row)
        {
            try
            {
                ShipEngineLabel label = null;
                ShipEngineVoidMessage voidResult = null;
                label = await _orderService.GetShipEngineOrderLabel(row.trackingNumber);
                if (label != null)
                {
                    voidResult = await _orderService.VoidFulfillmentLabel(label.LabelId);
                }

                if (label == null || voidResult.Approved)
                {
                    row.voided = true;
                    row.voidDate = _now;

                    _unitOfWork.OrderFulfillments.Update(row);
                    _ = await _unitOfWork.SaveChangesAsync();
                    return await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(orderId);
                }
                else
                    _logger.LogError("Error occurred while voiding fulfillment label.");
                return await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(orderId);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //_logger.LogInformation("Fulfillment not found for voiding. OrderId: {orderId}, FulfillmentId: {fulfillmentId}", orderId, row.OrderFulfillmentId);
                    row.voided = true;
                    row.voidDate = _now;

                    _unitOfWork.OrderFulfillments.Update(row);
                    _ = await _unitOfWork.SaveChangesAsync();
                    return await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(orderId);
                }
                _logger.LogError(ex, "Error occurred while voiding fulfillment.");
                throw;
            }
        }
    }
}