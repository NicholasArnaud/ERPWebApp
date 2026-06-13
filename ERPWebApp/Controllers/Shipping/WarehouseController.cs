using ERPWebApp.Models;
using ERPWebApp.Services;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace ERPWebApp.Controllers.Shipping;

[Authorize(Roles = RoleList.ShippingManager + "," + RoleList.ShippingBasic + "," + RoleList.Administrator + "," + RoleList.Manager)]
[AutoValidateAntiforgeryToken]
public class WarehouseController : Controller
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        Expression<Func<Warehouse, object>>[] includes = new Expression<Func<Warehouse, object>>[]
        {
            w => w.BillingAddress 
        };
        var warehouses = await _warehouseService.GetAllAsync(includes: includes);

        return View(warehouses);
    }
    
    public async Task<IActionResult> Create()
    {
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
                            .Select(tz => new SelectListItem
                            {
                                Value = tz.Id,
                                Text = tz.DisplayName
                            })
                            .ToList();

        ViewData["TimeZone"] = new SelectList(timeZones, "Value", "Text");
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("WarehouseName,DefaultWarehouse,Company,Country,StreetAddress1,StreetAddress2,City," +
        "State,PostalCode,PhoneNumber,Email,TimeZone,SameAsReturnAddress,BillingAddressId,BillingAddress")] Warehouse warehouse)
    {
        if (ModelState.IsValid) 
        {
               await _warehouseService.AddAsync(warehouse);
                return RedirectToAction(nameof(Index));
        }
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
                            .Select(tz => new SelectListItem
                            {
                                Value = tz.Id,
                                Text = tz.DisplayName
                            })
                            .ToList();

        ViewData["TimeZone"] = new SelectList(timeZones, "Value", "Text");

        return View(warehouse);
    }

    public async Task<IActionResult> Details(int? id)
    {
        var warehouse = await _warehouseService.GetAsync(
        x => x.WarehouseId == id,
        includes: [w => w.BillingAddress]
        );

        if (warehouse == null)
        {
            return NotFound();
        }

        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(warehouse.TimeZone);
        ViewData["TimeZoneDisplayName"] = tzInfo.DisplayName;
        
        return View(warehouse);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        var warehouse = await _warehouseService.GetAsync(
        x => x.WarehouseId == id,
        includes: [w => w.BillingAddress]
        );
        if (warehouse == null)
        {
            return NotFound();
        }
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
                            .Select(tz => new SelectListItem
                            {
                                Value = tz.Id,
                                Text = tz.DisplayName
                            })
                            .ToList();

        ViewData["TimeZone"] = new SelectList(timeZones, "Value", "Text");

        return View(warehouse);
    }

    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("WarehouseId,WarehouseName,DefaultWarehouse,Company,Country,StreetAddress1,StreetAddress2,City," +
        "State,PostalCode,PhoneNumber,Email,TimeZone,SameAsReturnAddress,BillingAddressId,BillingAddress")] Warehouse warehouse)
    {

        if (ModelState.IsValid)
        {
            var isExist = await _warehouseService.IsExistsAsync(x => x.WarehouseId == id);
            if (!isExist)
            {
                return NotFound();
            }

            else
            {
                try
                {
                    await _warehouseService.UpdateAsync(warehouse);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured in saving warehouse");
                    ModelState.AddModelError(string.Empty, "Unable to update warehouse.");
                }
            }
        }
        
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
                            .Select(tz => new SelectListItem
                            {
                                Value = tz.Id,
                                Text = tz.DisplayName
                            })
                            .ToList();

        ViewData["TimeZone"] = new SelectList(timeZones, "Value", "Text");

        return View(warehouse);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        var warehouse = await _warehouseService.GetAsync(x => x.WarehouseId == id);
        
        if (warehouse == null)
        {
            return NotFound();
        }
        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(warehouse.TimeZone);
        ViewData["TimeZoneDisplayName"] = tzInfo.DisplayName;

        return View(warehouse);
    }

    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var isExist = await _warehouseService.IsExistsAsync(x => x.WarehouseId == id);
        if (isExist)
        {
            await _warehouseService.RemoveAsync(id);
        }
        else
        {
            return NotFound(id);
        }

        return RedirectToAction(nameof(Index));
    }
}
