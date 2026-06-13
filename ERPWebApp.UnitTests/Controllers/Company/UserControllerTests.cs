using ERPWebApp.Controllers.Company;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Company
{
    [Trait("Category", "execute")]
    public class UserControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
        private readonly Mock<IEmployeeService> _employeeServiceMock = new();
        private readonly Mock<IFilesService> _filesServiceMock = new();
        private readonly Mock<ITriggerEmailAlertService> _triggerEmailAlertService = new();

        //private readonly Mock<IUserImageService> _userProfileServiceMock = new Mock<IUserImageService>();
        private UserController _controller;
        public UserControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new UserController(
                _userManagerMock.Object,
                _employeeServiceMock.Object,
                _filesServiceMock.Object, //,
                _triggerEmailAlertService.Object
               // _userProfileServiceMock.Object
                )
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
        public async Task Index_ReturnsViewWithValidData()
        {
            // Arrange
            var users = new List<IdentityUser> { new() { Id = "1", UserName = "user1" } };
            _ = _userManagerMock.Setup(static um => um.Users).Returns(users.AsQueryable());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<ApplicationUser>>(viewResult.Model);

            Assert.Equal(users.Count, model.Count);
            Assert.Equal(users[0].UserName, model.First().UserName);
        }

        [Fact]
        public async Task Index_ReturnsBadRequestWhenNoUsers()
        {
            // Arrange
            var users = new List<IdentityUser>();
            _ = _userManagerMock.Setup(static um => um.Users).Returns(users.AsQueryable());


            // Act
            var result = await _controller.Index();

            // Assert
            _ = Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@completeful.com",
                PhoneNumber = "1234567890",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            };
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(),
                                                        It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");

            // Act
            var result = await _controller.Create(user, file.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Create_WithInvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var user = new ApplicationUser();
            _controller.ModelState.AddModelError("", "Model error");

            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(),
                                                        It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");

            // Act
            var result = await _controller.Create(user, file.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);

            var modelStateEntry = Assert.Single(viewResult.ViewData.ModelState);
            Assert.NotNull(modelStateEntry.Value);
            var error = Assert.Single(modelStateEntry.Value.Errors);
            Assert.Equal("Model error", error.ErrorMessage);
        }

        [Fact]
        public async Task Create_WithMismatchedPasswords_ReturnsRedirectToCreate()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@completeful.com",
                PhoneNumber = "1234567890",
                Password = "password",
                ConfirmPassword = "mismatchedpassword"
            };
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(),
                                                        It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");
            // Act
            var result = await _controller.Create(user, file.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Create", redirectToActionResult.ViewName);
        }

        [Fact]
        public async Task Create_WithUserCreationFailure_ReturnsRedirectToCreate()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                PhoneNumber = "1234567890",
                Password = "password",
                ConfirmPassword = "password"
            };
            _ = _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(),
                                                       It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");

            // Act
            var result = await _controller.Create(user, file.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<ViewResult>(result);
            //Assert.Equal("Create", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewWithUser()
        {
            // Arrange
            var existingUser = new ApplicationUser { Id = "1", UserName = "existinguser" };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(existingUser.Id)).ReturnsAsync(existingUser);


            // Act
            var result = await _controller.Edit(existingUser.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ApplicationUser>(viewResult.Model);
            Assert.Equal(existingUser.Id, model.Id);
            Assert.Equal(existingUser.UserName, model.UserName);
            Assert.Equal(existingUser.Email, model.Email);
            Assert.Equal(existingUser.PhoneNumber, model.PhoneNumber);
            Assert.Equal(existingUser.EmailConfirmed, model.EmailConfirmed);
            Assert.Equal(existingUser.PhoneNumberConfirmed, model.PhoneNumberConfirmed);
            Assert.Equal(existingUser.TwoFactorEnabled, model.TwoFactorEnabled);
            Assert.Equal(existingUser.LockoutEnd, model.LockoutEnd);
            Assert.Equal(existingUser.LockoutEnabled, model.LockoutEnabled);
            Assert.Equal(existingUser.AccessFailedCount, model.AccessFailedCount);
        }

        [Fact]
        public async Task Edit_WithNullId_ReturnsNotFound()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>();

            // Act
            var result = await _controller.Edit(null);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithNonexistentId_ReturnsNotFound()
        {
            // Arrange
            var nonexistentId = "nonexistentid";
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(nonexistentId)).ReturnsAsync((ApplicationUser?)null);


            // Act
            var result = await _controller.Edit(nonexistentId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidUser_ReturnsRedirectToIndex()
        {
            // Arrange
            var id = "1";
            var existingUser = new ApplicationUser { Id = id };
            var user = new ApplicationUser { Id = id, UserName = "updatedemail", Email = "updatedemail@completeful.com", PhoneNumber = "1234567890", EmailConfirmed = true, TwoFactorEnabled = true, LockoutEnabled = true };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(existingUser);
            _ = _userManagerMock.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Success);

            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");
            // Act
            var result = await _controller.Edit(id, user, file.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WithNullUser_ReturnsNotFound()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");
            // Act
            var result = await _controller.Edit("1", null, file.Object);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithUserUpdateFailure_ReturnsRedirectToEdit()
        {
            // Arrange
            var id = "1";
            var existingUser = new IdentityUser { Id = id };
            var user = new ApplicationUser { Id = id, UserName = "updatedemail", Email = "updatedemail@example.com", PhoneNumber = "1234567890", EmailConfirmed = true, TwoFactorEnabled = true, LockoutEnabled = true };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(existingUser);
            _ = _userManagerMock.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Failed());
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
              .Verifiable();
            var files = FilesFixtures.GetTestFiles();

            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            //_userProfileServiceMock.Setup(x => x.AddAsync(It.IsAny<UserImage>())).ReturnsAsync(new UserImage());
            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");

            // Act
            var result = await _controller.Edit(id, user, file.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ViewName);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithUser()
        {
            // Arrange
            var id = "1";
            var existingUser = new IdentityUser { Id = id };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(existingUser);


            // Act
            var result = await _controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ApplicationUser>(viewResult.Model);
            _ = model.Should().NotBeNull();
        }

        [Fact]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            // Arrange

            // Act
            var result = await _controller.Details(null);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WithNonexistentId_ReturnsNotFound()
        {
            // Arrange
            var nonexistentId = "nonexistentid";
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(nonexistentId)).ReturnsAsync((ApplicationUser?)null);


            // Act
            var result = await _controller.Details(nonexistentId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewWithUser()
        {
            // Arrange
            var id = "1";
            var existingUser = new ApplicationUser { Id = id };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(existingUser);


            // Act
            var result = await _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ApplicationUser>(viewResult.Model);
            _ = model.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFound()
        {
            // Arrange

            // Act
            var result = await _controller.Delete(null);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithNonexistentId_ReturnsNotFound()
        {
            // Arrange
            var nonexistentId = "nonexistentid";
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(nonexistentId)).ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _controller.Delete(nonexistentId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ReturnsRedirectToIndex()
        {
            // Arrange
            var id = "1";
            var existingUser = new IdentityUser { Id = id };
            _ = _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(existingUser);
            _ = _userManagerMock.Setup(um => um.DeleteAsync(existingUser)).ReturnsAsync(IdentityResult.Success);
            var employee = new Employee { ApsuId = id };
            _ = _employeeServiceMock.Setup(es => es.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(),
                                                         It.IsAny<Expression<Func<Employee, object>>[]>())).ReturnsAsync(employee);


            // Act
            var result = await _controller.DeleteConfirmed(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }


    }
}
