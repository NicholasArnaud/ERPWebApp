using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ERPWebApp.Controllers;

[Authorize(
    Roles = RoleList.Administrator
        + ","
        + RoleList.Manager
        + ","
        + RoleList.ExternalUser
        + ","
        + RoleList.CustomViewOnly
        + ","
        + RoleList.SellerBasic
)]
[CwaFeatureGate(CwaFeatures.INVENTORY)]
[CwaFeatureGate(CwaFeatures.SELLER)]
[AutoValidateAntiforgeryToken]
public class ProductGalleryController : Controller
{
    public int PageSize { get; set; } = 16;
    IProductService _productService;
    IDepartmentService _departmentService;
    public ProductGalleryController(IProductService productService, IDepartmentService departmentService)
    {
        _productService = productService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(
        [Bind(@"pageIndex,SelectedDepartments,SelectedProducts")] ProductGalleryDTO dto
    )
    {
        dto.SelectedDepartments ??= new List<int>();
        dto.SelectedProducts ??= new List<int>();

       await LoadListValues(dto.SelectedDepartments);

        var productGallery = await GetProducts(dto.pageIndex, dto.SelectedDepartments, dto.SelectedProducts);

        return View(productGallery);
    }

    public async Task<IActionResult> Pagination(int pageIndex, string departmentIds, string productIds)
    {

        var departments = !string.IsNullOrEmpty(departmentIds) && departmentIds != "null" ?
            JsonConvert.DeserializeObject<List<int>>(departmentIds) : new List<int>();

        var products = !string.IsNullOrEmpty(productIds) && productIds != "null" ?
            JsonConvert.DeserializeObject<List<int>>(productIds) : new List<int>();

       await LoadListValues(departments);

        var productGallery = await GetProducts(pageIndex, departments, products);

        return View("Index", productGallery);
    }

    private async Task LoadListValues(List<int> departments)
    {
        var departmentList = await _departmentService.GetListAsync((x) => x.IsProduction && x.IsActive);

        var productList = await _productService.GetListAsync((q) =>
            q.Where(
                x => x.IsActive
                && x.ProductImages.Any()
                && x.Departments.Any(
                    y => y.IsActive
                    && y.IsProduction
                    && ((departments.Count > 0 && departments.Contains(y.DepartmentId)) || (departments.Count == 0))
                  )
             )
            .OrderBy(x => x.Sku)
            .Select(z => new { z.ProductId, Sku = z.Sku + " : " + z.Description })
         );

        ViewData["ProductList"] = new SelectList(productList, "ProductId", "Sku");

        ViewData["DepartmentList"] = new SelectList(
            departmentList.OrderBy(x => x.DepartmentName),
            "DepartmentId",
            "DepartmentName"
        );
    }

    private async Task<ProductGalleryDTO> GetProducts(int pageIndex, List<int> departments, List<int> products)
    {

        var count = await _productService.GetCountAsync(
              x => x.IsActive
              && x.ProductImages.Any(y => y.IsDefault)
              && x.Departments.Any(
                  y => y.IsProduction
                    && y.IsActive
                    && ((departments.Count > 0 && departments.Contains(y.DepartmentId)) || (departments.Count == 0))
                 )
              && ((products.Count > 0 && products.Contains(x.ProductId)) || (products.Count == 0))
            );

        int totalPages = (int)Math.Ceiling(count / (double)PageSize);

        if (pageIndex < 1)
        {
            pageIndex = 1;
        }
        else if (pageIndex > totalPages)
        {
            pageIndex = totalPages;
        }

        IQueryable<Product> query(IQueryable<Product> q)
        {
            if (departments.Count > 0)
            {
                q = q.Where(x => x.Departments.Any(y => departments.Contains(y.DepartmentId)));
            }

            if (products.Count > 0)
            {
                q = q.Where(x => products.Contains(x.ProductId));
            }

            return q.Where(
                 x => x.IsActive
                 && x.ProductImages.Any(y => y.IsDefault)
                 && x.Departments.Any(y => y.IsProduction && y.IsActive)
             )
             .Include(x => x.Departments.Where(d => d.IsProduction && d.IsActive))
             .Include(x => x.ProductImages.Where(i => i.IsDefault))
             .OrderBy(x => x.Sku)
             .Skip((pageIndex - 1) * PageSize)
             .Take(PageSize)
             .Select(x => new Product
             {
                 ProductId = x.ProductId,
                 Sku = x.Sku,
                 Description = x.Description,
                 ProductImages = x.ProductImages
             });
        }

        var result = await _productService.GetListAsync(query);

        return new ProductGalleryDTO()
        {
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalEntries = count,
            EntriesPerPage = PageSize,
            Products = result,
            SelectedDepartments = departments,
            SelectedProducts = products
        };

    }


    public async Task<IActionResult> GetProductsByDepartment(string departmentIds, string productIds)
    {

        var selectedProduct = productIds != null ? JsonConvert.DeserializeObject<List<int>>(productIds) : null;

        if (departmentIds != null)
        {
            var _selectedDepartments = departmentIds != null ? JsonConvert.DeserializeObject<List<int>>(departmentIds) : null;

            var product = await _productService.GetListAsync(
              (q) => q.Where(x => x.IsActive
              && x.ProductImages.Any(p => p.IsDefault)
              && x.Departments.Any(y => _selectedDepartments.Contains(y.DepartmentId)
              && y.IsActive && y.IsProduction))
                  .Select(z => new
                  {
                      z.ProductId,
                      Sku = z.Sku + " : " + z.Description
                  })
            );

            return Ok(new
            {
                selectedProduct,
                product
            });
        }
        else
        {
            var product = await _productService.GetListAsync(
              (q) => q.Where(x => x.IsActive
                && x.ProductImages.Any(p => p.IsDefault)
                && x.Departments.Any(y => y.IsActive && y.IsProduction))
                .Select(z => new
                {
                    z.ProductId,
                    Sku = z.Sku + " : " + z.Description
                })
            );

            return Ok(new
            {
                selectedProduct,
                product
            });
        }
    }
}
