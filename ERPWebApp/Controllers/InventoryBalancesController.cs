using ERPWebApp.Data;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;

namespace ERPWebApp.Controllers
{
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.InventoryBasic
            + ","
            + RoleList.ShippingBasic
    )]
    [CwaFeatureGate(CwaFeatures.INVENTORY)]
    [AutoValidateAntiforgeryToken]
    public class InventoryBalancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderShippingService _orderShippingService;
        private static DateTime _date = TimeZoneInfo.ConvertTime(
            DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );

        public InventoryBalancesController(ApplicationDbContext context, IOrderShippingService orderShippingService)
        {
            _context = context;
            _orderShippingService = orderShippingService;
        }

        // GET: InventoryBalances
        public async Task<IActionResult> Index()
        {
            var currentDate = TimeZoneInfo.ConvertTime(
                DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            );
            var diff1 = currentDate.Subtract(_date);
            ViewData["TimeWarning"] = "";
            //warns after 6 hours
            if (diff1.Hours > 5)
            {
                ViewData["TimeWarning"] = " Last Refreshed" + ": " + diff1.Hours + " Hour(s) Ago";
            }

            // handle 3 cases, grab data for all inventoryBalances, only internal, or only external
            var allInventoryBalances = await (from a in _context.InventoryBalance
                                              join p in _context.Product on a.Sku equals p.Sku
                                              select new BalanceModel
                                              {
                                                  InventoryBalanceId = a.InventoryBalanceId,
                                                  Sku = a.Sku,
                                                  Description = a.Description,
                                                  TotalAvailable = a.TotalAvailable,
                                                  PendingShipStationOrders = a.PendingShipStationOrders,
                                                  OrderDifference = a.OrderDifference,
                                                  IsExternalSiteInventory = a.IsExternalSiteInventory,
                                                  MinCost = p.MinInventory
                                              }).ToListAsync();//  _context.InventoryBalance.Join(_context.Product,).ToListAsync();

            if (User.IsInRole(RoleList.Administrator) || User.IsInRole(RoleList.ExternalViewer))
            {
                return View(allInventoryBalances);
            }

            if (User.IsInRole(RoleList.ExternalUser))
            {
                var externalBalances = allInventoryBalances.Where(e => e.IsExternalSiteInventory);
                return View(externalBalances);
            }

            // implied else case -> if user is not admin/manager or external, then they are an internal user
            var internalBalances = allInventoryBalances.Where(e => !e.IsExternalSiteInventory);
            return View(internalBalances);
        }
        public class BalanceModel
        {
            public int InventoryBalanceId { get; set; }
            public string Sku { get; set; }
            public string Description { get; set; }

            [Display(Name = "Total Available")]
            public int TotalAvailable { get; set; }

            [Display(Name = "ShipStation Orders")]
            public int PendingShipStationOrders { get; set; }

            [Display(Name = "Order Difference")]
            public int OrderDifference { get; set; }

            [Display(Name = "External Site?")]
            [DefaultValue(false)]
            public bool IsExternalSiteInventory { get; set; }

            public int MinCost { get; set; }

        }
        // GET: InventoryBalances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryBalance = await _context.InventoryBalance.FirstOrDefaultAsync(
                m => m.InventoryBalanceId == id
            );
            if (inventoryBalance == null)
            {
                return NotFound();
            }

            return View(inventoryBalance);
        }

        // GET: InventoryBalances/Create
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

        // POST: InventoryBalances/Create
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
                "InventoryBalanceId,Sku,Description,TotalAvailable,PendingShipStationOrders,OrderDifference"
            )]
                InventoryBalance inventoryBalance
        )
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventoryBalance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(inventoryBalance);
        }

        // GET: InventoryBalances/Edit/5
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
            {
                return NotFound();
            }

            var inventoryBalance = await _context.InventoryBalance.FindAsync(id);
            if (inventoryBalance == null)
            {
                return NotFound();
            }

            return View(inventoryBalance);
        }

        // POST: InventoryBalances/Edit/5
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
                "InventoryBalanceId,Sku,Description,TotalAvailable,PendingShipStationOrders,OrderDifference"
            )]
                InventoryBalance inventoryBalance
        )
        {
            if (id != inventoryBalance.InventoryBalanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventoryBalance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryBalanceExists(inventoryBalance.InventoryBalanceId))
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

            return View(inventoryBalance);
        }

        // GET: InventoryBalances/Delete/5
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
            {
                return NotFound();
            }

            var inventoryBalance = await _context.InventoryBalance.FirstOrDefaultAsync(
                m => m.InventoryBalanceId == id
            );
            if (inventoryBalance == null)
            {
                return NotFound();
            }

            return View(inventoryBalance);
        }

        // POST: InventoryBalances/Delete/5
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
            var inventoryBalance = await _context.InventoryBalance.FindAsync(id);
            _context.InventoryBalance.Remove(inventoryBalance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryBalanceExists(int id)
        {
            return _context.InventoryBalance.Any(e => e.InventoryBalanceId == id);
        }

        [Authorize(Roles = RoleList.Administrator + "," + RoleList.Manager)]
        [HttpPost, ActionName("Reload")]
        public async Task<ActionResult> Reload()
        {

            var conn = _context.Database.GetDbConnection();
            Console.WriteLine("Order Balance is now Reloading");
            await conn.OpenAsync();
            try
            {
                using var command = conn.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "UpdateInventoryBalance";
                command.CommandTimeout = 200;
                DbDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    Console.Write("it has rows");
                }
            }
            finally
            {
                conn.Close();
            }
            _date = TimeZoneInfo.ConvertTime(
                DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            );
            ViewData["TimeWarning"] = "";
            return RedirectToAction(nameof(Index));

        }
    }
}
