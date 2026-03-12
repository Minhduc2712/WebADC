using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.RegionView)]
    public class RegionController : BaseController
    {
        private readonly IRegionApiClient _regionApiClient;
        private readonly ILogger<RegionController> _logger;

        public RegionController(IRegionApiClient regionApiClient, ILogger<RegionController> logger)
        {
            _regionApiClient = regionApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var regions = await _regionApiClient.GetAllAsync();
                return View(regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading regions");
                SetErrorFromException(ex);
                return View(Enumerable.Empty<RegionDTO>());
            }
        }

        [RequirePermission(PermissionCodes.RegionCreate)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RegionCreate)]
        public async Task<IActionResult> Create(CreateRegionDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _regionApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm khu vực thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm khu vực thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating region");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.RegionUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var region = await _regionApiClient.GetByIdAsync(id);
                if (region == null)
                {
                    SetErrorMessage("Không tìm thấy vùng.");
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateRegionDto
                {
                    Id = region.Id,
                    Region_code = region.Region_code,
                    Region_name = region.Region_name
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading region for edit");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RegionUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateRegionDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _regionApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật khu vực thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating region");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RegionDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _regionApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa khu vực thành công!");
                else
                    SetErrorMessage(error ?? "Xóa thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region");
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
