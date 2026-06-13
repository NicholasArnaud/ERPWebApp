using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ERPWebApp.Controllers.Company;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic)]
[AutoValidateAntiforgeryToken]
public class UserController : Controller
{
    IFilesService _filesService;
    //IUserImageService _userProfileService;
    private IEmployeeService _employeeService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITriggerEmailAlertService _triggerEmailAlertService;
    public UserController(UserManager<IdentityUser> userManager, IEmployeeService employeeService, IFilesService filesService, ITriggerEmailAlertService triggerEmailAlertService) //, IUserImageService userProfileService)
    {
        _userManager = userManager;
        _employeeService = employeeService;
        _filesService = filesService;
        _triggerEmailAlertService = triggerEmailAlertService;
        //_userProfileService = userProfileService;
    }

    public async Task<IActionResult> Index()
    {
        var result = _userManager.Users.ToList();
        if (result == null || result.Count == 0)
        {
            return BadRequest();
        }
        var data = await ToMapData(result);

        return View(data);
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserName,Email,PhoneNumber,Password,ConfirmPassword")] ApplicationUser user, IFormFile profilePic)
    {
        if (ModelState.IsValid)
        {
            if (_userManager.Users.Any(x => x.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email already exists.");
                return View(nameof(Create), user);
            }
            //create regular expression to check format of the given email
            Regex re = new Regex(@"^[a-zA-Z0-9._%+-]+(@completeful.com|@ERP.com)$");
            bool IsERPEmail = re.IsMatch(user.Email);
            if (!IsERPEmail)
            {
                ModelState.AddModelError("Email", "The Email needs to be part of the @ERP.com domain.");
                return View(nameof(Create), user);
            }
            if (!user.Password.Equals(user.ConfirmPassword))
            {
                ModelState.AddModelError("Password", "Password and Confirm password does not match.");
                return View(nameof(Create), user);
            }
            var result = await _userManager.CreateAsync(user, user.Password);

            if (result.Succeeded)
            {
                //var userResult = await _userManager.FindByNameAsync(user.UserName);
                //await AddUserProfilePic(profilePic, userResult);
                var userEmailData = new UserEmailAlertDTO
                {
                    userName = user.UserName,
                    userEmail = user.Email,
                    password = user.ConfirmPassword,
                };

                await _triggerEmailAlertService.SendUserCreateEmail(userEmailData);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors.Select(x => x.Description)));
                return View(nameof(Create), user);
            }
        }
        return View(user);
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null) return NotFound();

