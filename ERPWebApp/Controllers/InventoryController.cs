using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using ERPWebApp.Models.Company;
using System.Security.Claims;
using ERPWebApp.Middleware;
using ERPWebApp.Models.Common;

namespace ERPWebApp.Controllers;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.Developer)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class InventoryController : Controller
{
    private InventoryViewModel _Inventory;
    private readonly IInventoryService _inventoryService;
    private readonly IStocksService _stocksService;
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly IMyDashService _myDashService;

    public InventoryController(
        IInventoryService inventoryService,
        IStocksService stocksService,
        IUserPreferencesService userPreferencesService,
        IMyDashService myDashService
    )
    {
        _inventoryService = inventoryService;
        _stocksService = stocksService;
        _userPreferencesService = userPreferencesService;
        _Inventory = new InventoryViewModel();
        _myDashService = myDashService;
    }

    public async Task<IActionResult> IndexAsync()
    {
        var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (currentUserID is not null)
        {
            _Inventory.DashboardLayouts = await _userPreferencesService.GetDashboardLayoutByDashboardAsync(currentUserID, DashboardNames.DashboardInventory.ToString());
            var result = await _myDashService.GetUserDashboadData(currentUserID);
            if (result != null)
            {
                _Inventory.SiteVolumetrics = result.SiteVolumetrics;
                _Inventory.ProductCyleCount = result.ProductCyleCount;
                _Inventory.TopRequestedProducts = result.TopRequestedProducts;
                _Inventory.TopMovedProducts = result.TopMovedProducts;
                _Inventory.TopReasonRequest = result.TopReasonRequest;
            }   
        }

        return View(_Inventory);
    }
    public async Task<List<MovedProductsDto>> MovedProducts(int days)
    {
        var inventoryInformation = await _inventoryService.MovedProducts(days);
        var movedProductData = JsonConvert.SerializeObject(inventoryInformation);
        return inventoryInformation;
    }
    public async Task<List<ProductCycleCountDto>> ProductCycleCount()
    {
        var inventoryInformation = await _inventoryService.ProductCycleCount();
        var productCycleCountData = JsonConvert.SerializeObject(inventoryInformation);
        return inventoryInformation;
    }
    public async Task<List<RequestedProductsDto>> RequestedProducts(int days)
    {
        var inventoryInformation = await _inventoryService.RequestedProducts(days);
        var requestedProductsData = JsonConvert.SerializeObject(inventoryInformation);
        return inventoryInformation;
    }
    public async Task<List<RequestedReasonDto>> RequestedReason(int days)
    {
        var inventoryInformation = await _inventoryService.RequestedReason(days);
        var requestedReasonData = JsonConvert.SerializeObject(inventoryInformation);
        return inventoryInformation;
    }

    public async Task<List<VolumetricsDto>> Volumetrics()
    {
        var volumetricsData = await _inventoryService.Volumetrics();
        var volumetricsDtoList = new List<VolumetricsDto>();

        //I needed access to stocksService for the volume tally, and I'm unsure if I should be utilizing a service in a repository, so I did it here for now.
        foreach (var site in volumetricsData)
        {
            var total = _stocksService.SetupVolumeTally(site.SiteId);
            var volumeTally = (int)decimal.Round(total);
            var volumeTarget = (int)decimal.Round(site.SiteVolume);
            var volRatio = site.SiteVolume != 0 ? Math.Round((double)total / (double)site.SiteVolume * 100, 2) : 0;

            var gradToColors = volRatio <= 10 ? "#22C494" :
                                volRatio <= 20 ? "#3AB892" :
                                volRatio <= 30 ? "#52AD8F" :
                                volRatio <= 40 ? "#6AA18C" :
                                volRatio <= 50 ? "#82968A" :
                                volRatio <= 60 ? "#9A8A87" :
                                volRatio <= 70 ? "#B27F84" :
                                volRatio <= 80 ? "#CA7381" :
                                volRatio <= 90 ? "#E2687F" :
                                                "#FA5C7C";

            volumetricsDtoList.Add(new VolumetricsDto
            {
                SiteNames = site.SiteNames,
                VolumeTally = volumeTally,
                VolumeTarget = volumeTarget,
                VolRatio = volRatio,
                GradToColors = gradToColors
            });
        }

        return volumetricsDtoList;
    }
}
