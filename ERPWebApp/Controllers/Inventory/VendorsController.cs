using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Inventory
{
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.InventoryBasic
            + ","
            + RoleList.ShippingBasic
            + ","
            + RoleList.ExternalUser
    )]
    [CwaFeatureGate(CwaFeatures.INVENTORY)]
    [AutoValidateAntiforgeryToken]
    public class VendorsController : Controller
    {
        private readonly IVendorService _vendorService;
        private static List<Vendor> _vendorDbFull = new();
        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // GET: Vendors
        public async Task<IActionResult> Index()
        {
            var query = (IQueryable<Vendor> vendor) =>
            {
                if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
                    vendor = vendor.Where(s => !s.IsExternal || s.IsExternal);
                else if (User.IsInRole(RoleList.ExternalUser))
                    vendor = vendor.Where(s => s.IsExternal);
                else
                    vendor = vendor.Where(s => !s.IsExternal);
                return vendor.OrderBy(x => x.VendorName);
            };

            List<Vendor> listvendor = await _vendorService.GetListAsync(query);
            return View(listvendor);
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public List<Vendor> ToggleActive(bool id)
        {
            _vendorDbFull = _vendorService.GetList(x => x.IsActive == id);
            return _vendorDbFull;
        }

        [HttpGet]
        public IActionResult PartialViewTableShow()
        {
            return PartialView("PartialIndex", _vendorDbFull);
        }

        // GET: Vendors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var vendor = await _vendorService.GetAsync(x => x.VendorId == id.Value);
            return vendor == null ? NotFound() : View(vendor);
        }

        // GET: Vendors/Create
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        [HttpPost]
        
        public async Task<IActionResult> Create(
            [Bind(
                "VendorId,VendorNumber,VendorName,Notes,ContactName,PhoneNumber,BusinessEmail,Fax,Website,Address1,Address2,City,State,PostalCode,Country,IsActive,IsExternal"
            )]
                Vendor vendor,
            int? nirfFormId
        )
        {
            if (!ModelState.IsValid)
                return View(vendor);

            var IsExist = await _vendorService.IsExistsAsync(x => x.VendorName == vendor.VendorName);
            if(IsExist == true)
            {
                ModelState.AddModelError("VendorName", "Vendor name is already exist");
                return View(vendor);
            }
            vendor.LastModified = DateTime.Now;
            await _vendorService.AddAsync(vendor);

            //if the request came with a nirf form id bring them back to where they came from instead of the index page of vendors.
            if (nirfFormId != null)
            {
                return RedirectToAction("Edit", "NirfForms", new { id = nirfFormId });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Vendors/Edit/5
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vendor = await _vendorService.GetAsync(x => x.VendorId == id.Value);
            return vendor == null ? NotFound() : View(vendor);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        [HttpPost]
        
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "VendorId,VendorNumber,VendorName,Notes,ContactName,PhoneNumber,BusinessEmail,Fax,Website,Address1,Address2,City,State,PostalCode,Country,IsActive,IsExternal"
            )]
                Vendor vendor
        )
        {
            if (id != vendor.VendorId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vendor);

            try
            {
                vendor.LastModified = DateTime.Now;
                await _vendorService.UpdateAsync(vendor);
            }
            catch (DbUpdateConcurrencyException)
            {
                var IsExist = await _vendorService.IsExistsAsync(x => x.VendorId == vendor.VendorId);
                if (!IsExist)
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Vendors/Delete/5
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var vendor = await _vendorService.GetAsync(x => x.VendorId == id.Value);
            return vendor == null ? NotFound() : View(vendor);
        }

        // POST: Vendors/Delete/5
        [Authorize(
            Roles = RoleList.Administrator
                + ","
                + RoleList.InventoryManager
                + ","
                + RoleList.ShippingManager
        )]
        [HttpPost, ActionName("Delete")]
        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vendorService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
