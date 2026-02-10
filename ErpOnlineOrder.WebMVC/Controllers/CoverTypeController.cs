using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
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
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<CoverTypeDto>());
            }
        }

        [RequirePermission("CATEGORY_CREATE")]
        public IActionResult Create()
        {
            return View(new CreateCoverTypeDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_CREATE")]
        public async Task<IActionResult> Create(CreateCoverTypeDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _coverTypeApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm loại bìa thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm loại bìa thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cover type");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var coverType = await _coverTypeApiClient.GetByIdAsync(id);
                if (coverType == null)
                    return NotFound();

                var updateDto = new UpdateCoverTypeDto { Id = coverType.Id, Cover_type_code = coverType.Cover_type_code, Cover_type_name = coverType.Cover_type_name };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cover type for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_UPDATE")]
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
                    TempData["SuccessMessage"] = "Cập nhật loại bìa thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
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
        [RequirePermission("CATEGORY_DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _coverTypeApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa loại bìa thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cover type");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
