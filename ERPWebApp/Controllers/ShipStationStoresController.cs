using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.CustomerSupportBasic)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class ShipStationStoresController(
    IShipStationStoreService shipStationStoreService,
    ILogger<ShipStationStoresController> logger,
    IWebhooks webhooks
) : Controller
{
    // GET: ShipStationStores
    public async Task<IActionResult> Index()
    {
        var storeList = await shipStationStoreService.GetAllAsync(
            includes:
            [
                x=>x.StoreFiles
            ]
        );
        return View(storeList);
    }

    // GET: ShipStationStores/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shipStationStore = await shipStationStoreService.GetAsync(
            (q) => q.Where(x => x.ShipStationStoreId == id)
                .Include(x => x.StoreFiles)
                .ThenInclude(x => x.Files)
        );
        return shipStationStore == null ? NotFound() : View(shipStationStore);
    }

    // GET: ShipStationStores/Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> Create()
    {
        await LoadStores(null);

        LoadStoreTypes("");

        return View();
    }

    private void LoadStoreTypes(string storeType)
    {
        var StoreTypes = Enum.GetValues(typeof(StoreType)).Cast<StoreType>().Select(
            v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }
        ).ToList();

        ViewData["StoreTypes"] = new SelectList(StoreTypes, "Text", "Text", storeType);
    }

    // POST: ShipStationStores/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create(
        [Bind("ShipStationStoreId,StoreId,StoreName,ContactName,PhoneNumber,FaxNumber,Address,Email,PublicEmail,IsActive,HasIncreasedPricing,StoreType,RawFiles,Notes")]
        ShipStationStore shipStationStore
    )
    {
        if (!ModelState.IsValid)
        {
            LoadStoreTypes(shipStationStore.StoreType.ToString());
            return View(shipStationStore);
        }

        var storeIdExists = await shipStationStoreService.IsExistsAsync(
            x => x.StoreId == shipStationStore.StoreId
        );
        if (storeIdExists)
            ModelState.AddModelError("StoreId", $"StoreId {shipStationStore.StoreId} already exists.");
        
        var storeNameExists = await shipStationStoreService.IsExistsAsync(
            x => x.StoreName.Trim().ToLower() == shipStationStore.StoreName.Trim().ToLower()
        );
        if (storeNameExists)
            ModelState.AddModelError("StoreName", $"StoreName {shipStationStore.StoreName} already exists.");

        if (ModelState.IsValid)
        {
            await shipStationStoreService.AddAsync(shipStationStore);
            return RedirectToAction(nameof(Index));
        }

        LoadStoreTypes(shipStationStore.StoreType.ToString());
        await LoadStores(shipStationStore.StoreId);
        return View(shipStationStore);
    }

    // GET: ShipStationStores/Edit/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shipStationStore = await shipStationStoreService.GetAsync(
            (q) => q.Where(x => x.ShipStationStoreId == id)
                .Include(x => x.StoreFiles)
                .ThenInclude(x => x.Files)
        );

        if (shipStationStore == null)
        {
            return NotFound();
        }

        var StoreTypes = Enum.GetValues(typeof(StoreType)).Cast<StoreType>().Select(
            v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }
        ).ToList();

        ViewData["StoreTypes"] = new SelectList(StoreTypes, "Text", "Text", shipStationStore.StoreType.ToString());

        return View(shipStationStore);
    }

    // POST: ShipStationStores/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(
        int id,
        [Bind("ShipStationStoreId,ContactName,PhoneNumber,FaxNumber,Address,Email,PublicEmail,IsActive,HasIncreasedPricing,StoreType,StoreName,RawFiles,Notes")]
    ShipStationStore shipStationStore
    )
    {
        if (id != shipStationStore.ShipStationStoreId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Model state is not valid.");
            return View(shipStationStore);
        }

        try
        {
            var store = await shipStationStoreService.GetAsync(
                x => x.ShipStationStoreId == shipStationStore.ShipStationStoreId && x.IsActive
            );
            if (store == null)
            {
                ModelState.AddModelError(string.Empty, "No ShipStation store with a matching Id was found.");
                return View(shipStationStore);
            }

            store.ContactName = shipStationStore.ContactName;
            store.PhoneNumber = shipStationStore.PhoneNumber;
            store.FaxNumber = shipStationStore.FaxNumber;
            store.Address = shipStationStore.Address;
            store.Email = shipStationStore.Email;
            store.PublicEmail = shipStationStore.PublicEmail;
            store.IsActive = shipStationStore.IsActive;
            store.HasIncreasedPricing = shipStationStore.HasIncreasedPricing;
            store.StoreType = shipStationStore.StoreType;
            store.RawFiles = shipStationStore.RawFiles;
            store.Notes = shipStationStore.Notes;

            await shipStationStoreService.UpdateAsync(store);
        }
        catch (DbUpdateConcurrencyException)
        {
            var isExists = await shipStationStoreService.IsExistsAsync(x => x.ShipStationStoreId == shipStationStore.ShipStationStoreId);
            if (!isExists)
            {
                ModelState.AddModelError(string.Empty, "The store being updated no longer exists.");
                return View(shipStationStore);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "A concurrency error occurred. Please try again.");
                return View(shipStationStore);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: ShipStationStores/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shipStationStore = await shipStationStoreService.GetAsync(x => x.ShipStationStoreId == id);

        return shipStationStore == null ? NotFound() : View(shipStationStore);
    }

    // POST: ShipStationStores/Delete/5
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.CustomerSupportManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await shipStationStoreService.RemoveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> DeleteFile(int storeFileId)
    {
        var storeFile = await shipStationStoreService.GetStoreFileAsync(storeFileId);
        if (storeFile != null)
        {
            await shipStationStoreService.DeleteStoreFile(storeFile);
        }
        return RedirectToAction(nameof(Edit), new { id = storeFile?.ShipStationStoreId });
    }

    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        var storeList= await GetShipStationStores();
        return Ok(storeList);
    }

    private async Task LoadStores(int? storeId)
    {
        var storeList= await GetShipStationStores();
        ViewBag.Stores = new SelectList(
            storeList,
            nameof(ShipStationJson.StoreId),
            nameof(ShipStationJson.StoreName),
            storeId
        );
    }

    private async Task<IEnumerable<ShipStationJson>> GetShipStationStores()
    {
        //Retrieve all ShipStations from ShipStationStores
        return await shipStationStoreService.GetShipStationStores();
    }

    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyStores()
    {
        try
        {
            var results = await GetShipStationStores();
            var messages = await shipStationStoreService.VerifyShipStationStores(results);
            return Json(new
            {
                success = true,
                message = "Stores verified successfully",
                data = messages
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred");
            return Json(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }
}