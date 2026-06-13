using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "execute")]
    public class AuditLogControllerTests
    {
        private readonly Mock<IAuditLogService> _auditLogService = new();
        private readonly AuditLogController _controller;
        public AuditLogControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new AuditLogController(_auditLogService.Object)
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
        public async Task When_AuditLogTableHasData_ReturnsAViewResult()
        {
            // Arrange
            var auditLogServiceMock = new Mock<IAuditLogService>();
            var controller = new AuditLogController(auditLogServiceMock.Object);

            var requestData = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "draw", "1" },
                { "start", "0" },
                { "length", "10" },
                { "order[0][column]", "0" },
                { "order[0][dir]", "desc" },
                { "search[value]", "searchValue" }
            });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = requestData } }
            };

            var auditLogs = new List<AuditLogDTO> {
                new() { User = "User1", Timestamp = DateTime.Now, BusinessEntity = "Entity1", PropertyName = "Property1", OldValue = "OldValue1", NewValue = "NewValue1" },
                new() { User = "User2", Timestamp = DateTime.Now, BusinessEntity = "Entity2", PropertyName = "Property2", OldValue = "OldValue2", NewValue = "NewValue2" },
            };
            _ = auditLogServiceMock.Setup(static s => s.GetCountAsync(null)).ReturnsAsync(auditLogs.Count);
            _ = auditLogServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Func<IQueryable<AuditLog>, IQueryable<AuditLogDTO>>>())).ReturnsAsync(auditLogs);

            var jsonData = new
            {
                draw = "1",
                recordsFiltered = auditLogs.Count,
                recordsTotal = auditLogs.Count,
                data = auditLogs
            };

            // Act
            var result = await controller.GetAuditLogs() as ActionResult;

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expectedJson = JsonConvert.SerializeObject(jsonData);
            var actualJson = JsonConvert.SerializeObject(okResult.Value);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
