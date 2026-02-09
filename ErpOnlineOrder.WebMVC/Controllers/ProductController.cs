using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using Microsoft.Extensions.Logging;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.ProductView)]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthorService _authorService;
        private readonly IPublisherService _publisherService;
        private readonly ICoverTypeService _coverTypeService;
        private readonly IDistributorService _distributorService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IAuthorService authorService,
            IPublisherService publisherService,
            ICoverTypeService coverTypeService,
            IDistributorService distributorService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _authorService = authorService;
            _publisherService = publisherService;
            _coverTypeService = coverTypeService;
            _distributorService = distributorService;
            _logger = logger;
        }
        public async Task<IActionResult> Index(string? name, string? author, string? publisher)
        {
            try
            {
                IEnumerable<ProductDTO> products;

                if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(author) || !string.IsNullOrEmpty(publisher))
                {
                    products = await _productService.SearchAsync(name, author, publisher);
                }
                else
                {
                    products = await _productService.GetAllAsync();
                }

                ViewBag.Name = name;
                ViewBag.Author = author;
                ViewBag.Publisher = publisher;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<ProductDTO>());
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                    return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
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

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;

                // Thêm categories
                if (categoryIds != null)
                {
                    model.Product_Categories = categoryIds.Select(id => new Product_category
                    {
                        Category_id = id,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    }).ToList();
                }

                // Thêm authors
                if (authorIds != null)
                {
                    model.Product_Authors = authorIds.Select(id => new Product_author
                    {
                        Author_id = id,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    }).ToList();
                }

                await _productService.CreateProductAsync(model);
                TempData["SuccessMessage"] = "Thêm s?n ph?m thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                await LoadDropdownsAsync();
                return View(model);
            }
        }
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetEntityByIdAsync(id);

                if (product == null)
                    return NotFound();

                await LoadDropdownsAsync(product);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> Edit(int id, Product model)
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

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;

                await _productService.UpdateProductAsync(model);
                TempData["SuccessMessage"] = "C?p nh?t s?n ph?m thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
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
                await _productService.DeleteProductAsync(id);
                TempData["SuccessMessage"] = "Xóa s?n ph?m thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(Product? product = null)
        {
            var categories = await _categoryService.GetAllAsync();
            var authors = await _authorService.GetAllAsync();
            var publishers = await _publisherService.GetAllAsync();
            var coverTypes = await _coverTypeService.GetAllAsync();
            var distributors = await _distributorService.GetAllAsync();

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
