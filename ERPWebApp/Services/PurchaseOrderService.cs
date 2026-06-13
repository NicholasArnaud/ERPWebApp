using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static ERPWebApp.Controllers.PurchaseOrders.PurchaseOrdersController;

namespace ERPWebApp.Services
{
    public class PurchaseOrderService : Service<PurchaseOrder>, IPurchaseOrderService
    {
        IUnitOfWork _unitOfWork;
        public PurchaseOrderService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddMiscProductAsync(MiscProduct miscProduct)
        {
            _unitOfWork.MiscProducts.Add(miscProduct);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddMiscProductListAsync(List<MiscProduct> miscProductList)
        {
            await _unitOfWork.MiscProducts.AddRangeAsync(miscProductList);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task ForceCloseAsync(int id, string closeNote)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var po = await _unitOfWork.PurchaseOrders.GetByIdAsync(id);
                po.POStatus = Status.Close;

                if (!string.IsNullOrEmpty(po.Notes))
                    po.Notes += $"\n\nClose Note: \n{closeNote}";
                else
                    po.Notes = $"Close Note: \n{closeNote}";

                _unitOfWork.PurchaseOrders.Update(po);
                await _unitOfWork.SaveChangesAsync();

                var po_products = await _unitOfWork.ProductPurchaseOrders.GetListByQueryAsync(
                    (q) => q.Where(x => x.PurchaseOrderId == id)
                        .Include(x => x.ProductVendorMapping)
                            .ThenInclude(x=>x.Vendor)
                        .Select(X =>new {
                            X.ProductVendorMapping.ProductId,
                             IsUSVender = X.ProductVendorMapping.Vendor.Country!=null ? X.ProductVendorMapping.Vendor.Country.Equals("US", StringComparison.OrdinalIgnoreCase) : false,
                            cost = X.CustomCost
                        })
                );

                foreach (var po_product in po_products)
                {
                    int onOrder = await _unitOfWork.PurchaseOrders.GetProductOnOrderQtyAsync(po_product.ProductId);
                    var product = await _unitOfWork.Products.GetByIdAsync(po_product.ProductId);
                    product.OnOrder = onOrder;

                    if(po_product.IsUSVender){
                        product.Cost = po_product.cost;
                    }else{
                        product.OverseasCost = po_product.cost;
                    }

                    _unitOfWork.Products.Update(product);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

           
        }

        public void Close(int id)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var po = _unitOfWork.PurchaseOrders.GetById(id);
                po.POStatus = Status.Close;

                _unitOfWork.PurchaseOrders.Update(po);
                _unitOfWork.SaveChanges();

                var po_products = _unitOfWork.ProductPurchaseOrders.GetListByQuery(
                    (q) => q.Where(x => x.PurchaseOrderId == id)
                        .Include(x => x.ProductVendorMapping)
                            .ThenInclude(x=>x.Vendor)
                        .Select(X =>new {
                            X.ProductVendorMapping.ProductId,
                            IsUSVender = X.ProductVendorMapping.Vendor.Country!=null ? X.ProductVendorMapping.Vendor.Country.Equals("US", StringComparison.OrdinalIgnoreCase) : false,
                            cost = X.CustomCost
                        })
                );

                foreach (var po_product in po_products)
                {
                    var product = _unitOfWork.Products.GetById(po_product.ProductId);

                    if(po_product.IsUSVender){
                        product.Cost = po_product.cost;
                    }else{
                        product.OverseasCost = po_product.cost;
                    }

                    _unitOfWork.Products.Update(product);
                }
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }


        public async Task<List<PurchaseOrder>> GetActivePurchaseOrdersByProductAsync(int productId)
        {
            return await _unitOfWork.PurchaseOrders.GetActivePurchaseOrdersByProductAsync(productId);
        }

        public async Task<List<MiscProduct>> GetMiscProductsByPurchaseOrderId(int purchaseOrderId)
        {
            return await _unitOfWork.MiscProducts.GetMiscProductsByPurchaseOrderId(purchaseOrderId);
        }
        public async Task DeleteMiscProductAsync(int id, string modifiedByUser)
        {
            await _unitOfWork.MiscProducts.DeleteMiscProductAsync(id, modifiedByUser);
        }

        public async Task UpdateMiscProducts(List<MiscProduct> miscProducts)
        {
            await _unitOfWork.MiscProducts.UpdateMiscProducts(miscProducts);
        }


        public async Task<byte[]> GeneratePdfWithProductsAndMisc(List<CombinedProductInfo> combinedProductInfoList, PurchaseOrder purchaseOrderSingle)
        {
            try
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9));
                        page.PageColor(Colors.White);

