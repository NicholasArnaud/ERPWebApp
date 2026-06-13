using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ERPWebApp.Controllers.Company;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class DepartmentRoleMappingController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IDepartmentService _departmentService;
    private readonly IDepartmentRoleMappingService _departmentRoleMappingService;

    public DepartmentRoleMappingController(RoleManager<IdentityRole> roleManager, IDepartmentService departmentService, IDepartmentRoleMappingService departmentRoleMappingService)
    {
        _roleManager = roleManager;
        _departmentService = departmentService;
        _departmentRoleMappingService = departmentRoleMappingService;
    }

    public async Task<IActionResult> Index()
    {
        var departmentRoleMapping = await _departmentRoleMappingService.GetAllAsync(null, [x => x.Department, x => x.Role]);


        var departmentRoles = departmentRoleMapping
        .GroupBy(dr => dr.DepartmentId) // Group by DepartmentId
        .Select(group => new DepartmentRoleMappingViewModel
        {
            DepartmentId = group.Key, 
            DepartmentName = group.FirstOrDefault().Department?.DepartmentName,
            UserRoleIds = group.Select(dr => dr.Role.Name).ToList()
        }).ToList();

        return View(departmentRoles);
    }

    public async Task<IActionResult> Details(int? id)
    {

        var departmentRoleMappings = await _departmentRoleMappingService.GetListAsync(
        x => x.DepartmentId == id, 
        includes: new Expression<Func<DepartmentRoleMapping, object>>[]
        {
            x => x.Department, 
            x => x.Role 
        }
        );

        if (departmentRoleMappings == null || !departmentRoleMappings.Any())
        {
            return NotFound();
        }
        var result = new DepartmentRoleMappingViewModel
        {
            DepartmentId = id ?? 0,
            DepartmentName = departmentRoleMappings.First().Department.DepartmentName,
            Roles = departmentRoleMappings.Select(drm => drm.Role).Distinct().ToList()
        };

        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var departments = await _departmentService.GetAllAsync();
        ViewData["DepartmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
        ViewData["UserRoleId"] = new SelectList(roles, "Id", "Name");
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("DepartmentRoleId,DepartmentId,UserRoleIds")] DepartmentRoleMappingViewModel departmentRoleMapping)
    {
        if (departmentRoleMapping.DepartmentId == 0)
            ModelState.AddModelError(nameof(DepartmentRoleMapping.DepartmentId), "The Department field is required.");
        if (departmentRoleMapping.UserRoleIds == null || !departmentRoleMapping.UserRoleIds.Any())
            ModelState.AddModelError(nameof(DepartmentRoleMappingViewModel.UserRoleIds), "The Default role field is required.");

        var isExist = await _departmentRoleMappingService.IsExistsAsync(x => x.DepartmentId == departmentRoleMapping.DepartmentId);

        if (isExist)
        {
            ModelState.AddModelError(string.Empty, "Unable to create mapping. Department already have default user role.");
        }
        else
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save each selected role mapping
                    foreach (var roleId in departmentRoleMapping.UserRoleIds)
                    {
                        var departmentRoleMappingModel = new DepartmentRoleMapping
                        {
                            DepartmentId = departmentRoleMapping.DepartmentId,
                            UserRoleId = roleId
                        };

                        // Add the mapping to the database
                        await _departmentRoleMappingService.AddAsync(departmentRoleMappingModel);
                        
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Unable to create mapping." + ex);
                }
            }
        }
        var roles = _roleManager.Roles.ToList();
        var departments = await _departmentService.GetAllAsync();
        ViewData["DepartmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
        ViewData["UserRoleId"] = new SelectList(roles, "Id", "Name");
        return View(departmentRoleMapping);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var departmentRoleMappings = await _departmentRoleMappingService.GetListAsync(
       x => x.DepartmentId == id, 
       includes: new Expression<Func<DepartmentRoleMapping, object>>[]
       {
            x => x.Department, 
            x => x.Role 
       }
       );
        if (departmentRoleMappings == null)
        {
            return NotFound();
        }
        var result = new DepartmentRoleMappingViewModel
        {
            DepartmentId = departmentRoleMappings.First().Department.DepartmentId,
            DepartmentName = departmentRoleMappings.First().Department.DepartmentName,
            Roles = departmentRoleMappings.Select(drm => drm.Role).Distinct().ToList(),
            UserRoleIds = departmentRoleMappings.Select(drm => drm.Role.Id).Distinct().ToList()
        };

        var roles = _roleManager.Roles.ToList();
        var departments = await _departmentService.GetAllAsync();
        ViewData["DepartmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
        ViewData["UserRoleId"] = new SelectList(roles, "Id", "Name");
        return View(result);
    }

    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentRoleId,DepartmentId,UserRoleIds")] DepartmentRoleMappingViewModel departmentRoleMapping)
    {

        if (ModelState.IsValid)
        {
            var departmentExists = await _departmentService.IsExistsAsync(x => x.DepartmentId == departmentRoleMapping.DepartmentId);
            if (!departmentExists)
            {
                return NotFound();
            }

            try
            {
                var existingMappings = await _departmentRoleMappingService.GetListAsync(
                x => x.DepartmentId == departmentRoleMapping.DepartmentId);

                foreach (var mapping in existingMappings)
                {
                    await _departmentRoleMappingService.RemoveAsync(mapping.DepartmentRoleId);
                }

                if (departmentRoleMapping.UserRoleIds != null && departmentRoleMapping.UserRoleIds.Any())
                {
                    foreach (var roleId in departmentRoleMapping.UserRoleIds)
                    {
                        var newMapping = new DepartmentRoleMapping
                        {
                            DepartmentId = departmentRoleMapping.DepartmentId,
                            UserRoleId = roleId
                        };
                        await _departmentRoleMappingService.AddAsync(newMapping);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to create mapping." + ex);
            }
        }
        var roles = _roleManager.Roles.ToList();
        var departments = await _departmentService.GetAllAsync();
        ViewData["DepartmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
        ViewData["UserRoleId"] = new SelectList(roles, "Id", "Name");
        return View(departmentRoleMapping);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        var departmentRoleMappings = await _departmentRoleMappingService.GetListAsync(
        x => x.DepartmentId == id,
        includes: new Expression<Func<DepartmentRoleMapping, object>>[]
        {
            x => x.Department,
            x => x.Role
        }
        );
        if (departmentRoleMappings == null || !departmentRoleMappings.Any())
        {
            return NotFound();
        }
        var result = new DepartmentRoleMappingViewModel
        {
            DepartmentId = id ?? 0,
            DepartmentName = departmentRoleMappings.First().Department.DepartmentName,
            Roles = departmentRoleMappings.Select(drm => drm.Role).Distinct().ToList()
        };
        return View(result);
    }

    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var departmentExists = await _departmentService.IsExistsAsync(x => x.DepartmentId == id);
            if (!departmentExists)
            {
                return NotFound();
            }
            var existingMappings = await _departmentRoleMappingService.GetListAsync(
            x => x.DepartmentId == id);

            foreach (var mapping in existingMappings)
            {
                await _departmentRoleMappingService.RemoveAsync(mapping.DepartmentRoleId);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Unable to delete mapping." + ex);
            return View();
        }
    }
}
