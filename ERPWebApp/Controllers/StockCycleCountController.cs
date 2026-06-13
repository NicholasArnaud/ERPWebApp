using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers;

[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class StockCycleCountController : Controller
{
    private readonly ApplicationDbContext _context;

    public StockCycleCountController(ApplicationDbContext context)
    {
        _context = context;

    }
    public IActionResult Index()
    {
        ViewData["SiteName"] = new SelectList(_context.Site, "SiteId", "SiteName");
        ViewData["DepartmentName"] = new SelectList(_context.Department, "DepartmentId", "DepartmentName");
        ViewData["SkuName"] = new SelectList(_context.Product, "ProductId", "Sku");


        var stockList = _context.Stock.Include(x => x.Products).Include(x => x.Location).Where(x => x.TotalAvailable > 0).ToList();

        return View(stockList);
    }

    public ActionResult DisplayResults(int siteId, int departmentId, int skuId)
    {
        var model = _context.Stock.Include(x => x.Location).Include(x => x.Location.Sites).Include(x => x.Products).Where(x => x.Location.SiteId == siteId).ToList();
        // xxx
        if (siteId == -1 && departmentId == -1 && skuId == -1)
        {
            model = _context.Stock.Where(x => x.TotalAvailable > 0).Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).ToList();
        }
        //xxy
        else if (siteId == -1 && departmentId == -1 && skuId != -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Products.ProductId == skuId).ToList();
        }
        //xyy
        else if (siteId == -1 && departmentId != -1 && skuId != -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Products.Departments).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Products.ProductId == skuId && x.Products.Departments.First().DepartmentId == departmentId).ToList();
        }
        //xyx
        else if (siteId == -1 && departmentId != -1 && skuId == -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Products.Departments).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Products.Departments.First().DepartmentId == departmentId).ToList();
        }
        //yxx
        else if (siteId != -1 && departmentId == -1 && skuId == -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Location.Sites.SiteId == siteId).ToList();
        }
        //yyx
        else if (siteId != -1 && departmentId != -1 && skuId == -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Location.Sites.SiteId == siteId && x.Products.Departments.First().DepartmentId == departmentId).ToList();
        }
        //yxy
        else if (siteId != -1 && departmentId == -1 && skuId != -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Location.Sites.SiteId == siteId && x.Products.ProductId == skuId).ToList();
        }
        //yyy
        else if (siteId != -1 && departmentId != -1 && skuId != -1)
        {
            model = _context.Stock.Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).Where(x => x.Location.Sites.SiteId == siteId && x.Products.ProductId == skuId && x.Products.Departments.First().DepartmentId == departmentId).ToList();
        }

        return PartialView("TablePartial", model);
    }
    public ActionResult DisplayDefault()
    {
        var model = _context.Stock.Where(x => x.TotalAvailable > 0).Include(x => x.Products).Include(x => x.Location.Sites).Include(x => x.Location).ToList();

        return PartialView("TablePartial", model);
    }
    // GET: Vendors/Edit/5
    [HttpGet]
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var stock = _context.Stock.Include(x => x.Products).Include(x => x.Products.Departments).Include(x => x.Location).Include(x => x.Location.Sites).Where(x => x.StockId == id).FirstOrDefault();
        if (stock == null)
        {
            return NotFound();
        }
        return PartialView(stock);
    }

    [HttpPost]
    
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit([Bind("StockId, TotalAvailable")] Stock stock)
    {
        if (stock.StockId == 0)
        {
            return RedirectToAction("Index");
        }

        try
        {
            var origional = _context.Stock.Find(stock.StockId);
            origional.ModifyDate = DateTime.Now;
            origional.ModifyByUser = this.User.Identity.Name;
            origional.TotalAvailable = stock.TotalAvailable;
            _context.Update(origional);
            await _context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }

        return View("Index");
    }
}
