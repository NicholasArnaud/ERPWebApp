
using BarcodeStandard;
using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System.Data;

namespace ERPWebApp.Controllers.Inventory;


[Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager + "," + RoleList.InventoryBasic + "," + RoleList.ShippingBasic)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class ProductContainersController : Controller
{
    private readonly decimal inchtofeet = (decimal)1 / (12 * 12 * 12);
    private readonly decimal centitofeet = (decimal)1 / ((decimal)2.54 * 12 * (decimal)2.54 * 12 * (decimal)2.54 * 12);
    private readonly decimal metertofeet = (decimal)(100 * 100 * 100) / ((decimal)2.54 * 12 * (decimal)2.54 * 12 * (decimal)2.54 * 12);
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    private readonly ApplicationDbContext _context;
    public static ProductContainersFilter _productContainerDbFull = new ProductContainersFilter();

    public ProductContainersController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        _productContainerDbFull.Sku = new SelectList(_context.Product, "ProductId", "Sku");
        _productContainerDbFull.Vendor = new SelectList(_context.Vendor, "VendorId", "VendorName");
        _productContainerDbFull.ProductContainers = _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor).ToList();
        return View(_productContainerDbFull);
    }
    //serverside datatable call gets the product container and updates the datatable is user searches
    [HttpPost("GetContainers")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetContainers(string sku, string vendor, bool active)
    {
        try
        {
            var skuFilterList = JsonConvert.DeserializeObject<string>(sku);
            var vendorFilterList = JsonConvert.DeserializeObject<string>(vendor);
            //gets data for datatables look into request if you want other options
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var end = Request.Form["columns[16][data]"].Last();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            //var calcfloorcnt = Math.Floor((decimal)end / 18);
            int pageSize = 0;
            if (length != null)
            {
                pageSize = Convert.ToInt32(length);
            }
            int skip = 0;
            if (start != null)
            {
                skip = Convert.ToInt32(start);
            }
            int recordsTotal = 0;
            var containerData = (from tempuser in _productContainerDbFull.ProductContainers select tempuser);
            if (active)
            {
                containerData = containerData.Where(x => x.IsActive);
            }
            if (skuFilterList != "Any")
            {
                if (vendorFilterList != "All")
                {
                    containerData = containerData.Where(x => x.ProductVendorMappings.ProductId.ToString() == skuFilterList || x.ProductVendorMappings.VendorId.ToString() == vendorFilterList);
                }
                else
                {
                    containerData = containerData.Where(x => x.ProductVendorMappings.ProductId.ToString() == skuFilterList);

                }
            }
            else
            {
                if (vendorFilterList != "All")
                {
                    containerData = containerData.Where(x => x.ProductVendorMappings.VendorId.ToString() == vendorFilterList);

                }
                else
                {

                }
            }
            recordsTotal = containerData.Count();
            //gets sort colum nand direction
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            {
                if (sortColumnDirection == "asc")
                {
                    switch (sortColumn)
                    {
                        case "ProductVendorMappings.Product.Sku":
                            containerData = containerData.OrderBy(s => s.ProductVendorMappings.Product.Sku).ThenBy(s => s.ProductVendorMappings.Vendor.VendorName);
                            break;
                        case "ProductVendorMappings.Vendor.VendorName":
                            containerData = containerData.OrderBy(s => s.ProductVendorMappings.Vendor.VendorName).ThenBy(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        case "ContainerQuantity":
                            containerData = containerData.OrderBy(s => s.ContainerQuantity).ThenBy(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        case "ContainerCost":
                            containerData = containerData.OrderBy(s => s.ContainerCost).ThenBy(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        default:
                            containerData = containerData.OrderBy(s => s.ProductVendorMappings.Product.Sku).ThenBy(s => s.ProductVendorMappings.Vendor.VendorName);
                            break;
                    }

                }
                else if (sortColumnDirection == "desc")
                {
                    switch (sortColumn)
                    {
                        case "ProductVendorMappings.Product.Sku":
                            containerData = containerData.OrderByDescending(s => s.ProductVendorMappings.Product.Sku).ThenByDescending(s => s.ProductVendorMappings.Vendor.VendorName);
                            break;
                        case "ProductVendorMappings.Vendor.VendorName":
                            containerData = containerData.OrderByDescending(s => s.ProductVendorMappings.Vendor.VendorName).ThenByDescending(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        case "ContainerQuantity":
                            containerData = containerData.OrderByDescending(s => s.ContainerQuantity).ThenByDescending(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        case "ContainerCost":
                            containerData = containerData.OrderByDescending(s => s.ContainerCost).ThenByDescending(s => s.ProductVendorMappings.Product.Sku);
                            break;
                        default:
                            containerData = containerData.OrderByDescending(s => s.ProductVendorMappings.Product.Sku).ThenByDescending(s => s.ProductVendorMappings.Vendor.VendorName);
                            break;
                    }
                }
            }
            //filters based on searchvalue entered
            if (!string.IsNullOrEmpty(searchValue))
            {
                containerData = containerData.Where(m => m.ProductVendorMappings.Product.Sku.Contains(searchValue)
                || m.ProductVendorMappings.Vendor.VendorName.ToUpper().Contains(searchValue.ToUpper())
                || m.ProductVendorMappings.Product.Description.ToUpper().Contains(searchValue.ToUpper())
                || m.ContainerCost.ToString().Contains(searchValue));
            }
            recordsTotal = containerData.Count();
            var data = containerData.Skip(skip).Take(pageSize).ToList();
            var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
            return Ok(jsonData);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    //get create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult Create(string helper)
    {
        if (helper != null)
        {
            ViewData["errormessage"] = helper;
        }
        var selectListProductContainer = _context.ProductVendorMapping.Include(x => x.Product).Include(x => x.Vendor).Where(x => !_context.ProductContainer.Any(y => y.ProductVendorMappingId.ToString().Equals(x.ProductVendorMappingId.ToString()))).OrderBy(x => x.Product.Sku);
        ViewData["Products"] = new SelectList(selectListProductContainer.ToList().Select(x => new { ProductId = x.ProductVendorMappingId, SkuDes = "Sku: " + x.Product.Sku + " || Vendor: " + x.Vendor.VendorName }), "ProductId", "SkuDes", null);
        ViewData["ProductList"] = new SelectList(_context.Product.OrderBy(o => o.Sku).Select(x => new { ProductId = x.ProductId, Sku = x.Sku + " || " + x.Description }), "ProductId", "Sku");
        ViewData["VendorList"] = new SelectList(_context.Vendor.OrderBy(o => o.VendorId), "VendorId", "VendorName");
        return View();
    }

    // POST: /Create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("ContainerId,ProductId,ProductVendorMappingId,ContainerQuantity,Length,Width,Height,IsActive,ContainerDiminsions,ContainerCost")] ProductContainer productContainer)
    {
        if (ModelState.IsValid)
        {
            var productContainerDb = await _context.ProductVendorMapping.Include(x => x.Product).Where(x => x.ProductVendorMappingId == productContainer.ProductVendorMappingId).SingleOrDefaultAsync();
            if (productContainerDb != null)
            {
                var productVolumeConverstion = GetVolumeConversion(productContainerDb.Product.DimensionalUnit);
                var containerVolumeConversion = GetVolumeConversion(productContainer.ContainerDiminsions);
                var volume = productContainerDb.Product.Length * productContainerDb.Product.Width * productContainerDb.Product.Height;
                if (productContainer.Length * productContainer.Width * productContainer.Height * containerVolumeConversion < volume * productVolumeConverstion * productContainer.ContainerQuantity)
                {
                    return RedirectToAction(nameof(Create), new { @helper = "The Container Volume is to Small for the Product" });
                }
                productContainer.ModifyByUser = User.Identity.Name;
                productContainer.ModifyDate = now;
                _context.Add(productContainer);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
        var selectListProductContainer = _context.ProductVendorMapping.Include(x => x.Product).Include(x => x.Vendor).Where(x => !_context.ProductContainer.Any(y => y.ProductVendorMappingId.ToString().Equals(x.ProductVendorMappingId.ToString()))).OrderBy(x => x.Product.Sku);
        ViewData["Products"] = new SelectList(selectListProductContainer.ToList().Select(x => new { ProductId = x.ProductVendorMappingId, SkuDes = "Sku: " + x.Product.Sku + " || Vendor: " + x.Vendor.VendorName }), "ProductId", "SkuDes", null);
        ViewData["ProductList"] = new SelectList(_context.Product.OrderBy(o => o.Sku).Select(x => new { ProductId = x.ProductId, Sku = x.Sku + " || " + x.Description }), "ProductId", "Sku");
        ViewData["VendorList"] = new SelectList(_context.Vendor.OrderBy(o => o.VendorId), "VendorId", "VendorName");
        return View(productContainer);
    }

    //product vendor mapping create
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public IActionResult GetCreate()
    {
        return PartialView("Create");
    }
    // get/delete
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productContainerDb = await _context.ProductContainer
            .Include(x => x.ProductVendorMappings)
            .FirstOrDefaultAsync(m => m.ContainerId == id);
        if (productContainerDb == null)
        {
            return NotFound();
        }

        return View(productContainerDb);
    }

    //post/delete
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var productContainerDb = await _context.ProductContainer.FindAsync(id);
        _context.ProductContainer.Remove(productContainerDb);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    //details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var productContainerDb = await _context.ProductContainer
            .Include(p => p.ProductVendorMappings)
            .FirstOrDefaultAsync(m => m.ContainerId == id);
        if (productContainerDb == null)
        {
            return NotFound();
        }

        return View(productContainerDb);
    }
    // GET: /Edit/
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Edit(int? id, string helper)
    {
        if (id == null)
        {
            return NotFound();
        }
        if (helper != null)
        {
            ViewData["errormessage"] = helper;
        }
        var productContainerDb = await _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor).Where(x => x.ContainerId == id).SingleOrDefaultAsync();
        if (productContainerDb == null)
        {
            return NotFound();

        }
        ViewData["volumePUnit"] = GetVolumeConversion(productContainerDb.ProductVendorMappings.Product.DimensionalUnit);
        ViewData["volumeP"] = productContainerDb.ProductVendorMappings.Product.Length * productContainerDb.ProductVendorMappings.Product.Width * productContainerDb.ProductVendorMappings.Product.Height;
        var dbhelper = _context.ProductContainer.Where(x => x.ContainerId == id).FirstOrDefault();
        var selectListProductContainer = _context.ProductVendorMapping.Include(x => x.Product).Include(x => x.Vendor).Where(x => x.ProductVendorMappingId.Equals(dbhelper.ProductVendorMappingId) || !_context.ProductContainer.Any(y => y.ProductVendorMappingId.ToString().Equals(x.ProductVendorMappingId.ToString()))).OrderBy(x => x.Product.Sku);
        ViewData["Products"] = new SelectList(selectListProductContainer.ToList().Select(x => new { ProductId = x.ProductVendorMappingId, SkuDes = "Sku: " + x.Product.Sku + " || Vendor: " + x.Vendor.VendorName }), "ProductId", "SkuDes", null);
        ViewData["ProductList"] = new SelectList(_context.Product.OrderBy(o => o.Sku).Select(x => new { ProductId = x.ProductId, Sku = x.Sku + " || " + x.Description }), "ProductId", "Sku");
        ViewData["VendorList"] = new SelectList(_context.Vendor.OrderBy(o => o.VendorId), "VendorId", "VendorName");
        return View(productContainerDb);
    }

    // POST: /Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.InventoryManager + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, decimal volume, decimal volumeConversion, [Bind("ContainerId,ProductVendorMappingId,ProductId,ContainerQuantity,Length,Width,Height,IsActive,ContainerDiminsions,ContainerCost")] ProductContainer productContainer)
    {
        if (id != productContainer.ContainerId)
        {
            return NotFound();
        }
        //var prodcontainDbhelper=_context.ProductContainer.Include(x=>x.Products).Where(x=>x.ContainerId == id).FirstOrDefault();
        if (ModelState.IsValid)
        {
            try
            {
                productContainer.ModifyByUser = User.Identity.Name;
                productContainer.ModifyDate = now;
                var containerVolumeCOnverstion = GetVolumeConversion(productContainer.ContainerDiminsions);
                if (productContainer.Length * productContainer.Width * productContainer.Height * containerVolumeCOnverstion < volume * volumeConversion * productContainer.ContainerQuantity)
                {
                    return RedirectToAction(nameof(Edit), new { @id = productContainer.ContainerId, @helper = "The Container Volume is to Small for the Product" });
                }
                _context.Update(productContainer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContainerExists(productContainer.ContainerId))
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
        return RedirectToAction(nameof(Edit), new { @id = productContainer.ContainerId, @helper = "Model is not Valid" });
    }
    //checks if the container exists 
    private bool ContainerExists(int id)
    {
        return _context.ProductContainer.Any(e => e.ContainerId == id);
    }

    private decimal GetVolumeConversion(ContainerDiminsions container)
    {
        var volumeUnits = new decimal();
        if (container == ContainerDiminsions.Inches)
        {
            volumeUnits = inchtofeet;
        }
        else if (container == ContainerDiminsions.Feet)
        {
            volumeUnits = 1;
        }
        else if (container == ContainerDiminsions.Centimeters)
        {
            volumeUnits = centitofeet;
        }
        else if (container == ContainerDiminsions.Meters)
        {
            volumeUnits = metertofeet;
        }
        return volumeUnits;
    }

    //gets the conversion between the current units and feet
    private decimal GetVolumeConversion(DimensionalUnit container)
    {
        var volumeUnits = new decimal();
        if (container == DimensionalUnit.Inches)
        {
            volumeUnits = inchtofeet;
        }
        else if (container == DimensionalUnit.Feet)
        {
            volumeUnits = 1;
        }
        else if (container == DimensionalUnit.Centimeters)
        {
            volumeUnits = centitofeet;
        }
        else if (container == DimensionalUnit.Meters)
        {
            volumeUnits = metertofeet;
        }
        return volumeUnits;
    }

    //gets the volumetrics for the product when switching through the list
    public ActionResult GetProductVolume(string id)
    {
        if (id == null)
        {
            return Ok();
        }
        decimal VolumeLength = new decimal(),
        VolumeHeight = new decimal(),
        VolumeWidth = new decimal(),
        VolumeProd = new decimal(),
        VolumePUnit = new decimal();
        string DiminsionalUnits = "";
        var getDbProduct = _context.ProductVendorMapping.Include(x => x.Product).Where(x => x.ProductVendorMappingId.ToString().Equals(id)).Single();
        if (getDbProduct == null)
        {
            return NotFound();
        }
        VolumePUnit = GetVolumeConversion(getDbProduct.Product.DimensionalUnit);
        VolumeLength = getDbProduct.Product.Length;
        VolumeWidth = getDbProduct.Product.Width;
        VolumeHeight = getDbProduct.Product.Height;
        VolumeProd = getDbProduct.Product.Height * getDbProduct.Product.Width * getDbProduct.Product.Length;
        DiminsionalUnits = getDbProduct.Product.DimensionalUnit.ToString();
        return Json(new
        {
            VolumeLength,
            VolumeHeight,
            VolumeWidth,
            VolumeProd,
            VolumePUnit,
            DiminsionalUnits
        });
    }

    //gets containers by product and vendor selections
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ProductContainersFilter> GetContainersBySkuAndVendor(string Sku, string Vendor)
    {
        var selectedContainers = new List<ProductContainer>();
        if (Sku == "Any")
        {
            if (Vendor == "All")
            {
                var containersAll = await _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor).ToListAsync();
                selectedContainers.AddRange(containersAll);
            }
            else
            {
                var containersAllProducts = await _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor)
                    .Where(x => x.ProductVendorMappings.Vendor.VendorName == Vendor).ToListAsync();
                selectedContainers.AddRange(containersAllProducts);
            }
        }
        else
        {
            if (Vendor == "All")
            {
                var containersAllVendors = await _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor)
                    .Where(x => x.ProductVendorMappings.Product.Sku == Sku).ToListAsync();
                selectedContainers.AddRange(containersAllVendors);
            }
            else
            {
                var containersFiltered = await _context.ProductContainer.Include(x => x.ProductVendorMappings).Include(x => x.ProductVendorMappings.Product).Include(x => x.ProductVendorMappings.Vendor)
                    .Where(x => x.ProductVendorMappings.Product.Sku == Sku && x.ProductVendorMappings.Vendor.VendorName == Vendor).ToListAsync();
                selectedContainers.AddRange(containersFiltered);
            }
        }
        _productContainerDbFull.ProductContainers = selectedContainers;
        return _productContainerDbFull;
    }

    //partial view of the datatable for containers
    [HttpGet]
    public IActionResult PartialViewTable()
    {
        return PartialView("PartialView", _productContainerDbFull.ProductContainers);
    }

    //gets a list of available vendors based on product selected
    public IActionResult VendorsByProductId(int Id)
    {
        var resultsByProductId = _context.ProductVendorMapping.Where(x => x.ProductId == Id);
        var vendorModel = _context.Vendor.Where(x => !resultsByProductId.Any(y => y.VendorId == x.VendorId));
        if (Id == 0)
        {
            var nulllist = _context.Vendor.Where(x => resultsByProductId.Any(y => y.VendorId == x.VendorId));
            return Json(nulllist);
        }
        return Json(vendorModel);
    }
    //updates the product list after altering the database 
    public IActionResult VPMList()
    {
        var vendorModel = _context.ProductVendorMapping.Include(x => x.Product).Include(x => x.Vendor).Where(x => !_context.ProductContainer.Any(y => y.ProductVendorMappingId.ToString().Equals(x.ProductVendorMappingId.ToString()))).OrderBy(x => x.Product.Sku);
        return Json(vendorModel);
    }
    //updates the vendor list after altering the database
    public IActionResult VPMVendorList()
    {
        var vendorModel = _context.Vendor.Where(x => _context.ProductVendorMapping.Any(y => y.VendorId == x.VendorId));
        return Json(vendorModel);
    }

    public IActionResult DownloadBarcode(int id)
    {
        using MemoryStream ms = new();
        var b = new Barcode
        {
            IncludeLabel = true,
            LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
        };
        using var bitmap = SKBitmap.FromImage(b.Encode(BarcodeStandard.Type.Code128B, id.ToString()));
        bitmap.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(ms);
        string fileName = "barcode.jpg";
        return File(ms.ToArray(), "image/jpeg", fileName);
    }
}
