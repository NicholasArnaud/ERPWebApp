using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using System.Linq.Expressions;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "pending")]
    public class DepartmentServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IDepartmentService _departmentService;
        public DepartmentServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _departmentService = new DepartmentService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void UpdateProductDepartments_WithNullSelectedDepartments_ShouldClearDepartments()
        {
            // Arrange
            var selectedDepartments = (string[])null;
            var productToUpdate = new Product();
            _ = _mockUnitOfWork.Setup(static u => u.Departments.GetAll(
                  It.IsAny<Expression<Func<Department, string>>[]>(),
                 It.IsAny<Expression<Func<Department, object>>[]>()
                 )).Returns(
         [
            new() { DepartmentId = 1 },
            new() { DepartmentId = 2 },
            new() { DepartmentId = 3 },
         ]);
            // Act
            var result = _departmentService.UpdateProductDepartments(selectedDepartments, productToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Departments);
        }

        [Fact]
        public void UpdateProductDepartments_WithNoExistingDepartments_ShouldAddSelectedDepartments()
        {
            // Arrange
            var selectedDepartments = new string[] { "1", "2", "3" };
            var productToUpdate = new Product();
            _ = _mockUnitOfWork.Setup(static u => u.Departments.GetAll(
                 It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
                )).Returns(
        [
            new() { DepartmentId = 1 },
            new() { DepartmentId = 2 },
            new() { DepartmentId = 3 },
        ]);

            // Act
            var result = _departmentService.UpdateProductDepartments(selectedDepartments, productToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Departments.Count);
        }

        [Fact]
        public void UpdateProductDepartments_WithExistingDepartments_ShouldAddAndRemoveDepartments()
        {
            // Arrange
            var selectedDepartments = new string[] { "1", "2", "3" };
            var productToUpdate = new Product
            {
                Departments =
            [
                new() { DepartmentId = 2 },
                new() { DepartmentId = 3 },
            ]
            };
            _ = _mockUnitOfWork.Setup(static u => u.Departments.GetAll(
                  It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
                )).Returns(
        [
            new() { DepartmentId = 1 },
            new() { DepartmentId = 2 },
            new() { DepartmentId = 3 },
        ]);

            // Act
            var result = _departmentService.UpdateProductDepartments(selectedDepartments, productToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Departments.Count);
            Assert.Contains(result.Departments, static d => d.DepartmentId == 1);
            Assert.DoesNotContain(result.Departments, static d => d.DepartmentId == 4);
        }
    }
}
