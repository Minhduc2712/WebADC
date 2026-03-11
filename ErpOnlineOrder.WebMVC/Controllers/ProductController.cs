using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.ProductView)]
    public class ProductController : BaseController
    {
        private readonly IProductApiClient _productApiClient;
        private readonly IProductService _productService;
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly IAuthorApiClient _authorApiClient;
        private readonly IPublisherApiClient _publisherApiClient;
        private readonly ICoverTypeApiClient _coverTypeApiClient;
        private readonly IDistributorApiClient _distributorApiClient;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductApiClient productApiClient,
            IProductService productService,
            ICategoryApiClient categoryApiClient,
            IAuthorApiClient authorApiClient,
            IPublisherApiClient publisherApiClient,
            ICoverTypeApiClient coverTypeApiClient,
            IDistributorApiClient distributorApiClient,
            ILogger<ProductController> logger)
        {
            _productApiClient = productApiClient;
            _productService = productService;
            _categoryApiClient = categoryApiClient;
            _authorApiClient = authorApiClient;
            _publisherApiClient = publisherApiClient;
            _coverTypeApiClient = coverTypeApiClient;
            _distributorApiClient = distributorApiClient;
            _logger = logger;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? search = null, int? categoryId = null, int? publisherId = null)
        {
            try
            {
                var result = await _productApiClient.GetPagedAsync(page, pageSize, search, categoryId, publisherId);
                ViewBag.Search = search;
                ViewBag.CategoryId = categoryId;
                ViewBag.PublisherId = publisherId;
                ViewBag.PageSize = pageSize;
                ViewBag.Categories = new SelectList(await _categoryApiClient.GetAllAsync(), "Id", "Category_name", categoryId);
                ViewBag.Publishers = new SelectList(await _publisherApiClient.GetAllAsync(), "Id", "Publisher_name", publisherId);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                SetErrorFromException(ex);
                return View(new PagedResult<ProductDTO> { Items = new List<ProductDTO>() });
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productApiClient.GetByIdAsync(id);

                if (product == null)
                {
                    SetErrorMessage("Không tìm thấy sản phẩm.");
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }
        [RequirePermission(PermissionCodes.ProductCreate)]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProductCreate)]
        public async Task<IActionResult> Create(Product model, int[]? categoryIds, int[]? authorIds)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var dto = new CreateProductDto
                {
                    Product_code = model.Product_code,
                    Product_name = model.Product_name,
                    Product_price = model.Product_price,
                    Product_link = model.Product_link,
                    Product_description = model.Product_description,
                    Tax_rate = model.Tax_rate,
                    Cover_type_id = model.Cover_type_id,
                    Publisher_id = model.Publisher_id,
                    Distributor_id = model.Distributor_id,
                    CategoryIds = categoryIds,
                    AuthorIds = authorIds
                };

                var created = await _productApiClient.CreateAsync(dto);
                if (created != null)
                {
                    SetSuccessMessage("Thêm sản phẩm thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm sản phẩm thất bại.");
                await LoadDropdownsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                await LoadDropdownsAsync();
                return View(model);
            }
        }
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productApiClient.GetEntityByIdAsync(id);

                if (product == null)
                {
                    SetErrorMessage("Sản phẩm không tồn tại hoặc đã bị xóa.");
                    return RedirectToAction(nameof(Index));
                }

                await LoadDropdownsAsync(product);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> Edit(int id, Product model, int[]? categoryIds, int[]? authorIds)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync(model);
                    return View(model);
                }

                var dto = new UpdateProductDto
                {
                    Id = model.Id,
                    Product_code = model.Product_code,
                    Product_name = model.Product_name,
                    Product_price = model.Product_price,
                    Product_link = model.Product_link,
                    Product_description = model.Product_description,
                    Tax_rate = model.Tax_rate,
                    Cover_type_id = model.Cover_type_id,
                    Publisher_id = model.Publisher_id,
                    Distributor_id = model.Distributor_id,
                    CategoryIds = categoryIds,
                    AuthorIds = authorIds
                };

                var (success, error) = await _productApiClient.UpdateAsync(id, dto);
                if (success)
                {
                    SetSuccessMessage("Cập nhật sản phẩm thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
                await LoadDropdownsAsync(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                SetErrorFromException(ex);
                await LoadDropdownsAsync(model);
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProductDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _productApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa sản phẩm thành công!");
                else
                    SetErrorMessage(error ?? "Xóa thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }

        [RequirePermission(PermissionCodes.ProductExport)]
        [HttpGet]
        public async Task<IActionResult> Export(string? search)
        {
            try
            {
                var bytes = await _productService.ExportProductsToExcelAsync(search);
                if (bytes == null || bytes.Length == 0)
                {
                    SetErrorMessage("Không có dữ liệu để xuất.");
                    return RedirectToAction(nameof(Index));
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"SanPham_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting products");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadDropdownsAsync(Product? product = null)
        {
            var categories = await _categoryApiClient.GetAllAsync();
            var authors = await _authorApiClient.GetAllAsync();
            var publishers = await _publisherApiClient.GetAllAsync();
            var coverTypes = await _coverTypeApiClient.GetAllAsync();
            var distributors = await _distributorApiClient.GetAllAsync();

            ViewBag.Categories = new MultiSelectList(categories, "Id", "Category_name",
                product?.Product_Categories?.Select(pc => pc.Category_id));
            ViewBag.Authors = new MultiSelectList(authors, "Id", "Author_name",
                product?.Product_Authors?.Select(pa => pa.Author_id));
            ViewBag.Publishers = new SelectList(publishers, "Id", "Publisher_name", product?.Publisher_id);
            ViewBag.CoverTypes = new SelectList(coverTypes, "Id", "Cover_type_name", product?.Cover_type_id);
            ViewBag.Distributors = new SelectList(distributors, "Id", "Distributor_name", product?.Distributor_id);
        }
    }
}
