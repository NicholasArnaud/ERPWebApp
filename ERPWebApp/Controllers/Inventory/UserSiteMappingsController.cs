using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Controllers.Inventory;

[Authorize(
    Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.InventoryManager
            + ","
            + RoleList.HRManager
            + ","
            + RoleList.HRBasic
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class UserSiteMappingsController(
    IUserSiteMappingService userSiteMappingService,
    ISiteService siteService,
    IUserService userService
) : Controller
{
    // GET: UserSiteMappings
    public async Task<IActionResult> Index()
    {
        var userSiteMappings = await userSiteMappingService.GetAllAsync(null, [x => x.IdentityUser, x => x.Site]);
        return View(userSiteMappings);
    }

    // GET: UserSiteMappings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userSiteMapping = await userSiteMappingService.GetAsync(x => x.UserSiteMappingId == id, includes: [x => x.IdentityUser, x => x.Site]);
        if (userSiteMapping == null)
        {
            return NotFound();
        }

        return View(userSiteMapping);
    }

    // GET: UserSiteMappings/Create
    public async Task<IActionResult> Create()
    {
        var userList = await userService.GetList(x => x.EmailConfirmed == true);
        ViewData["UserId"] = new SelectList(userList, "Id", "UserName");
        await GetSiteListAsync(null);
        return View();
    }

    private async Task GetSiteListAsync(int? selectedId)
    {
       var sites= await siteService.GetListAsync(
            (IQueryable<Site> p) => p
                .Where(x => x.IsActive == true)
                .Select(x => new SelectListItem { Value = x.SiteId.ToString(), Text = x.SiteName })
        );
        ViewData["SiteId"] = new SelectList(sites, "Value", "Text", selectedId);
    }

    // POST: UserSiteMappings/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("UserSiteMappingId,UserId,SiteId")] UserSiteMapping userSiteMapping)
    {
        if (userSiteMapping.SiteId == 0)
            ModelState.AddModelError(nameof(UserSiteMapping.SiteId), "The Site field is required.");
        try
        {
            if (ModelState.IsValid)
            {
                await userSiteMappingService.AddAsync(userSiteMapping);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to create mapping. User and Site combination already exists.");
        }
        var userList = await userService.GetList(x => x.EmailConfirmed == true);
        ViewData["UserId"] = new SelectList(userList, "Id", "UserName");
        await GetSiteListAsync(null);
        return View(userSiteMapping);
    }

    // GET: UserSiteMappings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var userSiteMapping = await userSiteMappingService.GetAsync(x => x.UserSiteMappingId == id, includes: [x => x.IdentityUser, x => x.Site]);
        if (userSiteMapping == null)
        {
            return NotFound();
        }
        var userList = await userService.GetList(x => x.EmailConfirmed == true);
        ViewData["UserId"] = new SelectList(userList, "Id", "UserName");
        await GetSiteListAsync(null);
        return View(userSiteMapping);
    }

    // POST: UserSiteMappings/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("UserSiteMappingId,UserId,SiteId")] UserSiteMapping userSiteMapping)
    {
        if (id != userSiteMapping.UserSiteMappingId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await userSiteMappingService.UpdateAsync(userSiteMapping);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserSiteMappingExists(userSiteMapping.UserSiteMappingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        var userList = await userService.GetList(x => x.EmailConfirmed == true);
        ViewData["UserId"] = new SelectList(userList, "Id", "UserName");
        await GetSiteListAsync(null);
        return View(userSiteMapping);
    }

    // GET: UserSiteMappings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var userSiteMapping = await userSiteMappingService.GetAsync(x => x.UserSiteMappingId == id, includes: [x => x.IdentityUser, x => x.Site]);
        if (userSiteMapping == null)
        {
            return NotFound();
        }

        return View(userSiteMapping);
    }

    // POST: UserSiteMappings/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userSiteMapping = await userSiteMappingService.GetAsync(x => x.UserSiteMappingId == id);
        if (userSiteMapping != null)
        {
            await userSiteMappingService.RemoveAsync(id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool UserSiteMappingExists(int id)
    {
        return userSiteMappingService.Any(x => x.UserSiteMappingId == id);
    }
}