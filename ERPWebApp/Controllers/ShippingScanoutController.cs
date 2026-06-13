using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Extensions;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Shipping;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ERPWebApp.Controllers;

public enum ScanActionType
{
    Submit,
    Add,
    Delete,
    Unknown
}

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.ShippingBasic)]
[CwaFeatureGate(CwaFeatures.ORDER)]
[AutoValidateAntiforgeryToken]
public class ShippingScanoutController(
    IOrderService orderService,
    IOrderShippingService orderShippingService,
    IWarehouseService warehouseService,
    IShippingScanoutService shippingScanoutService,
    ILogger<ShippingScanoutController> logger,
    IWebhooks webhooks,
    IFilesService filesService
) : Controller
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IOrderService _orderService = orderService;
    private readonly IShippingScanoutService _shippingScanoutService = shippingScanoutService;
    private readonly DateTime _now = TimeZoneInfo.ConvertTime(
        DateTime.Now,
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
    );


    /// <summary>
    /// This method is used to create a shipping scanout viewmodel and pass it to the index view
    /// </summary>
    /// <param name="current">This is the current shipping scanout viewmodel</param>
    /// <returns>Returns the index view with the current shipping scanout viewmodel</returns>
    [HttpGet]
    public async Task<IActionResult> Index(ShippingScanoutViewModel current = default, string activeTab = null)
    {
        if (current == default)
        {

            current = new ShippingScanoutViewModel();
        }
        ViewData["LastScannedTrackingNumber"] = TempData["LastScannedTrackingNumber"];
        ViewData["manifestError"] = TempData["manifestError"];

        switch (activeTab)
        {
            case "manifest" or "closedshipments":
                await LoadShippingProperties();
                break;
            case "openshipments":
                await LoadOpenShipmentsByCarrier();
                break;
        }
        ViewData["ActiveTab"] = activeTab;
        return View(current);
    }

    [HttpPost, ActionName("PerformScanAction")]
    
    public async Task<IActionResult> PerformScanActionAsync(
        [Bind("CurrentScan,DeleteScan,HistoricalScans,CarrierDayTotal,ActionType, AudioAlerts")] ShippingScanoutViewModel shippingScanoutViewModel)
    {
        //This model check should verify if the scanned tracking number is valid/ has not been scanned before
        if (ModelState.IsValid || (shippingScanoutViewModel.ActionType != ScanActionType.Add && shippingScanoutViewModel.ActionType != ScanActionType.Delete))
        {
            shippingScanoutViewModel.CurrentScan.ScannedTrackingNumber = shippingScanoutViewModel.CurrentScan?.ScannedTrackingNumber.Trim().Normalize();
            IActionResult result = shippingScanoutViewModel.ActionType switch
            {
                ScanActionType.Submit => await SubmitScans(shippingScanoutViewModel.CurrentScan.TrailerNumber),
                ScanActionType.Add => await AddScan(shippingScanoutViewModel),
                ScanActionType.Delete => await DeleteScan(shippingScanoutViewModel.CurrentScan.ShippingScanoutId),
                _ => RedirectToAction(nameof(Index), shippingScanoutViewModel),
            };
            return result;
        }
        
        return View(nameof(Index),shippingScanoutViewModel);
    }

    private async Task<IActionResult> SubmitScans(string trailerNumber)
    {
        var ValidUPSScansToSend = await shippingScanoutService.GetListAsync(x => x.IsValidTrackingNumber && x.CreateDate.Date >= _now.Date.AddDays(-3) && x.WebhookBatchId == null);
        try
        {
            if (ValidUPSScansToSend.Any() && trailerNumber.Any())
            {
                // Process tracking numbers with the SendUPSListAddOrRemoveFromUloRequest method  
                await shippingScanoutService.SendUPSListAddOrRemoveFromUloRequest(ValidUPSScansToSend, trailerNumber);
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending scans to ULO");
            throw;
        }

        return RedirectToAction(nameof(Index));
    }


    private async Task<IActionResult> DeleteScan(int ShippingScanoutId)
    {
        if (ShippingScanoutId != default)
            await shippingScanoutService.RemoveAsync(ShippingScanoutId);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetScan([Bind("TrackingNumber")]ShippingScanoutViewModel shippingScanoutViewModel)
    {
        if(!shippingScanoutViewModel.TrackingNumber.Any())
        {
            return RedirectToAction(nameof(Index));
        }
        var scannedTrackingNumber = await shippingScanoutService.GetAsync(x => x.ScannedTrackingNumber.EndsWith(shippingScanoutViewModel.TrackingNumber));
        if (scannedTrackingNumber == null)
        {
            ModelState.AddModelError(nameof(shippingScanoutViewModel.TrackingNumber), "Tracking Number not scanned");
        }
        var shippedTrackingNumber = await orderShippingService.GetAsync(x => x.trackingNumber.EndsWith(shippingScanoutViewModel.TrackingNumber));
        if (scannedTrackingNumber == null)
        {
            ModelState.AddModelError(nameof(shippingScanoutViewModel.TrackingNumber), "Tracking Number not found or created");
        }
        shippingScanoutViewModel.SearchedScanned = scannedTrackingNumber;
        ViewData["ActiveTab"] = "search";
        return View(nameof(Index), shippingScanoutViewModel);
    }

    private async Task<IActionResult> AddScan(ShippingScanoutViewModel shippingScanoutViewModel)
    {
        ViewData["ActiveTab"] = "scanout";
        if (shippingScanoutViewModel.CurrentScan == default || TrackingNumberExists(shippingScanoutViewModel.CurrentScan.ScannedTrackingNumber))
        {
            ModelState.AddModelError("CurrentScan.ScannedTrackingNumber","Invalid Tracking Number Or Already Exists");
            return View(nameof(Index), shippingScanoutViewModel);
        }
        try
        {
            shippingScanoutViewModel.CurrentScan.CreatedBy = User.Identity.Name;
            await shippingScanoutService.CreateNewShippingScanout(shippingScanoutViewModel.CurrentScan);
        }
        catch(SqlException ex)
        {
            logger.LogInformation(ex, "Error creating shipping scanout");
            ModelState.AddModelError("CurrentScan.ScannedTrackingNumber", "Invalid Tracking Number Or Already Exists");
            return View(nameof(Index), shippingScanoutViewModel);
        }

        TempData["LastScannedTrackingNumber"] = shippingScanoutViewModel.CurrentScan.ScannedTrackingNumber;
        return RedirectToAction(nameof(Index));
    }

    private bool TrackingNumberExists(string trackingNumber)
    {
        return shippingScanoutService.IsExists(e => e.ScannedTrackingNumber == trackingNumber || e.ScannedTrackingNumber.EndsWith(trackingNumber));
    }

    public async Task<IReadOnlyCollection<ShipmentsCountByCarrier>> GetOpenShipmentsCountByCarrier()
        => await shippingScanoutService.GetOpenShipmentsCountByCarrierAsync();

    private Task<IEnumerable<ShipEngineWarehouse>> ShippingWarehouseSelectList => _warehouseService.FetchWarehouses();
    private Task<List<ShipEngineCarriers>> ShippingCarrierSelectList => shippingScanoutService.FetchCarriers<List<ShipEngineCarriers>>();


    private async Task LoadShippingProperties()
    {
        var warehouses = await ShippingWarehouseSelectList;
        ViewData["Warehouses"] = new SelectList(warehouses ?? [], "WarehouseId", "WarehouseName");

        var carriers = await ShippingCarrierSelectList;
        var carrierList = carriers.Select(x => new SelectListItem
        {
            Text = $"{x.CarrierCode} | {x.Nickname}",
            Value = x.CarrierId

        }
        ).ToList();
        ViewData["Carriers"] = new SelectList(carrierList ?? [], "Value", "Text");
    }

    private async Task LoadOpenShipmentsByCarrier()
    {
        var openShipmentsCountByCarrier = await shippingScanoutService.GetOpenShipmentsCountByCarrierAsync();
        ViewData["ShipmentsCountByCarrier"] = openShipmentsCountByCarrier;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateManifest(ManifestGenerateRequest request)
    {
        ViewData["ActiveTab"] = "manifest";
        if (string.IsNullOrEmpty(request.WarehouseId) || string.IsNullOrEmpty(request.CarrierId))
            return RedirectToAction(nameof(Index), new { activeTab = "manifest" });

        var warehouses = await ShippingWarehouseSelectList;
        var selectedWarehouse = warehouses?.FirstOrDefault(x => x.WarehouseId == request.WarehouseId);

        var carriers = await ShippingCarrierSelectList;
        var selectedCarrier = carriers?.FirstOrDefault(x => x.CarrierId == request.CarrierId);
        var carrier = $"{selectedCarrier!.CarrierCode} | {selectedCarrier.Nickname}";

        if (selectedCarrier.Nickname.ToLower().Contains("stamps")
           || selectedCarrier.CarrierCode.ToLower().Contains("stamps")
           || selectedCarrier.CarrierCode.ToLower().Contains("usps")
        )
        {
            await _shippingScanoutService.GenerateUspsManifest(selectedWarehouse);
        }
        else
        {
            await GenerateDhlManifests(
                request.WarehouseId,
                selectedWarehouse!.WarehouseName,
                request.CarrierId,
                carrier
            );
        }

        return RedirectToAction(nameof(Index), new { activeTab = "manifest" });
    }

    private async Task GenerateDhlManifests(
        string warehouseId,
        string warehouseName,
        string carrierId,
        string carrierCode
    )
    {
        var response = await webhooks.GenerateManifest<IReadOnlyCollection<ShippingManifest>>(
            warehouseId,
            carrierId,
            DateTime.UtcNow
        );

        if (response.Count > 0)
        {
            foreach (var manifest in response)
            {
                manifest.Warehouse = warehouseName;
                manifest.Carrier = carrierCode;
            }

            await shippingScanoutService.SaveShippingManifestsAsync(response.ToList());
        }
    }

    [HttpGet("ClosedShipments")]
    public async Task<IActionResult> GetClosedShipments(
        string carrierId,
        string warehouseId,
        DateTime? shipDate
    )
    {
        var draw = Request.Query["draw"].FirstOrDefault();
        var start = Request.Query["start"].FirstOrDefault();
        var length = Request.Query["length"].FirstOrDefault();
        var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"]
            .FirstOrDefault();
        var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();

        var search = new SearchParameters
        {
            Start = string.IsNullOrEmpty(start) ? 0 : int.Parse(start),
            PageSize = string.IsNullOrEmpty(length) ? -1 : int.Parse(length),
            SortBy = sortColumn,
            IsDescending = !string.IsNullOrEmpty(sortColumnDirection) && sortColumnDirection.ToLower() is not "asc",
            UserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        };
        
        var (manifests, count) = await shippingScanoutService.GetShippingManifestsAsync(
            carrierId,
            warehouseId,
            shipDate,
            search
        );
        
        var jsonData = new
        {
            draw,
            data = manifests,
            recordsTotal = count,
            recordsFiltered = count
        };
            
        return Ok(jsonData);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadManifest(string manifestUrl, string carrier)
    {
        await webhooks.DownloadManifest(manifestUrl);
        return Ok();
    }
}
