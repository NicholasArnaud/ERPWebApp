using BarcodeStandard;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Models.PurchaseOrders.StockLocationEdit;
using ERPWebApp.Services;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel;
using System.Globalization;
using System.Security.Claims;
using static ERPWebApp.Models.PurchaseOrders.PurchaseOrderViewModel;

namespace ERPWebApp.Controllers.PurchaseOrders;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialBasic + "," + RoleList.ShippingBasic + "," + RoleList.Manager + "," + RoleList.Developer)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[AutoValidateAntiforgeryToken]
public class PurchaseOrdersController(
    IProductPurchaseOrderService productPurchaseOrderService,
    IProductService productService,
    IPurchaseOrderService purchaseorderService,
    IPurchaseOrderFilesMappingService purchaseOrderFilesMappingService,
    IProductVendorMappingService productVendorMappingService,
    IShippingMethodService shippingMethodService,
    IShippingProviderService shippingProviderService,
    IVendorService vendorService,
    IStocksService stocksService,
    IProductPurchaseOrderStockMappingService productPurchaseOrderStockMappingService,
    ILocationService locationService,
    IMoveStockHistoryService moveStockHistoryService,
    IFilesService fileService,
    UserManager<IdentityUser> userManager,
    IEmployeeService employeeService,
    IOrderBatchService orderBatchService
) : Controller
{
    public List<Product> Products { get; set; } = [];
    public static PurchaseOrderViewModel _PurchaseOrderDb = new();
    public static PurchaseOrderViewModel _PurchaseOrderDbPar = new();

    /// <summary>
    /// index of the purchase order system
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var existingIds = await productPurchaseOrderService.GetListAsync(
            (q) => q.Include(x => x.ProductVendorMapping).Select(x => x.ProductVendorMapping.ProductId)
        );

        var productList = await productService.GetListAsync(
            (p) => p.Where(
                    x => existingIds.Contains(x.ProductId)
                )
                .OrderBy(x => x.Sku)
                .Select(x => new Product { ProductId = x.ProductId, Sku = x.Sku + " || " + x.Description })
        );

        ViewData["ProductList"] = new SelectList(productList, "ProductId", "Sku");
        ViewData["StatusList"] = new SelectList(
            Enum.GetValues(typeof(Status)).Cast<Status>().Select(
                v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }
            ).ToList(), "Text", "Text"
        );

        return View();
    }


    /// <summary>
    /// links the user to the create page
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer + "," + RoleList.ShippingManager)]
    public async Task<IActionResult> Create()
    {
        var vendorMappings = await productVendorMappingService.GetListAsync(
            (q) => q.Where(x => x.IsActive).Include(v => v.Vendor).GroupBy(x => x.Vendor).Select(x => x.First())
        );

        ViewData["Vendor"] = new SelectList(vendorMappings, "VendorId", "Vendor.VendorName");
        
        var utcNow = DateTime.UtcNow;

        if (!await shippingMethodService.IsExistsAsync())
        {
            await shippingProviderService.AddAsync(
                new ShippingProvider
                {
                    ShippingProviderName = "To Be Determined",
                    ModifyDate = utcNow,
                    ModifyByUser = "Admin",
                    IsActive = true
                }
            );

            await shippingMethodService.AddAsync(
                new ShippingMethod
                {
                    ShippingMethodName = "To Be Determined",
                    ModifyDate = utcNow,
                    ModifyByUser = "Admin",
                    IsActive = true,
                    ShippingProviderId = 3
                }
            );
        }

        ViewData["ShippingPro"] = new SelectList(await shippingProviderService.GetListAsync(x => x.IsActive), "ShippingProviderId", "ShippingProviderName");

        var getShippingPro = await shippingProviderService.GetAsync(x => x.IsActive);

        var shippingMethods = await shippingMethodService.GetListAsync(
            x => x.ShippingProvider.ShippingProviderId == getShippingPro.ShippingProviderId,
            orderSelectors:
            [
                o => o.ShippingMethodName
            ],
            includes:
            [
                x => x.ShippingProvider
            ]
        );

        ViewData["ShippingMeth"] = new SelectList(shippingMethods, "ShippingMethodId", "ShippingMethodName");
        _PurchaseOrderDb.PurchaseOrderSingle = new PurchaseOrder();
        ViewData["PONumber"] = await NewOrderNumber(false);
        return View();
    }

    /// <summary>
    /// saves the created purchase order and the product purchase orders attached
    /// </summary>
    /// <param name="purchaseOrderViewModel">view model for the purchase orders</param>
    /// <param name="productskus">a string of skus for the products</param>
    /// <param name="productaverages">a string of average cost for the products </param>
    /// <param name="productcosts">a string of costs for the products</param>
    /// <param name="idvendor">the vendor id for the purchase order</param>
    /// <param name="idshippingmeth">the shipping method id for the purchase order</param>
    /// <param name="idshippingpro">the shipping provider id for the purchase order</param>
    /// <param name="productquantity">the quantity of products</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer + "," + RoleList.ShippingManager)]
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("PurchaseOrderSingle, PurchaseOrderSingle.PurchaseOrderId,PurchaseOrderSingle.ShippingMethodId,PurchaseOrderSingle.ShippingProviderId," +
      "PurchaseOrderSingle.VendorId,PurchaseOrderSingle.PurchaseOrderNumber,PurchaseOrderSingle.OrderDate,PurchaseOrderSingle.EstimatedDate," +
      "PurchaseOrderSingle.POStatus,PurchaseOrderSingle.ReferenceNumber, PurchaseOrderSingle.Notes,PurchaseOrderSingle.ShippingCost,PurchaseOrderSingle.GrandTotal," +
      "PurchaseOrderSingle.Discount,PurchaseOrderSingle.ShippingTax,PurchaseOrderSingle.OtherCost,PurchaseOrderSingle.IsActive")]
        PurchaseOrderViewModel purchaseOrderViewModel, string productskus, string productaverages, string productcosts, string idvendor, string idshippingmeth,
        string idshippingpro, string productquantity, IFormFile upload, string fileName, string discountPercentage, string totalCost, string expectedDates, string miscItems, string batchItems
    )
    {

        List<MiscProduct> miscItemList = new List<MiscProduct>();
        if (!string.IsNullOrWhiteSpace(miscItems) && !IsValidJson(miscItems))
        {
            ModelState.AddModelError("miscItems", "The miscItems parameter(s) are invalid");
            miscItems = miscItems.Replace("N/A", DateTime.UtcNow.Date.ToString());
           
        }

        var settings = new JsonSerializerSettings
        {
            DateFormatString = "M/d/yyyy"
        };

        miscItemList = JsonConvert.DeserializeObject<List<MiscProduct>>(miscItems, settings);

    
        purchaseOrderViewModel.MiscProducts = miscItemList;

        ModelState.Keys.Where(k => k.StartsWith("PurchaseOrderSingle.Vendor."))
            .ToList()
            .ForEach(key => ModelState.Remove(key));

        var vendors = await productVendorMappingService.GetVendorsAsync();
        ViewData["Vendor"] = new SelectList(vendors, "VendorId", "Vendor.VendorName",
            purchaseOrderViewModel?.PurchaseOrderSingle?.VendorId);

        ViewData["ShippingPro"] = new SelectList(await shippingProviderService.GetAllAsync(), "ShippingProviderId", "ShippingProviderName", purchaseOrderViewModel?.PurchaseOrderSingle?.ShippingProviderId);

        var shippingMethods = await shippingMethodService.GetListAsync(
            x => x.ShippingProvider.ShippingProviderId == purchaseOrderViewModel.PurchaseOrderSingle.ShippingProviderId,
            orderSelectors:
            [
                o => o.ShippingMethodName
            ],
            includes:
            [
                x => x.ShippingProvider
            ]
        );

        ViewData["ShippingMeth"] = new SelectList(shippingMethods, "ShippingMethodId", "ShippingMethodName", purchaseOrderViewModel?.PurchaseOrderSingle?.ShippingMethodId);
        
        var utcNow = DateTime.UtcNow;
        int purchaseOrderId = 0;
        List<int> splitBatches = null;

        try
        {
            ViewData["PONumber"] = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderNumber;
            if (ModelState.IsValid)
            {
                
                if (purchaseOrderViewModel.PurchaseOrderSingle.VendorId == 0)
                {
                    ModelState.AddModelError("PurchaseOrderSingle.VendorId", "A vendor needs to be selected..");
                    UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);
                    return View(purchaseOrderViewModel);
                }

                if (purchaseOrderViewModel.PurchaseOrderSingle.ShippingProviderId == 0)
                {
                    ModelState.AddModelError("PurchaseOrderSingle.ShippingProviderId", "A Shipping Provider needs to be selected..");
                    UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);
                    return View(purchaseOrderViewModel);
                }

                if (purchaseOrderViewModel.PurchaseOrderSingle.ShippingMethodId == 0)
                {
                    ModelState.AddModelError("PurchaseOrderSingle.ShippingMethodId", "A Shipping Method needs to be selected..");
                    UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);
                    return View(purchaseOrderViewModel);
                }

                if (productskus == null)
                {
                    ModelState.AddModelError("productSUK", "No Products Were Assigned to the PO");
                    _PurchaseOrderDb.PurchaseOrderSingle = purchaseOrderViewModel.PurchaseOrderSingle;
                    UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);

                    
                    ViewData["Vendor"] = new SelectList(vendors, "VendorId", "Vendor.VendorName",
                        purchaseOrderViewModel?.PurchaseOrderSingle?.VendorId);

                    ModelState.AddModelError("productskys", "Please Add at least one product");

                    return View(purchaseOrderViewModel);
                }

                var IsExists = await purchaseorderService.IsExistsAsync(x =>
                    x.PurchaseOrderNumber == purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderNumber);

                if (IsExists)
                {
                    ModelState.AddModelError("PurchaseOrderSingle.PurchaseOrderNumber", "Unable to save changes. Duplicate Po Number.");
                    UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);
                    return View(purchaseOrderViewModel);
                }

                purchaseOrderViewModel.PurchaseOrderSingle.IsActive = true;
                purchaseOrderViewModel.PurchaseOrderSingle.POStatus = Status.Draft;
                purchaseOrderViewModel.PurchaseOrderSingle.OrderDate += utcNow.TimeOfDay;
                purchaseOrderViewModel.PurchaseOrderSingle.ModifyDate = utcNow;
                purchaseOrderViewModel.PurchaseOrderSingle.ModifyByUser = User.Identity.Name;
                purchaseOrderViewModel.PurchaseOrderSingle.Vendor = await vendorService.GetAsync(v => v.VendorId == purchaseOrderViewModel.PurchaseOrderSingle.VendorId);
                purchaseOrderViewModel.PurchaseOrderSingle.ShippingProvider = await shippingProviderService.GetAsync(sp => sp.ShippingProviderId == purchaseOrderViewModel.PurchaseOrderSingle.ShippingProviderId);
                purchaseOrderViewModel.PurchaseOrderSingle.ShippingMethod = await shippingMethodService.GetAsync(sm => sm.ShippingMethodId == purchaseOrderViewModel.PurchaseOrderSingle.ShippingMethodId);
                await purchaseorderService.AddAsync(purchaseOrderViewModel.PurchaseOrderSingle);


                purchaseOrderId = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderId;
                if (!string.IsNullOrEmpty(miscItems))
                {
                     miscItemList = JsonConvert.DeserializeObject<List<MiscProduct>>(miscItems);



                    miscItemList.ForEach(item => {
                        item.IsActive = true;
                        item.ModifyDate = utcNow;
                        item.ModifyByUser = User?.Identity?.Name;
                        item.PurchaseOrderId = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderId;
                    });

                    await purchaseorderService.AddMiscProductListAsync(miscItemList);

                }

                var splitProducts = productskus.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitProductAvg = productaverages.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitProductCosts = productcosts.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitProductQuantity = productquantity.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitProductDiscount = discountPercentage.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitProductTotalCost = totalCost.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                var splitExpectedDates = expectedDates.Split(",").Select(x => x.Trim()).Skip(1).ToArray();
                splitBatches = batchItems
                                    ?.Split(",")
                                    .Select(x =>
                                    {
                                        bool success = int.TryParse(x.Trim(), out int val);
                                        return new { success, val };
                                    })
                                    .Where(x => x.success)
                                    .Select(x => x.val)
                                    .ToList();

                List<ProductPurchaseOrder> ProductPurchaseOrderList = new();
                //save each product purchase order
                for (int i = 0; i < splitProducts.Count(); i++)
                {
                    try
                    {
                        //this is null
                        var getPurchaseOrder = await purchaseorderService.GetAsync(po => po.PurchaseOrderNumber == purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderNumber);
                        ProductPurchaseOrder productPurchaseOrder = new ProductPurchaseOrder();
                        productPurchaseOrder.PurchaseOrder = getPurchaseOrder;
                        var getProduct = await productService.GetAsync(p => p.Sku == splitProducts[i]);

                        productPurchaseOrder.ProductVendorMapping = await productVendorMappingService.GetAsync(pvm => pvm.VendorId == getPurchaseOrder.VendorId && pvm.ProductId == getProduct.ProductId && pvm.IsActive);
                        productPurchaseOrder.CustomCost = Convert.ToDecimal(splitProductCosts[i]);
                        productPurchaseOrder.TotalOrdered = int.Parse(splitProductQuantity[i]);
                        productPurchaseOrder.DiscountPercentage = decimal.Parse(splitProductDiscount[i]);
                        productPurchaseOrder.TotalProductCost = Convert.ToDecimal(splitProductTotalCost[i]);
                        productPurchaseOrder.DiscountAmount = Math.Round(productPurchaseOrder.TotalProductCost * (productPurchaseOrder.DiscountPercentage / (100 - productPurchaseOrder.DiscountPercentage)), 4);
                        productPurchaseOrder.ExpectedDate = DateTime.ParseExact(splitExpectedDates[i], settings.DateFormatString, CultureInfo.InvariantCulture);
                        if (productPurchaseOrder.CustomCost == 0)
                        {
                            productPurchaseOrder.CustomCost = Convert.ToDecimal(splitProductAvg[i]);
                        }

                        int[] siteIds = [1, 2, 48, 49];

                        var qusery = (IQueryable<Stock> product) => product.Where(
                                z => z.ProductId == getProduct.ProductId
                                     && siteIds.Contains(z.Location.SiteId)
                            )
                            .Include(x => x.Products)
                            .Include(x => x.Location)
                            .GroupBy(y => y.Products.ProductId)
                            .Select(
                                g => new Product
                                {
                                    ProductId = g.Key,
                                    StockTotalAvailable = g.Sum(i => i.TotalAvailable)
                                }
                            );

                        var productsWithStock = stocksService.QueryFilter(qusery).FirstOrDefault();

                        productPurchaseOrder.AverageCost = productsWithStock == null ? 0 : (productsWithStock.StockTotalAvailable * getProduct.Cost + productPurchaseOrder.CustomCost * productPurchaseOrder.TotalOrdered) / (productsWithStock.StockTotalAvailable + productPurchaseOrder.TotalOrdered);
                        productPurchaseOrder.TotalRecieved = 0;
                        productPurchaseOrder.ModifyByUser = User.Identity.Name;
                        productPurchaseOrder.ModifyDate = utcNow;

                        ProductPurchaseOrderList.Add(productPurchaseOrder);

                        var getPPOList = await productPurchaseOrderService.GetListAsync(
                            ppo => ppo.ProductVendorMapping.ProductId == getProduct.ProductId
                                   && ppo.PurchaseOrder.POStatus != Status.Cancelled &&
                                   ppo.PurchaseOrder.POStatus != Status.Close,
                            null,
                            includes:
                            [
                                x => x.ProductVendorMapping,
                                x => x.PurchaseOrder
                            ]
                        );

                        getProduct.OnOrder = 0;
                        foreach (var poProduct in getPPOList)
                        {
                            getProduct.OnOrder += poProduct.TotalOrdered - poProduct.TotalRecieved;
                        }

                        await productService.UpdateAsync(getProduct);
                    }
                    catch (Exception) { 
                    
                    }

                }

                if (splitBatches?.Count > 0)
                {
                    //heshan



                    await orderBatchService.UpdateOrderBatchPurchaseOrderDetailsAsync(purchaseOrderId, splitBatches);

                }

                //if (miscItemList.Count > 0)
                //{
                //    miscItemList.ForEach(item =>
                //    {
                //        item.IsActive = true;
                //        item.ModifyDate = utcNow;
                //        item.ModifyByUser = User?.Identity?.Name;
                //        item.PurchaseOrderId = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderId;
                //    });


                //    await purchaseorderService.AddMiscProductListAsync(miscItemList);
                //}

                var skuList = productskus.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .ToList();

                var productIds = ProductPurchaseOrderList
                    .Select(p => p.ProductVendorMapping.Product.ProductId)
                    .Distinct()
                    .ToList();

                var products = await productService.GetListAsync(p => productIds.Contains(p.ProductId));

                var combinedProductInfoList = (from item in ProductPurchaseOrderList
                                               join product in products on item.ProductVendorMapping.Product.ProductId equals product.ProductId into prodGroup
                                               from prod in prodGroup.DefaultIfEmpty()
                                               select new CombinedProductInfo
                                               {
                                                   Sku = prod?.Sku ?? item.ProductVendorMapping.Product.Sku,
                                                   VendorSku = item.ProductVendorMapping.VendorSku,
                                                   Description = prod?.Description ?? "-",
                                                   Quantity = item.TotalOrdered,
                                                   Cost = item.TotalProductCost,
                                                   CustomCost = item.CustomCost,
                                                   AverageCost = item.AverageCost,
                                                   TotalOrdered = item.TotalOrdered,
                                                   TotalReceived = item.TotalRecieved,
                                                   DiscountPercentage = item.DiscountPercentage,
                                                   DiscountAmount = item.DiscountAmount,
                                                   TotalProductCost = item.TotalProductCost,
                                                   ExpectedDate = item.ExpectedDate
                                               }).ToList();

                var doc = await purchaseorderService.GeneratePdfWithProductsAndMisc(
                    combinedProductInfoList,
                    purchaseOrderViewModel.PurchaseOrderSingle
                );

                // Save to file or return as response  
                var generatedFile = File(doc, "application/pdf", "report.pdf");

                #region FileUpload

                if (generatedFile != null)
                {
                    string extension = Path.GetExtension(generatedFile.FileDownloadName).ToLower();
                    if (extension == ".jpg" || extension == ".png" || extension == ".pdf")
                    {
                        fileName ??= generatedFile.FileDownloadName;
                        var type = FileType.Image;
                        if (extension == ".pdf")
                        {
                            type = FileType.Pdf;
                        }


                        var fileUrl = await fileService.UploadToAzureAsync(doc, fileName, type, "application/pdf");

                        var file = new Files
                        {
                            FileName = fileName,
                            FileType = type,
                            ContentType = "application/pdf",
                            FileUrl = fileUrl
                        };

                        await fileService.AddAsync(file);

                        await purchaseOrderFilesMappingService.AddAsync(new PurchaseOrderFilesMapping()
                        {
                            FileId = file.FileId,
                            PurchaseOrderId = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderId
                        });
                    }
                }

                if (upload != null)
                {
                    string extension = Path.GetExtension(upload.FileName).ToLower();
                    if (extension == ".jpg" || extension == ".png" || extension == ".pdf")
                    {
                        fileName ??= Path.GetFileName(upload.FileName);
                        var type = FileType.Image;
                        if (extension == ".pdf")
                        {
                            type = FileType.Pdf;
                        }


                        var fileUrl = await fileService.UploadToAzureAsync(upload, type);

                        var file = new Files
                        {
                            FileName = fileName,
                            FileType = type,
                            ContentType = upload.ContentType,
                            FileUrl = fileUrl
                        };

                        await fileService.AddAsync(file);

                        await purchaseOrderFilesMappingService.AddAsync(new PurchaseOrderFilesMapping()
                        {
                            FileId = file.FileId,
                            PurchaseOrderId = purchaseOrderViewModel.PurchaseOrderSingle.PurchaseOrderId
                        });
                    }
                }

                await purchaseorderService.AddProductPurchaseOrdersAsync(ProductPurchaseOrderList);
                #endregion

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var vendorMappings = await productVendorMappingService.GetListAsync(
                     (q) => q.Where(x => x.IsActive).Include(v => v.Vendor).GroupBy(x => x.Vendor).Select(x => x.First())
                 );
                ViewData["Vendor"] = new SelectList(vendorMappings, "VendorId", "Vendor.VendorName");
                return View(purchaseOrderViewModel);
            }
        }
        catch
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            UpdateDaynamicTableValues(productskus, productaverages, productcosts, productquantity);
            await orderBatchService.UndoBatchPOIdAssignmentAsync(purchaseOrderId, splitBatches);
            await purchaseorderService.RemoveAsync(purchaseOrderId);
            return View(purchaseOrderViewModel);
        }
    }

    /// <summary>
    /// grabs the details of the selected purchase order
    /// </summary>
    /// <param name="id">purchase order id</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialBasic + "," + RoleList.ShippingBasic + "," + RoleList.Manager + "," + RoleList.Developer)]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        _PurchaseOrderDb.PurchaseOrderSingle = await purchaseorderService.GetAsync(
            q => q.PurchaseOrderId == id,
            includes:
            [
                x => x.Vendor,
                x => x.ShippingMethod,
                x => x.ShippingProvider
            ]
        );

        if (_PurchaseOrderDb.PurchaseOrderSingle == null)
        {
            return NotFound();
        }

        _PurchaseOrderDb.ProductPurchaseOrders = await productPurchaseOrderService.GetListAsync(
            q => q.PurchaseOrderId == id,
            includes:
            [
                x => x.PurchaseOrder,
                x => x.ProductVendorMapping,
                x => x.ProductVendorMapping.Product
            ]
        );

        var productIds = _PurchaseOrderDb.ProductPurchaseOrders.Select(x => x.ProductVendorMapping.ProductId).ToList();
        _PurchaseOrderDb.productStock = await stocksService.GetListAsync(
            x => productIds.Contains(x.ProductId)
        );

        _PurchaseOrderDb.purchaseOrderFilesMappings = await purchaseOrderFilesMappingService.GetListAsync(
            x => x.PurchaseOrderId == _PurchaseOrderDb.PurchaseOrderSingle.PurchaseOrderId,
            null,
            includes:
            [
                x => x.Files
            ]
        );

        var poStatus = _PurchaseOrderDb.PurchaseOrderSingle.POStatus;
        ViewData["IsForceComplete"] = poStatus == Status.InProgress ? "yes" : "no";
        ViewData["IsComplete"] = poStatus == Status.FullyReceived ? "yes" : "no";

        _PurchaseOrderDb.ProductPurchaseOrderStockMappings = await productPurchaseOrderStockMappingService.GetListAsync(
            q => q.ProductPurchaseOrder.PurchaseOrderId == id,
            includes:
            [
                x => x.Stock,
                x => x.Stock.Products,
                x => x.Stock.Location,
                x => x.Stock.Location.Sites,
            ]
        );

        _PurchaseOrderDb.PurchaseOrderSingle.totalQty = _PurchaseOrderDb.ProductPurchaseOrders.Sum(x => x.TotalOrdered);
        _PurchaseOrderDb.detailPurchaseOrderModels = _PurchaseOrderDb.ProductPurchaseOrders
            .Select(x => new DetailPurchaseOrderModel
            {
                AverageCost = x.AverageCost,
                ProductId = x.ProductVendorMapping.Product.ProductId,
                Sku = x.ProductVendorMapping.Product.Sku,
                Quantity = x.TotalOrdered,
                VendorCost = x.CustomCost,
                Discount = x.DiscountAmount,
                Shipping = x.PurchaseOrder.ShippingCost / x.PurchaseOrder.totalQty * x.TotalOrdered,
                Tax = x.PurchaseOrder.ShippingTax / 100 * x.TotalProductCost / (x.PurchaseOrder.totalQty == 0 ? 1 : x.PurchaseOrder.totalQty) * x.TotalOrdered,
                OtherCost = x.PurchaseOrder.OtherCost / x.PurchaseOrder.totalQty * x.TotalOrdered,
                CostPerItem = (x.TotalProductCost
                               + x.PurchaseOrder.ShippingCost / (x.PurchaseOrder.totalQty == 0 ? 1 : x.PurchaseOrder.totalQty) * x.TotalOrdered
                               + x.PurchaseOrder.ShippingTax / 100 * x.TotalProductCost / x.PurchaseOrder.totalQty * x.TotalOrdered
                               + x.PurchaseOrder.OtherCost / (x.PurchaseOrder.totalQty == 0 ? 1 : x.PurchaseOrder.totalQty) * x.TotalOrdered)
                              / (x.TotalOrdered == 0 ? 1 : x.TotalOrdered),
                TotalCost = x.TotalProductCost
                            + x.PurchaseOrder.ShippingCost / x.PurchaseOrder.totalQty * x.TotalOrdered
                            + x.PurchaseOrder.ShippingTax / 100 * x.TotalProductCost / (x.PurchaseOrder.totalQty == 0 ? 1 : x.PurchaseOrder.totalQty) * x.TotalOrdered
                            + x.PurchaseOrder.OtherCost / (x.PurchaseOrder.totalQty == 0 ? 1 : x.PurchaseOrder.totalQty) * x.TotalOrdered,
                Open = x.TotalOrdered - x.TotalRecieved,
                TotalReceived = x.TotalRecieved,
                ExpectedDate = x.ExpectedDate,
            });
        return View(_PurchaseOrderDb);
    }


    /// <summary>
    /// saves the product information on the purchase order
    /// </summary>
    /// <param name="Id">product purchase order id</param>
    /// <param name="SiteLocation">location for the products</param>
    /// <param name="Recieved">how many items were recieved</param>
    /// <param name="ProductPurchase">not used</param>
    /// <param name="CustomCost">cost of the product</param>
    /// <param name="ProductQuantitiy">quantity of the product</param>
    /// <param name="GroupName">site list of the product</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.ShippingManager + "," + RoleList.ShippingBasic + "," + RoleList.InventoryBasic + "," + RoleList.Developer)]
    public async Task<IActionResult> EditStock(
        string Id,
        string SiteLocation,
        int Recieved,
        int? ProductPurchase,
        decimal CustomCost,
        int ProductQuantitiy,
        decimal DiscountPercentage,
        decimal TotalCost,
        string GroupName
    )
    {
        try
        {
            var productPoDb = await productPurchaseOrderService.GetAsync(
                q => q.ProductPurchaseOrderId == ProductPurchase,
                includes:
                [
                    x => x.ProductVendorMapping,
                    x => x.ProductVendorMapping.Product,
                    x => x.ProductVendorMapping.Vendor
                ]
            );
            var productDb = await productService.GetAsync(x => x.ProductId == productPoDb.ProductVendorMapping.Product.ProductId);

            if (Recieved != 0)
            {
                var findStock = await stocksService.GetAsync(
                    s => s.StockId.ToString() == SiteLocation,
                    includes: [x => x.Products, x => x.Location]
                );
                var findStocks = await stocksService.GetAsync(
                    x => x.ProductId == productPoDb.ProductVendorMapping.ProductId &&
                         x.LocationId.ToString() == SiteLocation,
                    includes: [x => x.Products, x => x.Location]
                );
                var findLocation = await locationService.GetAsync(x => x.LocationId.ToString() == SiteLocation);
                ProductPurchaseOrderStockMapping productStockMappingDb = new ProductPurchaseOrderStockMapping();
                if (Recieved + productPoDb.TotalRecieved > productPoDb.TotalOrdered)
                {
                    Recieved = productPoDb.TotalOrdered - productPoDb.TotalRecieved;
                }

                var utcNow = DateTime.UtcNow;
                //var findProduct = _context.Product.Where(p => p.Sku == Id).FirstOrDefault();
                if (GroupName == "Current Stock Location")
                {
                    if (findStock.TotalAvailable == 0)
                    {
                        findStock.RecentlyReadded = true;
                    }

                    findStock.TotalAvailable += Recieved;
                    findStock.ModifyByUser = User.Identity.Name;
                    findStock.ModifyDate = utcNow;
                    if (findStock.TotalAvailable <= 0)
                    {
                        findStock.LastCounted = utcNow;
                    }

                    await stocksService.UpdateAsync(findStock);
                }
                else
                {
                    if (findStocks != null)
                    {
                        if (findStocks.TotalAvailable == 0)
                        {
                            findStocks.RecentlyReadded = true;
                        }

                        findStocks.TotalAvailable += Recieved;
                        findStocks.ModifyByUser = User.Identity.Name;
                        findStocks.ModifyDate = utcNow;
                        if (findStocks.TotalAvailable <= 0)
                        {
                            findStocks.LastCounted = utcNow;
                        }

                        await stocksService.UpdateAsync(findStock);
                        findStock = findStocks;
                    }
                    else
                    {
                        Stock stock = new Stock();
                        stock.TotalAvailable = Recieved;
                        stock.RecentlyReadded = true;
                        stock.Products = productDb;
                        stock.Location = findLocation;
                        stock.IsPrimary = false;
                        stock.ModifyByUser = User.Identity.Name;
                        stock.ModifyDate = utcNow;
                        stock.LastCounted = utcNow;
                        await stocksService.AddAsync(stock);
                        findStock = stocksService.Get(
                            s => s.ProductId == stock.ProductId && s.LocationId == stock.LocationId,
                            includes: [x => x.Products, x => x.Location]
                        );
                    }
                }

                // add to movestockhistorytable futureproof
                //add to the history
                var history = new MoveStockHistory();
                history.ToStock = findStock;
                history.Sku = findStock.Products.Sku;
                history.Quantity = Recieved;
                history.Type = ActionType.Received;
                history.EmployeeName = User.Identity.Name;
                history.DateTime = utcNow;
                await moveStockHistoryService.AddAsync(history);

                productPoDb.ModifyByUser = User.Identity.Name;
                productPoDb.ModifyDate = utcNow;
                productPoDb.CustomCost = CustomCost;
                productPoDb.TotalOrdered = ProductQuantitiy;
                productPoDb.TotalRecieved += Recieved;
                productPoDb.TotalProductCost = TotalCost;
                productPoDb.DiscountAmount = Math.Round(TotalCost * (DiscountPercentage / (100 - DiscountPercentage)), 4);
                await productPurchaseOrderService.UpdateAsync(productPoDb);

                if (Recieved > 0)
                {
                    productStockMappingDb.ProductPurchaseOrder = productPoDb;
                    productStockMappingDb.Stock = findStock;
                    productStockMappingDb.QtyRecieved = Recieved;
                    var findProductStockMap = await productPurchaseOrderStockMappingService.GetAsync(
                        p => p.ProductPurchaseOrderId == productPoDb.ProductPurchaseOrderId
                             && p.StockId == findStock.StockId,
                        includes:
                        [
                            x => x.ProductPurchaseOrder,
                            x => x.Stock
                        ]
                    );

                    if (findProductStockMap == null)
                    {
                        await productPurchaseOrderStockMappingService.AddAsync(productStockMappingDb);
                    }
                    else
                    {
                        findProductStockMap.QtyRecieved += Recieved;
                        await productPurchaseOrderStockMappingService.UpdateAsync(findProductStockMap);
                    }
                }
            }
            else
            {
                productDb.OnOrder += ProductQuantitiy - productPoDb.TotalOrdered;
                productPoDb.ModifyByUser = User.Identity.Name;
                productPoDb.ModifyDate = DateTime.UtcNow;
                productPoDb.CustomCost = CustomCost;
                productPoDb.TotalOrdered = ProductQuantitiy;
                productPoDb.DiscountAmount = Math.Round(TotalCost * (DiscountPercentage / (100 - DiscountPercentage)), 4);
                productPoDb.TotalProductCost = TotalCost;
                await productPurchaseOrderService.UpdateAsync(productPoDb);
            }

            var getPPOList = await productPurchaseOrderService.GetListAsync(
                ppo => ppo.ProductVendorMapping.ProductId == productDb.ProductId
                       && ppo.PurchaseOrder.POStatus != Status.Cancelled
                       && ppo.PurchaseOrder.POStatus != Status.Close,
                null,
                includes:
                [
                    x => x.ProductVendorMapping
                ]
            );
            productDb.OnOrder = 0;
            foreach (var poProduct in getPPOList)
            {
                productDb.OnOrder += poProduct.TotalOrdered - poProduct.TotalRecieved;
            }

            await productService.UpdateAsync(productDb);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Ok();
        }

        return Ok();
    }

    /// <summary>
    /// grabs the id of the purchase order to be edited 
    /// </summary>
    /// <param name="id">id of the purchase order</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.ShippingManager + "," + RoleList.ShippingBasic + "," + RoleList.InventoryBasic + "," + RoleList.Developer)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        _PurchaseOrderDb.PurchaseOrderSingle = await purchaseorderService.GetAsync(
            q => q.PurchaseOrderId == id,
            includes:
            [
                x => x.Vendor,
                x => x.ShippingMethod,
                x => x.ShippingProvider
            ]
        );
        if (_PurchaseOrderDb.PurchaseOrderSingle == null)
        {
            return NotFound();
        }

        _PurchaseOrderDb.ProductPurchaseOrders = await productPurchaseOrderService.GetListAsync(
            q => q.PurchaseOrderId == id,
            null,
            includes:
            [
                x => x.ProductVendorMapping,
                x => x.ProductVendorMapping.Product
            ]
        );

        _PurchaseOrderDb.MiscProducts = await purchaseorderService.GetMiscProductsByPurchaseOrderId(id ?? 0) ?? new List<MiscProduct>();


        var productIds = _PurchaseOrderDb.ProductPurchaseOrders.Select(x => x.ProductVendorMapping.ProductId).ToList();
        _PurchaseOrderDb.productStock = await stocksService.GetListAsync(
            x => productIds.Contains(x.ProductId)
        );

        _PurchaseOrderDb.ProductPurchaseOrderStockMappings = await productPurchaseOrderStockMappingService.GetListAsync(
            q => q.ProductPurchaseOrder.PurchaseOrderId == id,
            includes:
            [
                x => x.Stock,
                x => x.Stock.Products,
                x => x.Stock.Location,
                x => x.Stock.Location.Sites
            ]
        );
        _PurchaseOrderDb.purchaseOrderFilesMappings = await purchaseOrderFilesMappingService.GetListAsync(
            q => q.PurchaseOrderId == _PurchaseOrderDb.PurchaseOrderSingle.PurchaseOrderId,
            null,
            includes:
            [
                x => x.Files
            ]
        );

        var ShippingMethods = await shippingMethodService.GetListAsync(
            q => q.ShippingProvider.ShippingProviderId == _PurchaseOrderDb.PurchaseOrderSingle.ShippingProviderId && q.IsActive,
            orderSelectors: [o => o.ShippingMethodName],
            includes: [x => x.ShippingProvider]
        );

        var VendorProducts = await productVendorMappingService.GetListAsync(
            q => q.VendorId == _PurchaseOrderDb.PurchaseOrderSingle.VendorId && q.IsActive,
            includes:
            [
                x => x.Vendor,
                x => x.Product
            ]
        );
        List<SelectListItem> VendorProductsDropDwon = new List<SelectListItem>();
        foreach (var item in VendorProducts)
        {
            VendorProductsDropDwon.Add(new SelectListItem { Value = item.ProductVendorMappingId.ToString(), Text = $"{item.Product.Sku} | {item.VendorSku}" });
        }

        ViewData["ShippingPro"] = new SelectList((await shippingProviderService.GetAllAsync()).Where(x => x.IsActive),"ShippingProviderId", "ShippingProviderName");
        ViewData["ShippingMeth"] = new SelectList(ShippingMethods, "ShippingMethodId", "ShippingMethodName");
        ViewData["VendorProducts"] = VendorProductsDropDwon; //new SelectList(VendorProducts, "ProductVendorMappingId", "Product.Sku"+"|"+ "VendorSku");
        ViewData["ProductsVendorId"] = _PurchaseOrderDb.PurchaseOrderSingle.VendorId;
        return View(_PurchaseOrderDb);
    }

    /// <summary>
    /// edit page, saves the purchase order and updates the grandtotal
    /// </summary> 
    /// <param name="id">id of the purchase order</param>
    /// <param name="purchaseOrderView">view model of the purchase orders</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.ShippingManager + "," + RoleList.ShippingBasic + "," + RoleList.InventoryBasic + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("PurchaseOrderSingle, ProductPurchaseOrders, PurchaseOrderSingle.PurchaseOrderId,PurchaseOrderSingle.ShippingMethodId,PurchaseOrderSingle.ShippingMethod, PurchaseOrderSingle.ShippingProvider,PurchaseOrderSingle.ShippingProviderId,PurchaseOrderSingle.VendorId,PurchaseOrderSingle.PurchaseOrderNumber,PurchaseOrderSingle.OrderDate,PurchaseOrderSingle.POStatus,PurchaseOrderSingle.ReferenceNumber," +
            "PurchaseOrderSingle.Notes,PurchaseOrderSingle.ShippingCost,PurchaseOrderSingle.GrandTotal,PurchaseOrderSingle.Discount,PurchaseOrderSingle.ShippingTax,PurchaseOrderSingle.OtherCost,PurchaseOrderSingle.IsActive, ProductPurchaseOrders.CustomCost, ProductPurchaseOrders.TotalOrdered, ProductPurchaseOrders.TotalRecieved")] PurchaseOrderViewModel purchaseOrderView, string idshippingmeth, string idshippingpro)
    {
        try
        {
            if (purchaseOrderView == null)
            {
                return NotFound();
            }

            if (id != purchaseOrderView.PurchaseOrderSingle.PurchaseOrderId)
            {
                return NotFound();
            }

            ModelState.Keys.Where(k => k.StartsWith("PurchaseOrderSingle.Vendor."))
                .ToList()
                .ForEach(key => ModelState.Remove(key));

            if (ModelState.IsValid)
            {
                var getPurchaseOrderDb = await purchaseorderService.GetAsync(x => x.PurchaseOrderId == id);
                var getProductPO = await productPurchaseOrderService.GetListAsync(x => x.PurchaseOrderId == getPurchaseOrderDb.PurchaseOrderId);
                var costCount = 0.00m;
                var inprogress = false;
                foreach (var item in getProductPO)
                {
                    costCount += item.CustomCost * item.TotalOrdered;
                    if (item.TotalRecieved > 0)
                    {
                        inprogress = true;
                    }
                }

                getPurchaseOrderDb.Discount = purchaseOrderView.PurchaseOrderSingle.Discount;
                if (inprogress)
                {
                    getPurchaseOrderDb.POStatus = Status.InProgress;
                    var fullyReceived = 0;
                    foreach (var item in getProductPO)
                    {
                        fullyReceived += item.TotalOrdered - item.TotalRecieved;
                    }

                    if (fullyReceived == 0)
                    {
                        getPurchaseOrderDb.POStatus = Status.FullyReceived;
                    }
                    else
                    {
                        getPurchaseOrderDb.POStatus = Status.InProgress;
                    }
                }

                getPurchaseOrderDb.OtherCost = purchaseOrderView.PurchaseOrderSingle.OtherCost;
                getPurchaseOrderDb.ShippingCost = purchaseOrderView.PurchaseOrderSingle.ShippingCost;
                getPurchaseOrderDb.ReferenceNumber = purchaseOrderView.PurchaseOrderSingle.ReferenceNumber;
                getPurchaseOrderDb.Notes = purchaseOrderView.PurchaseOrderSingle.Notes;
                getPurchaseOrderDb.IsActive = purchaseOrderView.PurchaseOrderSingle.IsActive;
                getPurchaseOrderDb.EstimatedDate = purchaseOrderView.PurchaseOrderSingle.EstimatedDate;
                var getShipMeth = await shippingMethodService.GetAsync(x => x.ShippingMethodId.ToString() == idshippingmeth);
                var getShipProv = await shippingProviderService.GetAsync(x => x.ShippingProviderId.ToString() == idshippingpro);
                getPurchaseOrderDb.ShippingMethod = getShipMeth;
                getPurchaseOrderDb.ShippingProvider = getShipProv;
                getPurchaseOrderDb.ModifyByUser = User.Identity.Name;
                getPurchaseOrderDb.ModifyDate = DateTime.UtcNow;
                costCount = costCount - costCount * getPurchaseOrderDb.Discount / 100;
                costCount = costCount + costCount * getPurchaseOrderDb.ShippingTax / 100;
                costCount += getPurchaseOrderDb.ShippingCost + getPurchaseOrderDb.OtherCost;
                getPurchaseOrderDb.GrandTotal = costCount;
                await purchaseorderService.UpdateAsync(getPurchaseOrderDb);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// not yet implemented
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator)]
    public IActionResult Delete()
    {
        return View();
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer)]
    public async Task<IActionResult> DeleteFile(int? id)
    {
        try
        {
            var poFile = await purchaseOrderFilesMappingService.GetAsync(
                q => q.PurchaseOrderFilesMappingId == id,
                includes:
                [
                    x => x.Files
                ]
            );

            if (poFile != null)
            {
                var getPOId = poFile.PurchaseOrderId;
                var file = poFile.Files;

                if (file.FileUrl != null && file.FileUrl != "")
                {
                    await fileService.RemoveAzureBlobAsync(file.FileUrl, file.FileType);
                }

                await fileService.RemoveAsync(file.FileId);
                await purchaseOrderFilesMappingService.RemoveAsync(poFile.PurchaseOrderFilesMappingId);

                return RedirectToAction(nameof(Edit), new { id = getPOId });
            }
        }
        catch
        {
        }

        return View();
    }

    /// <summary>
    /// grabs the vendor information based on the vendor selected
    /// </summary>
    /// <param name="id">vendor id</param>
    /// <returns></returns>
    public async Task<IActionResult> GetVendorInformation(string id)
    {
        if (id == null)
        {
            return Ok();
        }

        var getDbVendor = await vendorService.GetAsync(x => x.VendorId.ToString().Equals(id));
        var getPVM = await productVendorMappingService.GetListAsync(
            v => v.VendorId == getDbVendor.VendorId && v.IsActive,
            includes:
            [
                p => p.Product
            ]
        );
        var vendorContact = getDbVendor.PhoneNumber;
        var vendorEmail = getDbVendor.BusinessEmail;
        return Json(new
        {
            vendorContact,
            vendorEmail
        });
    }

    /// <summary>
    /// in creation get the list of products via product vendor mapping
    /// </summary>
    /// <param name="id">vendor id</param>
    /// <returns></returns>
    public async Task<IActionResult> GetProductsbyVendorId(string id)
    {
        if (id == null)
        {
            return Ok();
        }

        if (id == "0")
        {
            return Ok();
        }

        var getDbVolume = await vendorService.GetAsync(x => x.VendorId.ToString().Equals(id) && x.IsActive);
        var getPVM = await productVendorMappingService.GetListAsync(
            v => v.VendorId == getDbVolume.VendorId && v.IsActive,
            includes:
            [
                p => p.Product,
                p => p.Vendor
            ]
        );
        return Json(getPVM);
    }

    /// <summary>
    /// get PVM cost for product by the selected vendor
    /// </summary>
    /// <param name="productId">product id</param>
    /// <param name="vendorId">vendor id</param>
    /// <returns></returns>
    public async Task<IActionResult> GetPVMDetails(int productId, int vendorId)
    {
        decimal cost = 0;
        int productVendorMappingId = 0;
        int pvmLeadTime = 0;

        if (productId != 0 && vendorId != 0)
        {
            var data = await productVendorMappingService.GetAsync(p => p.ProductId == productId && p.VendorId == vendorId && p.IsActive);
            cost = data?.Cost ?? 0;
            productVendorMappingId = data.ProductVendorMappingId;
            pvmLeadTime = data?.LeadTime ?? 0;
        }

        return Json(new { cost, productVendorMappingId, pvmLeadTime });
    }


    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer)]
    public async Task<IActionResult> GetPVMAverageCost(string id)
    {
        try
        {
            var getpvm = await productVendorMappingService.GetAsync(
                p => p.ProductVendorMappingId.ToString() == id && p.IsActive,
                includes:
                [
                    x => x.Product,
                    x => x.Vendor
                ]
            );
            var pvmid = getpvm.ProductVendorMappingId;
            var getcost = getpvm.Product.Cost;
            var getaverage = 0.00m;


            var testAmount = await productPurchaseOrderService.GetListAsync(
                ppo => ppo.ProductVendorMapping.Product.ProductId == getpvm.ProductId
                       && ppo.PurchaseOrder.POStatus == Status.Close,
                includes:
                [
                    x => x.ProductVendorMapping,
                    x => x.ProductVendorMapping.Product,
                    x => x.PurchaseOrder
                ]
            );

            if (testAmount.Any())
            {
                getaverage = testAmount.Sum(x => x.CustomCost / testAmount.Count());
                return Json(new
                {
                    getaverage,
                    getcost,
                    pvmid
                });
            }

            return Json(new
            {
                getaverage,
                getcost,
                pvmid
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// grabs shipping method based on provider selected
    /// </summary>
    /// <param name="ShipId">shipping provider id</param>
    /// <returns></returns>
    public async Task<IActionResult> MethodsByShipPro(int ShipId)
    {
        List<ShippingMethod> shippingMthodList = await shippingMethodService.GetListAsync(
            x => x.ShippingProviderId == ShipId,
            orderSelectors: [o => o.ShippingMethodName],
            includes: [x => x.ShippingProvider]
        );
        return Json(shippingMthodList);
    }

    /// <summary>
    /// grabs locations based on the product sku
    /// </summary>
    /// <param name="psku">product sku</param>
    /// <returns></returns>
    public async Task<IActionResult> getCurrentStock(string psku)
    {
        psku = psku != null ? psku.TrimStart() : "";

        var query = (IQueryable<Stock> stock) => stock.Where(x => x.Products.Sku == psku)
            .Include(x => x.Location)
            .Include(x => x.Products)
            .Select(
                x => (object)new
                {
                    x.StockId,
                    Loc = x.Location.Sites.SiteName + ": " + x.Location.LocationName,
                    groupname = "Current Stock Location"
                }
            );

        var thestock = await stocksService.GetListAsync(query);
        return Json(thestock);
    }


    public async Task<IActionResult> GetCurrentStockAndAllOtherLocations(string psku)
    {
        psku = psku != null ? psku.TrimStart() : "";

        var stockLocationQuery = (IQueryable<Stock> stock) => stock.Where(x => x.Products.Sku == psku)
            .Include(x => x.Location)
            .Include(x => x.Products)
            .Select(
                x => new StockLocationDetails
                {
                    StockId = x.StockId,
                    Location = x.Location.Sites.SiteName + ": " + x.Location.LocationName,
                    GroupName = "Current Stock Location"
                }
            );

        var thestock = await stocksService.GetListAsync(stockLocationQuery);
        var groupedByStocks = thestock.GroupBy(item => item.GroupName).ToList();

        var allLocationQuery = (IQueryable<Location> location) => location
            .Where(x => x.IsActive)
            .Include(x => x.Sites)
            .OrderBy(x => x.Sites.SiteName)
            .ThenByDescending(q => q.LocationName)
            .Select(
                x => new LocationDetails
                {
                    LocationId = x.LocationId,
                    Location = x.LocationName,
                    GroupName = x.Sites.SiteName
                }
            );

        var thelocations = await locationService.GetListAsync(allLocationQuery);
        var groupedByLocations = thelocations.GroupBy(item => item.GroupName).ToList();

        List<Result> resultList = new List<Result>();


        foreach (var item in groupedByStocks)
        {
            resultList.Add(new Result()
            {
                Text = item.Key,
                Children = item.Select(item => new ResultChildren() { Id = item.StockId, Text = item.Location }).ToList()
            });
        }

        foreach (var item in groupedByLocations)
        {
            resultList.Add(new Result()
            {
                Text = item.Key,
                Children = item.Select(item => new ResultChildren() { Id = item.LocationId, Text = item.Location }).ToList()
            });
        }

        return Json(resultList);
    }




    /// <summary>
    /// grabs all locations
    /// </summary>
    /// <param name="psku">product sku</param>
    /// <returns></returns>
    public async Task<IActionResult> GetAllLocations(string psku)
    {
        var query = (IQueryable<Location> location) => location
            .Where(x => x.IsActive)
            .Include(x => x.Sites)
            .OrderBy(x => x.Sites.SiteName)
            .ThenByDescending(q => q.LocationName)
            .Select(
                x => (object)new
                {
                    LocationId = x.LocationId,
                    Loc = x.LocationName,
                    groupname = x.Sites.SiteName
                }
            );

        var thestock = await locationService.GetListAsync(query);
        return Json(thestock);
    }

    /// <summary>
    /// gets the stock list given the product sku
    /// </summary>
    /// <param name="psku"> product sku</param>
    /// <returns></returns>
    public async Task<PurchaseOrderViewModel> GetStockList(string psku)
    {
        _PurchaseOrderDb.ProductPurchaseOrderStockMappings = await productPurchaseOrderStockMappingService.GetListAsync(
            x => x.ProductPurchaseOrderId.ToString() == psku.TrimStart()
        );
        return _PurchaseOrderDb;
    }

    /// <summary>
    /// Grabs the data for the partialviewtable
    /// </summary>
    /// <returns></returns>
    public IActionResult PartialViewTable()
    {
        return PartialView("PartialIndex", _PurchaseOrderDb);
    }

    /// <summary>
    /// sets po to closed
    /// </summary>
    /// <param name="id">purchase order id</param>
    /// <returns></returns>
    public IActionResult ClosePO(int? id)
    {
        purchaseorderService.Close(id.Value);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Force closed
    /// </summary>
    /// <param name="id">purchase order id</param>
    /// <param name="closeNote">User note to force close</param>
    /// <returns></returns>
    public async Task<IActionResult> ForceClose(int id, string closeNote)
    {
        if (string.IsNullOrWhiteSpace(closeNote)) return RedirectToAction(nameof(Details));

        await purchaseorderService.ForceCloseAsync(id, closeNote);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// sets po to cancel
    /// </summary>
    /// <param name="id">purchase order id</param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer)]
    public async Task<IActionResult> CancelPO(int? id)
    {
        var getPurchaseOrder = await purchaseorderService.GetAsync(x => x.PurchaseOrderId == id);
        getPurchaseOrder.POStatus = Status.Cancelled;
        getPurchaseOrder.ModifyByUser = User.Identity.Name;
        getPurchaseOrder.ModifyDate = DateTime.UtcNow;
        await purchaseorderService.UpdateAsync(getPurchaseOrder);

        var getpurchaseOrderProducts = await productPurchaseOrderService.GetListAsync(
            x => x.PurchaseOrderId == getPurchaseOrder.PurchaseOrderId,
            null,
            includes:
            [
                x => x.ProductVendorMapping
            ]
        );

        foreach (var ppo in getpurchaseOrderProducts)
        {
            var getProduct = await productService.GetAsync(p => p.ProductId == ppo.ProductVendorMapping.ProductId);

            var getPPOList = await productPurchaseOrderService.GetListAsync(
                p => p.ProductVendorMapping.ProductId == getProduct.ProductId
                     && p.PurchaseOrder.POStatus != Status.Cancelled && p.PurchaseOrder.POStatus != Status.Close,
                null,
                includes:
                [
                    x => x.ProductVendorMapping
                ]
            );

            getProduct.OnOrder = 0;
            foreach (var poProduct in getPPOList)
            {
                getProduct.OnOrder += poProduct.TotalOrdered - poProduct.TotalRecieved;
            }

            await productService.UpdateAsync(getProduct);
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer)]
    [HttpPost]
    
    public async Task<IActionResult> AddProduct(int? id, [Bind("PurchaseOrderSingle, ProductPurchaseOrders, ProductPurchaseOrderSingle, PurchaseOrderSingle.PurchaseOrderId,")] PurchaseOrderViewModel purchaseOrderView)
    {
        try
        {
            var getPurchaseOrder = await purchaseorderService.GetAsync(po => po.PurchaseOrderId == purchaseOrderView.PurchaseOrderSingle.PurchaseOrderId); getPurchaseOrder.POStatus = Status.InProgress;

            await purchaseorderService.UpdateAsync(getPurchaseOrder);

            if (purchaseOrderView.ProductPurchaseOrders != null)
            {
                foreach (var productPurchaseOrderSingle in purchaseOrderView.ProductPurchaseOrders)
                {
                    var getPVM = await productVendorMappingService.GetAsync(x => x.ProductVendorMappingId == productPurchaseOrderSingle.ProductVendorMappingId);
                    var getProduct = await productService.GetAsync(p => p.ProductId == getPVM.ProductId);
                    ProductPurchaseOrder productPurchaseOrder = new ProductPurchaseOrder();
                    productPurchaseOrder.PurchaseOrder = getPurchaseOrder;
                    productPurchaseOrder.ProductVendorMapping = getPVM;
                    productPurchaseOrder.CustomCost = productPurchaseOrderSingle.CustomCost;
                    productPurchaseOrder.TotalOrdered = productPurchaseOrderSingle.TotalOrdered;
                    productPurchaseOrder.TotalRecieved = productPurchaseOrderSingle.TotalRecieved;
                    productPurchaseOrder.ModifyByUser = User.Identity.Name;
                    productPurchaseOrder.ModifyDate = DateTime.UtcNow;
                    productPurchaseOrder.DiscountPercentage = Convert.ToDecimal(productPurchaseOrderSingle.DiscountPercentage);
                    productPurchaseOrder.TotalProductCost = Convert.ToDecimal(productPurchaseOrderSingle.TotalProductCost);
                    productPurchaseOrder.DiscountAmount = Math.Round(productPurchaseOrder.TotalProductCost * (productPurchaseOrder.DiscountPercentage / (100 - productPurchaseOrder.DiscountPercentage)), 4);

                    var query = (IQueryable<Stock> stock) => stock
                        .Where(
                            z => z.ProductId == getProduct.ProductId
                                 && z.Location.SiteId == 1
                                 || z.Location.SiteId == 2
                                 || z.Location.SiteId == 48
                                 || z.Location.SiteId == 49
                        ).Include(x => x.Products)
                        .Include(x => x.Location)
                        .GroupBy(y => y.Products.ProductId)
                        .Select(
                            g => new Product
                            {
                                ProductId = g.Key,
                                StockTotalAvailable = g.Sum(i => i.TotalAvailable)
                            }
                        );

                    var productsWithStock = await stocksService.GetAsync(query);

                    productPurchaseOrder.AverageCost = (productsWithStock.StockTotalAvailable * getProduct.Cost + productPurchaseOrder.CustomCost * productPurchaseOrder.TotalOrdered) / (productsWithStock.StockTotalAvailable + productPurchaseOrder.TotalOrdered);

                    await productPurchaseOrderService.AddAsync(productPurchaseOrder);

                    var getPPOList = await productPurchaseOrderService.GetListAsync(
                        ppo => ppo.ProductVendorMapping.ProductId == getProduct.ProductId
                               && ppo.PurchaseOrder.POStatus != Status.Cancelled &&
                               ppo.PurchaseOrder.POStatus != Status.Close,
                        null,
                        includes:
                        [
                            x => x.ProductVendorMapping,
                            x => x.PurchaseOrder
                        ]
                    );

                    getProduct.OnOrder = 0;
                    foreach (var poProduct in getPPOList)
                    {
                        getProduct.OnOrder += poProduct.TotalOrdered - poProduct.TotalRecieved;
                    }

                    await productService.UpdateAsync(getProduct);
                }
            }

            return RedirectToAction(nameof(Edit), new { id = purchaseOrderView.PurchaseOrderSingle.PurchaseOrderId });
        }
        catch
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            return RedirectToAction(nameof(Edit), new { id = purchaseOrderView.PurchaseOrderSingle.PurchaseOrderId });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="PurchaseorderId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> GenerateBarcode(int PurchaseorderId)
    {
        var img = "";
        if (PurchaseorderId == 0)
        {
            return Json("please select a product");
        }

        var getPurchaseorder = await purchaseorderService.GetAsync(x => x.PurchaseOrderId == PurchaseorderId);
        using (MemoryStream ms = new())
        {
            var b = new Barcode
            {
                IncludeLabel = true,
                LabelFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Sans Serif"))
            };
            using var bitmap = SKBitmap.FromImage(b.Encode(BarcodeStandard.Type.Code128B, getPurchaseorder.PurchaseOrderId.ToString()));
            bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
            img = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }

        return Json(img);
    }

    /// <summary>
    /// uploads the file 
    /// </summary>
    /// <param name="purOrdid"></param>
    /// <param name="upload"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleList.Administrator + "," + RoleList.FinancialManager + "," + RoleList.Developer)]
    public async Task<IActionResult> SaveFile(string purOrdid, IFormFile upload, string fileName)
    {
        try
        {
            int poId = 0;
            if (purOrdid == null || upload == null || !Int32.TryParse(purOrdid, out poId))
            {
                return Ok();
            }

            string extension = Path.GetExtension(upload.FileName).ToLower();
            if (extension != ".jpg" && extension != ".png" && extension != ".pdf")
            {
                return RedirectToAction("Edit", new { id = purOrdid });
            }

            if (fileName == null)
            {
                fileName = Path.GetFileName(upload.FileName);
            }

            FileType type;
            switch (extension)
            {
                case ".jpg":
                case ".png":
                    type = FileType.Image;
                    break;
                case ".pdf":
                    type = FileType.Pdf;
                    break;
                default:
                    return RedirectToAction("Edit", new { id = purOrdid });
            }

            var fileUrl = await fileService.UploadToAzureAsync(upload, type);
            var file = new Files
            {
                FileName = fileName,
                FileType = type,
                ContentType = upload.ContentType,
                FileUrl = fileUrl
            };

            await fileService.AddAsync(file);
            await purchaseOrderFilesMappingService.AddAsync(new PurchaseOrderFilesMapping()
            {
                FileId = file.FileId,
                PurchaseOrderId = poId
            });

            return RedirectToAction(nameof(Edit), new { id = purOrdid });
        }
        catch
        {
            return RedirectToAction(nameof(Edit), new { id = purOrdid });
        }
    }

    [HttpPost("GetPurchaseOrderList")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetPurchaseOrderList(string productFilter, string statusFilter, string startDateInput, string endDateInput, bool isEstimateDate, bool islastDate)
    {
        var purchaseOrders = new List<PurchaseOrder>();
        int recordsTotal = 0;
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form[
            "columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"
        ].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault().ToLower();
        int pageSize = 0;
        var myPermission = "np";
        if (
            User.IsInRole(RoleList.Administrator)
            || User.IsInRole(RoleList.InventoryManager)
            || User.IsInRole(RoleList.ShippingManager)
            || User.IsInRole(RoleList.InventoryBasic)
        )
        {
            myPermission = "yes";
        }
        else
        {
            myPermission = "no";
        }

        var query = (IQueryable<PurchaseOrder> po) => po
            .Include(x => x.Vendor)
            .Include(x => x.ShippingMethod)
            .Include(x => x.ShippingProvider)
            .Include(x => x.OrderFiles);
        var result = purchaseorderService.QueryFilter(query);

        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                result = result.OrderBy(x => x.OrderDate);
            }
            else if (sortColumnDirection == "desc")
            {
                result = result.OrderByDescending(x => x.OrderDate);
            }
        }

        //filters through a user search string (massive for amount of columns to search through normally done behind the scenes)
        if (!string.IsNullOrEmpty(searchValue))
        {
            var filteredEnumTypes = Enum.GetValues<Status>()
                .Where(value => value.ToString().ToLower().Contains(searchValue))
                .ToList();
            var filteredBoolIsActive = "";
            if ("yes".Contains(searchValue) || "Yes".Contains(searchValue))
            {
                filteredBoolIsActive = "true";
            }
            else if ("no".Contains(searchValue) || "No".Contains(searchValue))
            {
                filteredBoolIsActive = "false";
            }
            else
            {
                filteredBoolIsActive = "~~~~~";
            }

            result = result.Where(
                l =>
                    l.Vendor.VendorName.Contains(searchValue)
                    || l.PurchaseOrderNumber.ToLower().Contains(searchValue)
                    || l.ReferenceNumber.ToLower().Contains(searchValue)
                    || filteredEnumTypes.Contains(l.POStatus)
                    || l.IsActive.ToString().ToLower().Contains(filteredBoolIsActive)
                    || l.OrderDate.ToString().ToLower().Contains(searchValue)
                    || l.Notes.Contains(searchValue)
            );
        }

        DateTime startDate = Convert.ToDateTime(startDateInput);
        DateTime endDate = Convert.ToDateTime(endDateInput);
        if (isEstimateDate)
        {
            result = result.Where(
                po => po.OrderDate >= startDate && po.OrderDate <= endDate);
        }

        if (islastDate)
        {
            result = result.Where(
                po => po.ModifyDate >= startDate && po.OrderDate <= endDate);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            var statusList = JsonConvert.DeserializeObject<List<Status>>(statusFilter);
            if (statusList.Count > 0)
            {
                result = result.Where(
                    po => statusList.Contains(po.POStatus));
            }
        }

        if (!string.IsNullOrEmpty(productFilter))
        {
            var productIds = JsonConvert.DeserializeObject<List<int>>(productFilter);
            if (productIds.Count > 0)
            {
                var getPPO = new List<int>();

                var queryPPO = (IQueryable<ProductPurchaseOrder> Query) => Query
                    .Where(x => productIds.Contains(x.ProductVendorMapping.ProductId))
                    .Include(x => x.ProductVendorMapping)
                    .Include(x => x.PurchaseOrder)
                    .Select(x => x.PurchaseOrderId);

                getPPO = await productPurchaseOrderService.GetListAsync(queryPPO);

                if (getPPO != null)
                {
                    result = result.Where(
                        x => getPPO.Contains(x.PurchaseOrderId));
                }
            }
        }

        // sets page size for the user
        if (length != null)
        {
            if (length == "-1")
            {
                pageSize = result.Count();
            }
            else
            {
                pageSize = Convert.ToInt32(length);
            }
        }

        //sets the start point to the skip point if your on a page
        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        recordsTotal = result.Count();

        try
        {
            purchaseOrders = (
                from purchase in result
                select new PurchaseOrder
                {
                    PurchaseOrderNumber = purchase.PurchaseOrderNumber,
                    Attachments = purchase.OrderFiles == null ? 0 : purchase.OrderFiles.Count,
                    Vendor = purchase.Vendor,
                    PurchaseOrderId = purchase.PurchaseOrderId,
                    ReferenceNumber = purchase.ReferenceNumber,
                    Notes = purchase.Notes,
                    OrderDate = purchase.OrderDate,
                    EstimatedDate = purchase.EstimatedDate,
                    IsActive = purchase.IsActive,
                    POStatus = purchase.POStatus,
                    ModifyDate = purchase.ModifyDate,
                    Permission = ((purchase.POStatus != Status.Close || purchase.POStatus != Status.Cancelled) && myPermission == "yes") ? "Yes" : "No",
                }
            ).Skip(skip).Take(pageSize).ToList();
        }
        catch (Exception)
        {
            throw;
        }

        //column sort direction


        var jsonData = new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data = purchaseOrders
        };


        return Ok(jsonData);
    }

    /// <summary>
    /// partial table call to relaod the partialview
    /// </summary>
    /// <returns></returns>
    public IActionResult POPartialTable()
    {
        var myPO = _PurchaseOrderDbPar.PurchaseOrders;
        return PartialView("POPartialTable", myPO);
    }

    public async Task<IActionResult> IsPOIdExisting(string id)
    {
        var IsExists = await purchaseorderService.IsExistsAsync(x => x.PurchaseOrderNumber == id);
        return Json(IsExists);
    }

    [HttpPost("GetPOProducts")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetPOProducts(string PoNumber)
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

        var po = await purchaseorderService.GetAsync(x => x.PurchaseOrderNumber == PoNumber);


        var query = (IQueryable<ProductPurchaseOrder> ppo) =>
        {
            ppo = ppo.Include(x => x.ProductVendorMapping).Include(x => x.ProductVendorMapping.Product)
                .Where(x => x.PurchaseOrderId == po.PurchaseOrderId);

            return ppo.Select(x => new POProductDTO
            {
                ProductId = x.ProductVendorMapping.ProductId,
                Sku = x.ProductVendorMapping.Product.Sku,
                Cost = x.CustomCost,
                Discount = x.DiscountPercentage,
                Quantity = x.TotalOrdered,
                Total = x.TotalOrdered * x.CustomCost,
                TotalRecieved = x.TotalRecieved
            });
        };
        var productDTOs = await productPurchaseOrderService.GetListAsync(query);
        if (length != null)
        {
            pageSize = length == "-1" ? productDTOs.Count() : Convert.ToInt32(length);
        }

        int skip = 0;
        if (start != null)
        {
            skip = Convert.ToInt32(start);
        }

        var recordsTotal = productDTOs.Count();

        var data = productDTOs.Skip(skip).Take(pageSize).ToList();

        return Ok(new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        });
    }

    private void UpdateDaynamicTableValues(string productskus, string productaverages, string productcosts, string productquantity)
    {
        ViewData["productskus"] = productskus;
        ViewData["productaverages"] = productaverages;
        ViewData["productcosts"] = productcosts;
        ViewData["productquantity"] = productquantity;
    }

    private async Task<string> NewOrderNumber(bool isFake)
    {
        var currentUserID = this.User?.FindFirst(ClaimTypes.NameIdentifier).Value;
        var User = userManager.Users.FirstOrDefault(x => x.Id == currentUserID);
        var employee = await employeeService.GetAsync(x => x.ApsuId == currentUserID);
        string first = User.NormalizedUserName[0].ToString();
        string last = User.NormalizedUserName[1].ToString();

        if (employee != null)
        {
            first = string.IsNullOrEmpty(employee.FirstName) ? "" : (employee.FirstName.ToUpper()[0]).ToString();
            last = string.IsNullOrEmpty(employee.LastName) ? "" : (employee.LastName.ToUpper()[0]).ToString();
        }

        if (!isFake)
        {
            int newId = (await purchaseorderService.GetCountAsync()) + 1;

            var number = newId.ToString().PadLeft(3, '0');

            return $"{first}{last}{number}";
        }

        return $"{first}{last}###";
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMiscRecord(int id)
    {
        try
        {
            await purchaseorderService.DeleteMiscProductAsync(id, User.Identity.Name);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error deactivating record: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMiscProducts(string MiscProducts)
    {
        if (string.IsNullOrEmpty(MiscProducts))
        {
            return BadRequest("No items to update.");
        }

        List<MiscProduct> miscProductsList = JsonConvert.DeserializeObject<List<MiscProduct>>(MiscProducts);

        if (miscProductsList == null || !miscProductsList.Any())
        {
            return BadRequest("Invalid data.");
        }

        foreach (var item in miscProductsList)
        {
            item.ModifyDate = DateTime.UtcNow;
            item.ModifyByUser = User.Identity.Name;
        }

        await purchaseorderService.UpdateMiscProducts(miscProductsList);
        return Ok(new { success = true });
    }
    [HttpGet]
    public async Task<JsonResult> GetPendingPurchaseOrderBatchNumbers()
    {
        var batchNumbers = await orderBatchService.GetListAsync(
            (IQueryable<OrderBatch> query) => query
                .Where(x => x.RequiresPO == true && x.PurchaseOrderId == null)
                .OrderBy(o => o.BatchNumber)
                .Select(o => new
                {
                    id = o.OrderBatchId,
                    text = o.BatchNumber
                })
        );

        return new JsonResult(new { results = batchNumbers });
    }

    private bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return false;

        try
        {
            var obj = JsonConvert.DeserializeObject<List<MiscProduct>>(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public record CombinedProductInfo
    {
        [DisplayName("SKU")]
        public string Sku { get; set; }

        [DisplayName("Vendor SKU")]
        public string VendorSku { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Quantity")]
        public int Quantity { get; set; }

        [DisplayName("Cost")]
        public decimal Cost { get; set; }

        [DisplayName("Custom Cost")]
        public decimal CustomCost { get; set; }

        [DisplayName("Average Cost")]
        public decimal AverageCost { get; set; }

        [DisplayName("Total Ordered")]
        public int TotalOrdered { get; set; }

        [DisplayName("Total Received")]
        public int TotalReceived { get; set; }

        [DisplayName("Discount Percentage")]
        public decimal DiscountPercentage { get; set; }

        [DisplayName("Discount Amount")]
        public decimal DiscountAmount { get; set; }

        [DisplayName("Total Product Cost")]
        public decimal TotalProductCost { get; set; }

        [DisplayName("Expected Date")]
        public DateTime ExpectedDate { get; set; }
    }
}