using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CategoryView)]
    public class CoverTypeController : BaseController
    {
        private readonly ICoverTypeApiClient _coverTypeApiClient;
        private readonly ILogger<CoverTypeController> _logger;

        public CoverTypeController(ICoverTypeApiClient coverTypeApiClient, ILogger<CoverTypeController> logger)
        {
            _coverTypeApiClient = coverTypeApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var coverTypes = await _coverTypeApiClient.GetAllAsync();
                return View(coverTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cover types");
                SetErrorFromException(ex);
                return View(Enumerable.Empty<CoverTypeDto>());
            }
        }

        [RequirePermission(PermissionCodes.CategoryCreate)]
        public IActionResult Create()
        {
            return View(new CreateCoverTypeDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryCreate)]
        public async Task<IActionResult> Create(CreateCoverTypeDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var (created, error) = await _coverTypeApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm loại bìa thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể thêm loại bìa. Vui lòng kiểm tra mã loại bìa và tên loại bìa.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cover type");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.CategoryUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var coverType = await _coverTypeApiClient.GetByIdAsync(id);
                if (coverType == null)
                {
                    SetErrorMessage("Không tìm thấy loại bìa.");
                    return RedirectToAction(nameof(Index));
                }

                var updateDto = new UpdateCoverTypeDto { Id = coverType.Id, Cover_type_code = coverType.Cover_type_code, Cover_type_name = coverType.Cover_type_name };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cover type for edit");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateCoverTypeDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _coverTypeApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật loại bìa thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể cập nhật loại bìa. Dữ liệu có thể không hợp lệ hoặc bản ghi không còn tồn tại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cover type");
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
                var (success, error) = await _coverTypeApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa loại bìa thành công!");
                else
                    SetErrorMessage(error ?? "Không thể xóa loại bìa. Loại bìa có thể đang được sử dụng bởi sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cover type");
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
