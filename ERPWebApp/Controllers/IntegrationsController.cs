using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ERPWebApp.Controllers;
[AutoValidateAntiforgeryToken]
public class IntegrationsController : Controller
{
    private readonly IIntegrationService _integrationService;

    public IntegrationsController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    // GET: Integrations  
    public async Task<IActionResult> Index()
    {
        var integrations = await _integrationService.GetAllAsync();
        return View(integrations);
    }

    // GET: Integrations/Create  
    public IActionResult Create()
    {
        return View();
    }

    // POST: Integrations/Create  
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("Name,Description,AccessToken,StoreName")] Integration integration)
    {
        if (ModelState.IsValid)
        {
            await _integrationService.AddAsync(integration);
            return RedirectToAction(nameof(Index));
        }
        return View(integration);
    }

    // GET: Integrations/Edit/5  
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var integration = await _integrationService.GetAsync(i => i.Id == id);
        if (integration == null)
        {
            return NotFound();
        }
        return View(integration);
    }

    // POST: Integrations/Edit/5  
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,AccessToken,StoreName")] Integration integration)
    {
        if (id != integration.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _integrationService.UpdateAsync(integration);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await IntegrationExists(integration.Id))
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
        return View(integration);
    }

    public IActionResult CreateShopifyIntegration(string shopName, string apiKey)
    {
        // Ensure shopName and apiKey are provided  
        if (string.IsNullOrEmpty(shopName) || string.IsNullOrEmpty(apiKey))
        {
            return BadRequest("Shop Name and API Key are required.");
        }

        // Construct the OAuth URL  
        string redirectUri = Url.Action("ShopifyCallback", "Integrations", null, Request.Scheme);
        string scopes = "read_orders,write_orders";
        string state = Guid.NewGuid().ToString(); // CSRF protection  

        string oauthUrl = $"https://{shopName}.myshopify.com/admin/oauth/authorize?client_id={apiKey}&scope={scopes}&redirect_uri={redirectUri}&state={state}";

        return Redirect(oauthUrl);
    }

    private async Task<bool> IntegrationExists(int id)
    {
        return await _integrationService.IsExistsAsync(i => i.Id == id);
    }
}