                        page.Header()
                            .AlignCenter()
                            .Text(purchaseOrderSingle.Vendor.VendorName)
                            .FontSize(16)
                            .Bold();

                        page.Content().Column(col =>
                        {
                            // --- Products Section ---  
                            col.Item().Element(e => e
                                .PaddingBottom(10)
                                .AlignCenter()
                                .Text("Products")
                                .FontSize(14)
                                .Bold()
                            );

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < 5; i++) columns.RelativeColumn(1); // Adjust for 5 columns  
                                });

                                table.Header(header =>
                                {
                                    string[] headers = new[]
                                    {
                                "SKU", "Vendor SKU", "Description", "Quantity", "Cost"
                            };
                                    foreach (var h in headers)
                                        header.Cell().Element(CellStyle).Text(h).Bold();
                                });

                                foreach (var item in combinedProductInfoList)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Sku);
                                    table.Cell().Element(CellStyle).Text(item.VendorSku);
                                    table.Cell().Element(CellStyle).Text(item.Description);
                                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).Text(item.Cost.ToString("F2"));
                                }
                            });

                            // --- Spacer between tables ---  
                            col.Item().PaddingVertical(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            // --- PO Details Section ---  
                            col.Item().Element(e => e
                                .PaddingBottom(10)
                                .AlignCenter()
                                .Text("Details")
                                .FontSize(14)
                                .Bold()
                            );

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // Label  
                                    columns.RelativeColumn(1); // Value  
                                });

                                void AddRow(string label, string value)
                                {
                                    table.Cell().Element(CellStyle).Text(label).Bold();
                                    table.Cell().Element(CellStyle).Text(value);
                                }

                                AddRow("Shipping Method", purchaseOrderSingle.ShippingMethod?.ShippingMethodName ?? "-");
                                AddRow("Shipping Provider", purchaseOrderSingle.ShippingProvider?.ShippingProviderName ?? "-");
                                AddRow("Order Date", purchaseOrderSingle.OrderDate.ToString("yyyy-MM-dd"));
                                AddRow("Grand Total", purchaseOrderSingle.GrandTotal.ToString("F2"));
                                AddRow("Tax (%)", purchaseOrderSingle.ShippingTax.ToString("F2"));
                                AddRow("Discount (%)", purchaseOrderSingle.Discount.ToString("F2"));
                                AddRow("Shipping Cost", purchaseOrderSingle.ShippingCost.ToString("F2"));
                                AddRow("Other Cost", purchaseOrderSingle.OtherCost.ToString("F2"));
                            });
                        });
                    });
                });

                return doc.GeneratePdf();
            }
            catch (Exception)
            {
                throw;
            }

            // Helper for consistent cell styling  
            static IContainer CellStyle(IContainer container) =>
                container.Padding(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }
        public async Task AddProductPurchaseOrdersAsync(List<ProductPurchaseOrder> productPurchaseOrderList)
        {
            var groupedProductPurchaseOrders = productPurchaseOrderList
                .GroupBy(ppo => ppo.ProductVendorMapping.ProductId)
                .Select(group => new ProductPurchaseOrder
                {
                    ProductVendorMapping = group.First().ProductVendorMapping,
                    PurchaseOrder = group.First().PurchaseOrder,
                    CustomCost = group.Sum(ppo => ppo.CustomCost),
                    TotalOrdered = group.Sum(ppo => ppo.TotalOrdered),
                    DiscountPercentage = group.Average(ppo => ppo.DiscountPercentage),
                    TotalProductCost = group.Sum(ppo => ppo.TotalProductCost),
                    DiscountAmount = group.Sum(ppo => ppo.DiscountAmount),
                    ExpectedDate = group.Max(ppo => ppo.ExpectedDate),
                    AverageCost = group.Average(ppo => ppo.AverageCost),
                    TotalRecieved = group.Sum(ppo => ppo.TotalRecieved),
                    ModifyByUser = group.First().ModifyByUser,
                    ModifyDate = group.First().ModifyDate
                }).ToList();

            foreach (var ppo in groupedProductPurchaseOrders)
            {
                _unitOfWork.ProductPurchaseOrders.Add(ppo);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}