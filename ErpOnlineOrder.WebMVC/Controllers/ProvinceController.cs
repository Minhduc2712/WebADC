using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.ProvinceView)]
    public class ProvinceController : BaseController
    {
        private readonly IProvinceService _provinceService;
        private readonly IRegionService _regionService;
        private readonly ILogger<ProvinceController> _logger;

        public ProvinceController(
            IProvinceService provinceService, 
            IRegionService regionService,
            ILogger<ProvinceController> logger)
        {
            _provinceService = provinceService;
            _regionService = regionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? regionId)
        {
            try
            {
                IEnumerable<ProvinceDTO> provinces;
                
                if (regionId.HasValue && regionId.Value > 0)
                {
                    provinces = await _provinceService.GetByRegionIdAsync(regionId.Value);
                }
                else
                {
                    provinces = await _provinceService.GetAllAsync();
                }

                var regions = await _regionService.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", regionId);
                ViewBag.SelectedRegionId = regionId;

                return View(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading provinces");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<ProvinceDTO>());
            }
        }

        [RequirePermission(PermissionCodes.ProvinceCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var regions = await _regionService.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProvinceCreate)]
        public async Task<IActionResult> Create(CreateProvinceDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var regions = await _regionService.GetAllAsync();
                    ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                    return View(model);
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _provinceService.CreateProvinceAsync(model, userId);
                TempData["SuccessMessage"] = "Thêm tỉnh/thành phố thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating province");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var regions = await _regionService.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                return View(model);
            }
        }

        [RequirePermission(PermissionCodes.ProvinceUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var province = await _provinceService.GetByIdAsync(id);
                if (province == null)
                    return NotFound();

                var model = new UpdateProvinceDto
                {
                    Id = province.Id,
                    Province_code = province.Province_code,
                    Province_name = province.Province_name,
                    Region_id = province.Region_id
                };

                var regions = await _regionService.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", province.Region_id);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading province for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProvinceUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateProvinceDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    var regions = await _regionService.GetAllAsync();
                    ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                    return View(model);
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _provinceService.UpdateProvinceAsync(model, userId);
                TempData["SuccessMessage"] = "Cập nhật tỉnh/thành phố thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating province");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var regions = await _regionService.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProvinceDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _provinceService.DeleteProvinceAsync(id);
                TempData["SuccessMessage"] = "Xóa tỉnh/thành phố thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting province");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
