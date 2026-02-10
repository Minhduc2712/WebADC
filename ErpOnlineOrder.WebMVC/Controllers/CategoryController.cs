using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CategoryView)]
    public class CategoryController : BaseController
    {
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryApiClient categoryApiClient, ILogger<CategoryController> logger)
        {
            _categoryApiClient = categoryApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryApiClient.GetAllAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<CategoryDto>());
            }
        }

        [RequirePermission(PermissionCodes.CategoryCreate)]
        public IActionResult Create()
        {
            return View(new CreateCategoryDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryCreate)]
        public async Task<IActionResult> Create(CreateCategoryDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _categoryApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm danh mục thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.CategoryUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryApiClient.GetByIdAsync(id);
                if (category == null)
                    return NotFound();

                return View(new UpdateCategoryDto { Id = category.Id, Category_code = category.Category_code, Category_name = category.Category_name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _categoryApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _categoryApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa danh mục thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
