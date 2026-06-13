using ERPWebApp.Controllers;
using ERPWebApp.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers
{
    public class SellerOrdersControllerTests
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IOrderShippingService> _orderShippingServiceMock = new();
        private readonly Mock<IWebhooks> _webhooksMock = new();
        private readonly Mock<IShipStationStoreService> _shipStationStoreService = new();
        private readonly SellerOrdersController _controller;

        public SellerOrdersControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new SellerOrdersController( _userManager,
                _webhooksMock.Object,_orderServiceMock.Object, 
                _orderShippingServiceMock.Object, _shipStationStoreService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };
        }

        [Fact]
        public void Index_NullOrder_ReturnsViewWithNewOrder()
        {
            // Arrange
            Order? order = null;

            // Act
            var result = _controller.Index(order);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsType<Order>(viewResult.Model);
        }

        [Fact]
        public void Index_NonNullOrder_ReturnsViewWithOrder()
        {
            // Arrange
            var order = OrderFixtures.GetTestOrders().First();

            // Act
            var result = _controller.Index(order);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(order, viewResult.Model);
        }

        // [Fact]
        // public async Task GetOrderDetailsAsync_WhenOrderStatusShipped_ReturnsViewResultWithTrackingStatus()
        // {
        //     // Arrange
        //     var orderNumber = "1";
        //     var existingOrders = OrderFixtures.GetTestOrders().Where(x => x.orderNumber == orderNumber).ToList();
        //     var getLabelInfo = new ObjectResult(new
        //     {
        //         labels = new[]
        //         {
        //         new
        //         {
        //             tracking_status = "delivered"
        //         }
        //     }
        //     });

        //     _orderShippingServiceMock.Setup(s => s.GetOrderDetails(orderNumber)).ReturnsAsync(existingOrders);
        //     _webhooksMock.Setup(s => s.GetShipEngineOrderLabel("TRACK123")).ReturnsAsync(getLabelInfo);

        //     // Act
        //     var result = await _controller.GetOrderDetailsAsync(orderNumber) as ViewResult;

        //     // Assert
        //     Assert.NotNull(result);
        //     Assert.Equal(nameof(Index), result.ViewName);
        //     Assert.Equal(existingOrders, result.Model);
        //     Assert.Equal("DE", result.ViewData["trackingStatus"]);
        // }
    }
}