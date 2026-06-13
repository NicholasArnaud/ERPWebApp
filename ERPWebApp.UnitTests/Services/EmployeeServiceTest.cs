namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class EmployeeServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IEmployeeService _employeeService;

        public EmployeeServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _employeeService = new EmployeeService(_mockUnitOfWork.Object);
        }

    }
}