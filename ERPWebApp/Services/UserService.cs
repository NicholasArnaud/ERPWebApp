using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
namespace ERPWebApp.Services
{
    public class UserService : IUserService
    {
        IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITriggerEmailAlertService _triggerEmailAlertService;
        private readonly IDepartmentRoleMappingService _roleMappingService;
        private readonly ILogger<UserService> _logger;
        public UserService(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, ITriggerEmailAlertService triggerEmailAlertService, IDepartmentRoleMappingService departmentRoleMapping, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _triggerEmailAlertService = triggerEmailAlertService;
            _roleMappingService = departmentRoleMapping;
            _logger = logger;
        }
        public async Task<List<UserRoleModel>> GetUsersInRole()
        {
            return await _unitOfWork.Users.GetAllUsersInRole();
        }

        public IdentityUser Get(Expression<Func<IdentityUser, bool>> expression = null)
        {
            return _unitOfWork.Users.FilterOne(expression);
        }

        public async Task<List<IdentityUser>> GetList(Expression<Func<IdentityUser, bool>> expression)
        {
            return await _unitOfWork.Users.GetListByFilterAsync(expression);
        }

        public async Task<IdentityResult> CreateUserForEmployee(string companyEmail, int departmentId)
        {
            if (_userManager.Users.Any(x => x.Email == companyEmail))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already exists" });
            }

            string password = GeneratePassword();
            ApplicationUser user = new ApplicationUser();
            int atIndex = companyEmail.IndexOf('@');
            user.UserName = companyEmail.Substring(0, atIndex);
            user.Email = companyEmail;
            user.Password = password;

            var result = await _userManager.CreateAsync(user, user.Password);

            if (result.Succeeded)
            {
                var departmentRoles = await _roleMappingService.GetListAsync(x => x.Department.DepartmentId == departmentId, includes: [x => x.Department, x => x.Role]);

                if (departmentRoles == null || !departmentRoles.Any())
                {
                    _logger.LogError("Department role not found, user save without role mapping");
                }
                else
                {
                    foreach (var departmentRole in departmentRoles)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, departmentRole.Role.Name);
                        if (!roleResult.Succeeded)
                        {
                            var errorDescriptions = roleResult.Errors.Select(x => x.Description).ToList();
                            var errorMessage = string.Join("; ", errorDescriptions);
                            _logger.LogError($"Failed to add user to role {departmentRole.Role.Name}. Errors: {errorMessage}");

                            return IdentityResult.Failed(new IdentityError { Description = errorMessage });
                        }
                    }
                }

                var userEmailData = new UserEmailAlertDTO
                {
                    userName = companyEmail.Substring(0, atIndex),
                    userEmail = companyEmail,
                    password = password,
                };

                try
                {
                    await _triggerEmailAlertService.SendUserCreateEmail(userEmailData);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send email alert for user {message}", ex.Message);
                }

                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Description = result.Errors.Select(x => x.Description).ToString() });
            }
        }
        public string GeneratePassword()
        {
            //char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()".ToCharArray();

            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numberChars = "1234567890";
            const string specialChars = "!@#$%^&*()";

            int length = 10;

            // Create a byte array to hold the random data
            byte[] data = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            result.Append(upperChars[random.Next(upperChars.Length)]);
            result.Append(numberChars[random.Next(numberChars.Length)]);
            result.Append(specialChars[random.Next(specialChars.Length)]);
            result.Append(lowerChars[random.Next(lowerChars.Length)]);

            // Fill the rest of the password length with random characters from all categories
            string allChars = lowerChars + upperChars + numberChars + specialChars;
            for (int i = 4; i < length; i++)
            {
                result.Append(allChars[data[i] % allChars.Length]);
            }
            return new string(result.ToString().OrderBy(c => random.Next()).ToArray());
        }
    }
}