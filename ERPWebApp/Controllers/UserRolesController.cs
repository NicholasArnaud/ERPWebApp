using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.RoleManager)]
[AutoValidateAntiforgeryToken]
public class UserRolesController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRolesController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;

    }
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.Users.OrderBy(m => m.UserName).FirstOrDefault()?.Id;
        List<SelectListItem> items = new List<SelectListItem>();
        var tempTest = new MultiSelectList(_roleManager.Roles, "Id", "Name");
        var Roles = await _roleManager.Roles.ToListAsync();

        //get the values for the select2
        foreach (var role in tempTest)
        {
            role.Value = role.Text;
            items.Add(role);
        }
        Dictionary<string, string> UserRoles = new Dictionary<string, string>();
        //get each userid rolename pairs
        foreach (IdentityRole role in Roles)
        {
            string roleName = role.Name;
            var UsersInRoleList = (await _userManager.GetUsersInRoleAsync(roleName));
            foreach (IdentityUser user in UsersInRoleList)
            {
                if (UserRoles.ContainsKey(user.Id))
                {
                    UserRoles[user.Id] = UserRoles[user.Id] + "," + roleName;
                }
                else
                {
                    UserRoles.Add(user.Id, roleName);
                }
            }
        }
        var currentUser = User.Identity.Name;
        ViewData["currentUser"] = currentUser;
        var selectUser = await _userManager.FindByIdAsync(userId);
        var users = await _userManager.Users.OrderBy(m => m.UserName).ToListAsync();
        var userRolesViewModel = new UserViewModel();
        var userRoleslViewModel = new List<UserRolesViewModel>();
        //set the user information
        foreach (IdentityUser user in users)
        {
            var thisViewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            try
            {
                thisViewModel.Role = UserRoles[user.Id];
            }
            catch
            {
                thisViewModel.Role = null;
            }

            userRoleslViewModel.Add(thisViewModel);


        }
        userRolesViewModel.User = userRoleslViewModel;
        userRolesViewModel.Role = Roles.OrderBy(x => x.Name).ToList();
        ViewData["Department"] = items.OrderBy(x => x.Text);
        return View(userRolesViewModel);
    }
    /*userviewmodel - model for the user role page contains a list of roles and a table of users*/
    public class UserViewModel
    {
        public IEnumerable<UserRolesViewModel> User { get; set; }
        public List<IdentityRole> Role { get; set; }
    }
    /* public async Task<IActionResult> ManageUserRoles(string userId)
     {
         ViewBag.userId = userId;
         var user = await _userManager.FindByIdAsync(userId);
         if (user == null)
         {
             ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
             return View("NotFound");
         }
         ViewBag.UserName = user.UserName;
         var model = new List<ManageUserRolesViewModel>();
         var userRoles = await _roleManager.Roles.ToListAsync();
         foreach (var role in userRoles)
         {
             var userRolesViewModel = new ManageUserRolesViewModel
             {
                 RoleId = role.Id,
                 RoleName = role.Name
             };
             var checkRole = await _userManager.IsInRoleAsync(user, role.Name);
             if (checkRole)

             {
                 userRolesViewModel.Selected = true;
             }
             else
             {
                 userRolesViewModel.Selected = false;
             }
             model.Add(userRolesViewModel);
         }
         return View();
     }*/
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.RoleManager)]
    [HttpPost]
    public async Task<IActionResult> ManageUserRoles(string filterStringHelper, string getuserid)
    {
        List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();
        var userRoles = await _roleManager.Roles.ToListAsync();
        var user = await _userManager.FindByNameAsync(getuserid);
        var filterstringarray = filterStringHelper.Split(",");
        if (user == null)
        {
            return View();
        }
        foreach (var role in userRoles)
        {
            var userRolesViewModel = new ManageUserRolesViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            if (filterStringHelper.Contains(role.Name))
            {
                foreach (var rolenames in filterstringarray)
                {
                    if (rolenames.Equals(role.Name.ToString()))
                    {
                        userRolesViewModel.Selected = true;
                    }
                }

            }
            else
            {
                userRolesViewModel.Selected = false;
            }
            model.Add(userRolesViewModel);
        }
        var roles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
        {

            ModelState.AddModelError("", "Cannot remove user existing roles");
            return View();
        }
        result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName).ToList());
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot add selected roles to user");
            return View();
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.RoleManager)]
    public async Task<IActionResult> Create(String myRoleName)
    {
        if (myRoleName != null)
        {
            var identityRoles = new IdentityRole();
            identityRoles.Name = myRoleName;
            identityRoles.NormalizedName = myRoleName.ToUpper();
            await _roleManager.CreateAsync(identityRoles);
        }
        return RedirectToAction("Index");
    }
}