        // var query = (IQueryable<UserImage> UserImage) => UserImage
        //.Where(x => x.UserId == existingUser.Id)
        //.Include(x => x.Files);
        // var UserImage = await _userProfileService.GetAsync(query);
        var user = new ApplicationUser
        {
            Id = existingUser.Id,
            UserName = existingUser.UserName,
            Email = existingUser.Email,
            PhoneNumber = existingUser.PhoneNumber,
            EmailConfirmed = existingUser.EmailConfirmed,
            PhoneNumberConfirmed = existingUser.PhoneNumberConfirmed,
            TwoFactorEnabled = existingUser.TwoFactorEnabled,
            LockoutEnd = existingUser.LockoutEnd,
            LockoutEnabled = existingUser.LockoutEnabled,
            AccessFailedCount = existingUser.AccessFailedCount,
            //UserImage = UserImage
        };

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("UserName,Email,PhoneNumber,EmailConfirmed,TwoFactorEnabled,LockoutEnabled")] ApplicationUser user, IFormFile profilePic)
    {
        if (user == null) return NotFound();
        var existingUser = await _userManager.FindByIdAsync(id);

        if (_userManager.Users.Any(x => x.Email == user.Email && x.Id != id))
        {
            ModelState.AddModelError("Email", "This email already exists.");
            return View(nameof(Edit), user);
        }
        //create regular expression to check format of the given email
        Regex re = new Regex(@"^[a-zA-Z0-9._%+-]+(@completeful.com|@ERP.com)$");
        bool IsERPEmail = re.IsMatch(user.Email);
        if (!IsERPEmail && existingUser.Email != user.Email)
        {
            ModelState.AddModelError("Email", "The Email needs to be part of the @ERP.com domain.");
            return View(nameof(Edit), user);
        }
        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.EmailConfirmed = user.EmailConfirmed;
        existingUser.TwoFactorEnabled = user.TwoFactorEnabled;
        existingUser.LockoutEnabled = user.LockoutEnabled;
        var result = await _userManager.UpdateAsync(existingUser);
        if (result.Succeeded)
        {
            // await AddUserProfilePic(profilePic, existingUser);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            ModelState.AddModelError("", result.Errors.Select(x => x.Description).ToString());
            return View(nameof(Edit), user);
        }
    }

    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null) return NotFound();
        return View(await ToMapData(existingUser));
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null) return NotFound();
        return View(await ToMapData(existingUser));
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var existingUser = await _userManager.FindByIdAsync(id);
        var result = await _userManager.DeleteAsync(existingUser);
        var employee = await _employeeService.GetAsync(x => x.ApsuId == id);
        if (employee != null)
        {
            employee.ApsuId = null;
            await _employeeService.UpdateAsync(employee);
        }
        //var UserImage = await _userProfileService.GetAsync(x => x.UserId == id);
        //if (UserImage != null)
        //{
        //    DeleteUserProfilePic(UserImage.UserImageId);
        //}
        return RedirectToAction(nameof(Index));
    }

    //public async Task<IActionResult> UpdateUserProfilePic(int imageId, IFormFile profilePic)
    //{
    //    var imageResult = await _userProfileService.GetAsync(x => x.UserImageId == imageId);
    //    var userResult = await _userManager.FindByIdAsync(imageResult.UserId);
    //    await DeleteUserProfilePic(imageId);
    //    await AddUserProfilePic(profilePic, userResult);
    //    TempData["ToastMessage"] = "Updated successfully.";
    //    return RedirectToAction("Edit", new { id = imageResult.UserId });
    //}

    //public async Task<IActionResult> DeleteImage(int imageId)
    //{
    //    //UserImage image = await DeleteUserProfilePic(imageId);
    //    TempData["ToastMessage"] = "Delete successfully.";
    //    return RedirectToAction("Edit", new { id = image.UserId });
    //}

    private async Task AddUserProfilePic(IFormFile profilePic, IdentityUser userResult)
    {
        if (profilePic is { Length: > 0 })
        {
            var imgUrl = await _filesService.UploadToAzureAsync(profilePic, FileType.Image);

            var thumUrl = await _filesService.UploadThumbnailToAzureAsync(profilePic);
            var image = new Files
            {
                FileName = Path.GetFileName(profilePic.FileName),
                FileType = FileType.Image,
                ContentType = profilePic.ContentType,
                FileUrl = imgUrl
            };

            await _filesService.AddAsync(image);

            //await _userProfileService.AddAsync(new UserImage()
            //{
            //    UserId = userResult.Id,
            //    FileId = image.FileId,
            //    FileUrl = image.FileUrl,
            //    ThumbnailUrl = thumUrl
            //});
        }
    }

    //private async Task<UserImage> DeleteUserProfilePic(int imageId)
    //{
    //    var image = await _userProfileService.GetAsync(x => x.UserImageId == imageId);

    //    if (image != null)
    //    {
    //        await _filesService.RemoveAzureBlobAsync(image.FileUrl, FileType.Image);

    //        if (!String.IsNullOrEmpty(image.ThumbnailUrl))
    //            await _filesService.RemoveAzureBlobAsync(image.ThumbnailUrl, FileType.Thumbnail);

    //        await _userProfileService.RemoveAsync(image.UserImageId);
    //        await _filesService.RemoveAsync(image.FileId);
    //    }

    //    return image;
    //}

    #region Private methods
    private async Task<List<ApplicationUser>> ToMapData(List<IdentityUser> result)
    {
        List<ApplicationUser> applicationUsers = new List<ApplicationUser>();
        foreach (var item in result)
        {

            applicationUsers.Add(new ApplicationUser
            {
                Id = item.Id,
                UserName = item.UserName,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                EmailConfirmed = item.EmailConfirmed,
                PhoneNumberConfirmed = item.PhoneNumberConfirmed,
                TwoFactorEnabled = item.TwoFactorEnabled,
                LockoutEnd = item.LockoutEnd,
                LockoutEnabled = item.LockoutEnabled,
                AccessFailedCount = item.AccessFailedCount,
            });
        }

        return await Task.FromResult(applicationUsers);
    }
    private async Task<ApplicationUser> ToMapData(IdentityUser item)
    {
        var applicationUsers = new ApplicationUser
        {
            Id = item.Id,
            UserName = item.UserName,
            Email = item.Email,
            PhoneNumber = item.PhoneNumber,
            EmailConfirmed = item.EmailConfirmed,
            PhoneNumberConfirmed = item.PhoneNumberConfirmed,
            TwoFactorEnabled = item.TwoFactorEnabled,
            LockoutEnd = item.LockoutEnd ?? new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero),
            LockoutEnabled = item.LockoutEnabled,
            AccessFailedCount = item.AccessFailedCount,
        };

        return await Task.FromResult(applicationUsers);
    }
    #endregion

}
