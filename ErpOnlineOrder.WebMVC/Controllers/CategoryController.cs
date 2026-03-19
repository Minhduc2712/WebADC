using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

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
                SetErrorFromException(ex);
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

                var (created, error) = await _categoryApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm danh mục thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể thêm danh mục. Vui lòng kiểm tra mã danh mục và tên danh mục.");
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
                {
                    SetErrorMessage("Không tìm thấy danh mục.");
                    return RedirectToAction(nameof(Index));
                }

                return View(new UpdateCategoryDto { Id = category.Id, Category_code = category.Category_code, Category_name = category.Category_name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit");
                SetErrorFromException(ex);
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
                    SetSuccessMessage("Cập nhật danh mục thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể cập nhật danh mục. Dữ liệu có thể không hợp lệ hoặc danh mục không còn tồn tại.");
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
                    SetSuccessMessage("Xóa danh mục thành công!");
                else
                    SetErrorMessage(error ?? "Không thể xóa danh mục. Danh mục có thể đang được sử dụng bởi sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
