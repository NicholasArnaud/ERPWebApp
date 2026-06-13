using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Inventory;

[Authorize(
    Roles = RoleList.Administrator
        + ","
        + RoleList.Manager
        + ","
        + RoleList.ProductionBasic
        + ","
        + RoleList.InventoryBasic
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class InventoryRequestFormsController(
    ISubCategoryService subCategoryService,
    IEmployeeService employeeService,
    IProductService productService,
    IStocksService stocksService,
    IInventoryRequestFormService inventoryRequestFormService,
    ILocationService locationService,
    IOrderService orderService
) : Controller
{
    // GET: InventoryRequestForms
    /// <summary>
    /// Membrane can only view active forms while Production and others can view the limited history
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        if (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.Manager)
            || User.IsInRole(RoleList.ProductionBasic)
            || User.IsInRole(RoleList.InventoryBasic)
        )
        {
            ViewBag.permission = "yes";
        }
        else
        {
            ViewBag.permission = "no";
        }

        SubCategoryList(null);

        return View();
    }

    private void SubCategoryList(int? selectedId)
    {
        var subCategoryList = subCategoryService.GetList(
            (IQueryable<SubCategory> s) => s.Where(x=>x.IsActive)
                .Select(x=> new SelectListItem{ Value = x.SubCategoryId.ToString(), Text = x.Description })
        );
        ViewData["SubCategoryList"] = new SelectList(subCategoryList, "Value", "Text", selectedId);
    }

    // GET: InventoryRequestForms/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryRequestForm = await inventoryRequestFormService.GetAsync(
            m => m.InventoryRequestFormId == id,
            [
                i => i.RequestedEmployee,
                i => i.PickedEmployee,
                i => i.Products,
                i => i.Stocks
            ]
        );

        if (inventoryRequestForm == null)
        {
            return NotFound();
        }
        return View(inventoryRequestForm);
    }
    private async Task loadOrderProducts(string orderNumber, int productId)
    {
        if (orderNumber == null)
        {
            var allProducts = await productService.GetNonProductionProducts();

            var productList = allProducts.Select(p => new
            {
                id = p.ProductId, 
                text = p.Sku + " | " + p.Description 
            }).ToList();

            ViewData["Products"] = new SelectList(
                productList,
                "id",
                "text",
                productId
            );
        }
        else
        {
            ViewData["Orders"] = new SelectList(
                new List<object> { new { ordernumber = orderNumber } },
                "ordernumber",
                "ordernumber",
                orderNumber
            );

            var products = await orderService.GetOrderProducts(orderNumber);

            var productList = products.Select(o => new
            {
                id = o.ProductId,
                text = o.Sku + " | " + o.Description
            }).ToList();

            ViewData["Products"] = new SelectList(
                productList,
                "id",
                "text",
                productId
            );
        }
    }

    // GET: InventoryRequestForms/Create
    /// <summary>
    /// Only Production and upper roles can create new request forms.
    /// </summary>
    /// <returns></returns>
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    public async Task<IActionResult> Create()
    {

        var selectListEmployees = await employeeService.GetListAsync(
            (IQueryable<Employee> query) => query
                .Where(i => i.JobStatus != JobStatus.Terminated)
                .Select(c => new SelectListItem
                {
                    Value = c.EmployeeId.ToString(),
                    Text = c.FullName + " - " + c.EmployeeReferenceNumber
                })
        );

        var locations = await locationService.GetListAsync(
            x => x.IsActive && x.Type == LocationType.ReceiveOnly,
            orderSelectors: [
                x=>x.LocationName
            ]
        );

        ViewData["RequestedEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text");
        ViewData["PickedEmployeeId"] = new SelectList(selectListEmployees, "Value", "Text");
        ViewData["ToLocations"] = new SelectList(locations, "LocationId", "LocationName");
        return View();
    }

    public async Task<JsonResult> GetOrderNumbers(string term)
    {
        var orderNumbers = await orderService.GetListAsync(
          (IQueryable<Order> query) => query
               .Where(x => x.orderNumber.ToLower().Contains(term.ToLower()))
               .OrderBy(o => o.orderNumber)
               .Take(10)
                .Select(o => new
                {
                    id = o.orderNumber,
                    text = o.orderNumber
                })
         );
        return new JsonResult(orderNumbers);

    }

    public async Task<JsonResult> GetProducts(string orderNumber = null)
    {
        List<Product> products;

        if (!string.IsNullOrEmpty(orderNumber))
        {          
            products = await orderService.GetOrderProducts(orderNumber);
        }
        else
        {
            products = await productService.GetNonProductionProducts();              
        }

        var productList = products.Select(o => new
        {
            id = o.ProductId,
            text = o.Sku + " | " + o.Description
        }).ToList();

        return new JsonResult(productList);
    }

    // POST: InventoryRequestForms/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    [HttpPost]
    
    public async Task<IActionResult> Create(
        [Bind(
            "InventoryRequestFormId,ProductId,QuantityNeeded,PickReason,RequestedByEmployeeId,OrderNumber,ToLocationId,ReasonExplanation"
        )]
            InventoryRequestForm inventoryRequestForm
    )
    {
        if (inventoryRequestForm.ToLocationId == 0)
        {
            ModelState.AddModelError(nameof(InventoryRequestForm.ToLocationId), "To Location field is required.");
        }
        if (inventoryRequestForm.PickReason == "Operator Error" && inventoryRequestForm.ReasonExplanation == null)
        {
            ModelState.AddModelError(nameof(InventoryRequestForm.ReasonExplanation), "Plaese explain the reason");
        }

        if (ModelState.IsValid)
        {
            inventoryRequestForm.RequestedByUser = User.Identity.Name;
            inventoryRequestForm.CreatedDate = DateTime.UtcNow;

            await inventoryRequestFormService.AddAsync(inventoryRequestForm);

            return RedirectToAction(nameof(Index));
        }

        var selectListEmployees = await employeeService.GetListAsync(
            (IQueryable<Employee> query) => query
                .Where(i => i.JobStatus != JobStatus.Terminated)
                .Select(c => new SelectListItem
                {
                    Value = c.EmployeeId.ToString(),
                    Text = c.FullName + " - " + c.EmployeeReferenceNumber
                })
        );

        ViewData["RequestedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.RequestedByEmployeeId
        );

        ViewData["PickedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.PickedByEmployeeId
        );

        var locations = await locationService.GetListAsync(
            x => x.IsActive && x.Type == LocationType.ReceiveOnly,
            orderSelectors: [
                x=>x.LocationName
            ]
        );
        ViewData["ToLocations"] = new SelectList(locations, "LocationId", "LocationName", inventoryRequestForm.ToLocationId);

        await loadOrderProducts(inventoryRequestForm.OrderNumber, inventoryRequestForm.ProductId);

        return View(inventoryRequestForm);
    }

    // GET: InventoryRequestForms/Edit/5
    /// <summary>
    /// Before an order is picked, production and higher roles can edit their changes.
    /// Membrane can modify their changes until the form has been set to be recieved.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryRequestForm = await inventoryRequestFormService.GetAsync(x => x.InventoryRequestFormId == id);

        if (inventoryRequestForm == null)
        {
            return NotFound();
        }

        var employeeQuery = (IQueryable<Employee> query) => query.Where(
            i => i.JobStatus != JobStatus.Terminated
        ).Select(
            c => new SelectListItem
            {
                Value = c.EmployeeId.ToString(),
                Text = c.FullName + " - " + c.EmployeeReferenceNumber
            }
        );

        var selectListEmployees = employeeService.QueryFilter(employeeQuery).ToList();

        var stockQuery = (IQueryable<Stock> query) => query.Where(
            i =>
                i.ProductId == inventoryRequestForm.ProductId
                && i.TotalAvailable > 0
                && i.Location.Type == LocationType.PickOnly
        )
        .Include(i => i.Location)
        .Include(i => i.Location.Sites)
        .Select(
            i => new SelectListItem
            {
                Value = i.StockId.ToString(),
                Text = i.Location.LocationName
            }
        ).Distinct();


        var selectListLocations = await stocksService.QueryFilter(stockQuery).ToListAsync();

        var locations = locationService.GetList(
            x => x.IsActive && x.Type == LocationType.ReceiveOnly,
            orderSelectors: [
                x=>x.LocationName
            ]
        );

        ViewData["RequestedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.RequestedByEmployeeId
        );
        ViewData["PickedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.PickedByEmployeeId
        );
        ViewData["StockId"] = new SelectList(
            selectListLocations,
            "Value",
            "Text",
            inventoryRequestForm.StockId
        );
        ViewData["ToLocations"] = new SelectList(locations, "LocationId", "LocationName", inventoryRequestForm.ToLocationId);
        await loadOrderProducts(inventoryRequestForm.OrderNumber, inventoryRequestForm.ProductId);

        return View(inventoryRequestForm);
    }

    // POST: InventoryRequestForms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    [HttpPost]
    
    public async Task<IActionResult> Edit(
        int id,
        [Bind(
            @"InventoryRequestFormId,ProductId,QuantityNeeded,RequestedByUser,RequestedByEmployeeId,
                CreatedDate,PickReason,IsPicked,PickedByUser,PickedByEmployeeId,PickedDate,IsFromExtrasLocation,
                StockId,FromLocation,IsReceived,ReceivedDate,ToLocationId,OrderNumber,ReasonExplanation"
        )]
            InventoryRequestForm inventoryRequestForm
    )
    {
        if (id != inventoryRequestForm.InventoryRequestFormId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (inventoryRequestForm.IsPicked)
                {
                    if (!inventoryRequestForm.IsFromExtrasLocation)
                    {
                        inventoryRequestForm.Stocks = await stocksService.GetAsync(
                            i => i.StockId == inventoryRequestForm.StockId
                        );

                        var selectedStockLocation = await locationService.GetAsync(
                                i => i.LocationId == inventoryRequestForm.Stocks.LocationId
                            );

                        if (inventoryRequestForm.FromLocation != selectedStockLocation.LocationName)
                        {
                            inventoryRequestForm.FromLocation = selectedStockLocation.LocationName;
                        }
                    }

                    inventoryRequestForm.PickedByUser = User.Identity.Name;
                    inventoryRequestForm.PickedDate = DateTime.UtcNow;
                }

                await inventoryRequestFormService.UpdateAsync(inventoryRequestForm);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = inventoryRequestFormService.IsExists(
                    e => e.InventoryRequestFormId == inventoryRequestForm.InventoryRequestFormId
                );
                if (!exists)
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

        var employeeQuery = (IQueryable<Employee> query) => query.Where(
            i => i.JobStatus != Models.Company.JobStatus.Terminated
        ).Select(
            c => new SelectListItem
            {
                Value = c.EmployeeId.ToString(),
                Text = c.FullName + " - " + c.EmployeeReferenceNumber
            }
        );

        var selectListEmployees = employeeService.QueryFilter<SelectListItem>(employeeQuery).ToList();

        var stocks = stocksService.GetList(
            i => i.ProductId == inventoryRequestForm.ProductId,
            includes: [
                i => i.Location
            ]
        );

        ViewData["RequestedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.RequestedByEmployeeId
        );
        ViewData["PickedEmployeeId"] = new SelectList(
            selectListEmployees,
            "Value",
            "Text",
            inventoryRequestForm.PickedByEmployeeId
        );

        ViewData["StockId"] = new SelectList(
            stocks,
            "StockId",
            "LocationName",
            inventoryRequestForm.StockId
        );

        await loadOrderProducts(inventoryRequestForm.OrderNumber, inventoryRequestForm.ProductId);

        return View(inventoryRequestForm);
    }

    // GET: Products/Delete/5
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryRequestForm = await inventoryRequestFormService.GetAsync(e => e.InventoryRequestFormId == id);

        if (inventoryRequestForm == null)
        {
            return NotFound();
        }

        return View(inventoryRequestForm);
    }

    // POST: Products/Delete/5
    [Authorize(
        Roles = RoleList.Administrator
            + ","
            + RoleList.Manager
            + ","
            + RoleList.ProductionBasic
            + ","
            + RoleList.InventoryBasic
    )]
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await inventoryRequestFormService.RemoveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Function to close out and end the Inventory Pick Request process.
    /// Calling this means pickers grabbed the item requested and the requester acknowledges that they received the item.
    /// </summary>
    /// <param name="InventoryRequestFormId">Id of chosen row in InventoryRequestForm table</param>
    /// <returns></returns>
    public async Task<IActionResult> CloseInventoryRequestForm(int? InventoryRequestFormId)
    {
        if (InventoryRequestFormId == null)
        {
            return NotFound();
        }
        try
        {
            var IRFToBeUpdated = await inventoryRequestFormService.GetAsync(
                m => m.InventoryRequestFormId == InventoryRequestFormId
            );

            if (IRFToBeUpdated == null)
            {
                return NotFound();
            }

            //gets where the stock is coming from
            var getFromStock = await stocksService.GetAsync(
                x => x.StockId == IRFToBeUpdated.StockId,
                [
                    x => x.Products
                ]
            );

            // check to make sure the move wont put the stock into the negatives.
            if ((getFromStock.TotalAvailable - IRFToBeUpdated.QuantityNeeded) < 0)
            {
                var message = "An ERROR has occurred. Total Quantity would be less than 0 for Product: ";
                message += $"{getFromStock.Products.Sku}, From Location: {IRFToBeUpdated.FromLocation}";
                throw new Exception(message);
            }

            var utcNow = DateTime.UtcNow;
            getFromStock.TotalAvailable -= IRFToBeUpdated.QuantityNeeded;
            IRFToBeUpdated.IsReceived = true;
            IRFToBeUpdated.ReceivedDate = utcNow;

            //gets where the stock is going
            var getToStock = await stocksService.GetAsync(
                x => x.Location.LocationId == IRFToBeUpdated.ToLocationId
                    && x.ProductId == IRFToBeUpdated.ProductId
            ) ??
            new Stock
            {
                ProductId = getFromStock.ProductId,
                LocationId = IRFToBeUpdated.ToLocationId,
                ModifyByUser = User.Identity.Name,
                ModifyDate = utcNow
            };

            getToStock.TotalAvailable += IRFToBeUpdated.QuantityNeeded;

            //creates a history of the stock transfer
            var history = new MoveStockHistory
            {
                ToStock = getToStock,
                FromStock = getFromStock,
                Sku = getFromStock.Products.Sku,
                Quantity = IRFToBeUpdated.QuantityNeeded,
                Type = ActionType.Transfer,
                EmployeeName = User.Identity.Name,
                DateTime = utcNow
            };

            await inventoryRequestFormService.CloseInventoryRequestAsync(IRFToBeUpdated, getFromStock, getToStock, history);
        }
        catch
        {
            ModelState.AddModelError(
                "",
                "Unable to save changes. Try again, and if the problem persists see your system administrator."
            );
        }
        return RedirectToAction("Index");
    }

    [HttpPost("LoadTable")]
    [IgnoreAntiforgeryToken]
    public ActionResult LoadTable(String orderNumber, int? subCategory, string dateFilter)
    {
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = 0;

        var query = (IQueryable<InventoryRequestForm> Query) => Query.Include(i => i.RequestedEmployee)
            .Include(i => i.PickedEmployee)
            .Include(i => i.Products)
            .Include(i => i.Products.SubCategory)
            .OrderByDescending(i => i.InventoryRequestFormId)
            .Select(t => new inventoryRequestModel
            {
                id = t.InventoryRequestFormId,
                sku = t.Products.Sku,
                description = t.Products.Description,
                quantityNeeded = t.QuantityNeeded,
                pickedReason = t.PickReason,
                requestedByUser = t.RequestedByUser,
                requestedEmployee = t.RequestedEmployee.FullName,
                pickedEmployee = t.PickedEmployee.FullName,
                isPicked = t.IsPicked,
                isReceived = t.IsReceived,
                createdDate = t.CreatedDate,
                pickedDate = t.PickedDate,
                recievedDate = t.ReceivedDate,
                orderNumber = t.OrderNumber,
                subCategoryId = t.Products.SubCategory.SubCategoryId
            });

        var applicationDbContext = inventoryRequestFormService.QueryFilter<inventoryRequestModel>(query);

        //filters through a user search string (miassive for amount of columns to search through normally done behind the scenes)
        if (!string.IsNullOrEmpty(searchValue))
        {
            applicationDbContext = applicationDbContext
                .Where(
                    x =>
                        x.sku.Contains(searchValue)
                        || x.pickedReason.Contains(searchValue)
                        || x.description.Contains(searchValue)
                        || x.quantityNeeded.ToString() == searchValue
                        || x.requestedByUser.Contains(searchValue)
                        || x.requestedEmployee.Contains(searchValue)
                        || x.pickedEmployee.Contains(searchValue)
                        || x.createdDate.ToString().Contains(searchValue)
                )
                .OrderByDescending(x => x.id);
        }

        if (!string.IsNullOrEmpty(dateFilter))
        {
            string[] dates = dateFilter.Split(" - ", 2);
            DateTime first = DateTime.Parse(dates[0]);
            DateTime second = DateTime.Parse(dates[1]);
            applicationDbContext = applicationDbContext
                .Where(x => x.createdDate >= first && x.createdDate <= second)
                .OrderByDescending(x => x.createdDate);
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            applicationDbContext = applicationDbContext
                .Where(x => x.orderNumber == orderNumber)
                .OrderByDescending(x => x.createdDate);
        }

        if (subCategory != null)
        {
            applicationDbContext = applicationDbContext
                .Where(x => x.subCategoryId == subCategory)
                .OrderByDescending(x => x.createdDate);
        }

        //Column Sort Direction
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.sku)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Picked Reason":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.pickedReason)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Description":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.description)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Quantity Needed":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.quantityNeeded)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Requesting Station/User":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.requestedByUser)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Requested By EmployeeId":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.requestedEmployee)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Picked By Employee":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.pickedEmployee)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Is Picked":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.isPicked)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Is Received":
                        applicationDbContext = applicationDbContext
                            .OrderBy(x => x.isReceived)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Created Date":
                        applicationDbContext = applicationDbContext.OrderBy(x => x.createdDate);
                        break;
                }
            }
            else if (sortColumnDirection == "desc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.sku)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Picked Reason":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.pickedReason)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Description":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.description)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Quantity Needed":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.quantityNeeded)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Requesting Station/User":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.requestedByUser)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Requested By EmployeeId":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.requestedEmployee)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Picked By Employee":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.pickedEmployee)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Is Picked":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.isPicked)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Is Received":
                        applicationDbContext = applicationDbContext
                            .OrderByDescending(x => x.isReceived)
                            .ThenByDescending(x => x.createdDate);
                        break;
                    case "Created Date":
                        applicationDbContext = applicationDbContext.OrderByDescending(
                            x => x.createdDate
                        );
                        break;
                }
            }
        }

        int recordsTotal = 0;
        recordsTotal = applicationDbContext.Count();

        //sets page size for the user
        if (length != null)
        {
            pageSize = length == "-1" ? recordsTotal : Convert.ToInt32(length);
        }

        //sets the start point to the skip point if your on a page
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }


        var data = applicationDbContext.Skip(skip).Take(pageSize).ToList();

        var jsonData = new
        {
            draw,
            data,
            recordsTotal,
            recordsFiltered = recordsTotal
        };

        return Ok(jsonData);
    }


    public class inventoryRequestModel
    {
        public int? id { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public string pickedReason { get; set; }
        public int? quantityNeeded { get; set; }
        public string orderNumber { get; set; }
        public string requestedByUser { get; set; }
        public string requestedEmployee { get; set; }
        public string pickedEmployee { get; set; }
        public bool? isPicked { get; set; }
        public bool? isReceived { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? pickedDate { get; set; }
        public DateTime? recievedDate { get; set; }
        public int? subCategoryId { get; set; }
    }
}
