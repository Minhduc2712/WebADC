using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.ProvinceView)]
    public class ProvinceController : BaseController
    {
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly IRegionApiClient _regionApiClient;
        private readonly ILogger<ProvinceController> _logger;

        public ProvinceController(
            IProvinceApiClient provinceApiClient,
            IRegionApiClient regionApiClient,
            ILogger<ProvinceController> logger)
        {
            _provinceApiClient = provinceApiClient;
            _regionApiClient = regionApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? regionId)
        {
            try
            {
                IEnumerable<ProvinceDTO> provinces;
                if (regionId.HasValue && regionId.Value > 0)
                    provinces = await _provinceApiClient.GetByRegionIdAsync(regionId.Value);
                else
                    provinces = await _provinceApiClient.GetAllAsync();

                var regions = await _regionApiClient.GetAllAsync();
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
                var regions = await _regionApiClient.GetAllAsync();
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
                    var regions = await _regionApiClient.GetAllAsync();
                    ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                    return View(model);
                }

                var created = await _provinceApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm tỉnh/thành phố thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm tỉnh/thành phố thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating province");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var regions = await _regionApiClient.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.ProvinceUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var province = await _provinceApiClient.GetByIdAsync(id);
                if (province == null)
                    return NotFound();

                var model = new UpdateProvinceDto
                {
                    Id = province.Id,
                    Province_code = province.Province_code,
                    Province_name = province.Province_name,
                    Region_id = province.Region_id
                };

                var regions = await _regionApiClient.GetAllAsync();
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
                    var regions = await _regionApiClient.GetAllAsync();
                    ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
                    return View(model);
                }

                var (success, error) = await _provinceApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật tỉnh/thành phố thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating province");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var regions = await _regionApiClient.GetAllAsync();
                ViewBag.Regions = new SelectList(regions, "Id", "Region_name", model.Region_id);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.ProvinceDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _provinceApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa tỉnh/thành phố thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
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
