using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.RegionView)]
    public class RegionController : BaseController
    {
        private readonly IRegionService _regionService;
        private readonly ILogger<RegionController> _logger;

        public RegionController(IRegionService regionService, ILogger<RegionController> logger)
        {
            _regionService = regionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var regions = await _regionService.GetAllAsync();
                return View(regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading regions");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
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

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _regionService.CreateRegionAsync(model, userId);
                TempData["SuccessMessage"] = "Thêm khu vực thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating region");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [RequirePermission(PermissionCodes.RegionUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var region = await _regionService.GetByIdAsync(id);
                if (region == null)
                    return NotFound();

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
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
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

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _regionService.UpdateRegionAsync(model, userId);
                TempData["SuccessMessage"] = "Cập nhật khu vực thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating region");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RegionDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _regionService.DeleteRegionAsync(id);
                TempData["SuccessMessage"] = "Xóa khu vực thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
