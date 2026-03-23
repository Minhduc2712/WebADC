using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.StaffRegionRuleView)]
    public class StaffRegionRuleController : BaseController
    {
        private readonly IStaffRegionRuleApiClient _staffRegionRuleApiClient;
        private readonly IAdminApiClient _adminApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly IWardApiClient _wardApiClient;
        private readonly ILogger<StaffRegionRuleController> _logger;

        public StaffRegionRuleController(
            IStaffRegionRuleApiClient staffRegionRuleApiClient,
            IAdminApiClient adminApiClient,
            IProvinceApiClient provinceApiClient,
            IWardApiClient wardApiClient,
            ILogger<StaffRegionRuleController> logger)
        {
            _staffRegionRuleApiClient = staffRegionRuleApiClient;
            _adminApiClient = adminApiClient;
            _provinceApiClient = provinceApiClient;
            _wardApiClient = wardApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? staffId, int? provinceId)
        {
            try
            {
                var rules = await _staffRegionRuleApiClient.GetAllAsync(staffId, provinceId);
                var provinces = await _provinceApiClient.GetAllAsync();
                var staffList = await _adminApiClient.GetAllStaffAsync();

                ViewBag.Provinces = provinces.ToList();
                ViewBag.StaffList = staffList.ToList();
                ViewBag.FilterStaffId = staffId;
                ViewBag.FilterProvinceId = provinceId;

                var permissions = HttpContext.Session.GetString("Permissions") ?? "";
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                bool isAdmin = roles.Contains("ROLE_ADMIN");
                ViewBag.CanCreate = isAdmin || permissions.Contains(PermissionCodes.StaffRegionRuleCreate);
                ViewBag.CanUpdate = isAdmin || permissions.Contains(PermissionCodes.StaffRegionRuleUpdate);
                ViewBag.CanDelete = isAdmin || permissions.Contains(PermissionCodes.StaffRegionRuleDelete);

                return View(rules.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách quy tắc phân công vùng");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(new List<StaffRegionRuleDto>());
            }
        }

        [RequirePermission(PermissionCodes.StaffRegionRuleCreate)]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new CreateStaffRegionRuleDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffRegionRuleCreate)]
        public async Task<IActionResult> Create(int Staff_id, int Province_id, [FromForm] int[] Ward_ids)
        {
            var fallbackDto = new CreateStaffRegionRuleDto { Staff_id = Staff_id, Province_id = Province_id };

            if (Staff_id <= 0)
            {
                ModelState.AddModelError("Staff_id", "Vui lòng chọn cán bộ");
                await LoadDropdownsAsync(Province_id > 0 ? Province_id : null);
                return View(fallbackDto);
            }
            if (Province_id <= 0)
            {
                ModelState.AddModelError("Province_id", "Vui lòng chọn tỉnh/thành");
                await LoadDropdownsAsync();
                return View(fallbackDto);
            }

            var dto = new CreateStaffRegionRuleDto
            {
                Staff_id = Staff_id,
                Province_id = Province_id,
                Ward_ids = Ward_ids.Length > 0 ? Ward_ids.ToList() : null
            };

            var (data, error) = await _staffRegionRuleApiClient.CreateAsync(dto);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                await LoadDropdownsAsync(Province_id);
                return View(fallbackDto);
            }

            SetSuccessMessage("Đã thêm quy tắc phân công vùng thành công");
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission(PermissionCodes.StaffRegionRuleUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            var rule = await _staffRegionRuleApiClient.GetByIdAsync(id);
            if (rule == null)
            {
                SetErrorMessage("Không tìm thấy quy tắc phân công");
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync(rule.Province_id);
            ViewBag.RuleId = id;
            ViewBag.ExistingWardIds = rule.Ward_ids ?? new List<int>();
            return View(new UpdateStaffRegionRuleDto
            {
                Staff_id = rule.Staff_id,
                Province_id = rule.Province_id,
                Ward_ids = rule.Ward_ids ?? new List<int>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffRegionRuleUpdate)]
        public async Task<IActionResult> Edit(int id, int Staff_id, int Province_id, [FromForm] int[] Ward_ids)
        {
            var dto = new UpdateStaffRegionRuleDto
            {
                Staff_id = Staff_id,
                Province_id = Province_id,
                Ward_ids = Ward_ids.Length > 0 ? Ward_ids.ToList() : null
            };

            var (data, error) = await _staffRegionRuleApiClient.UpdateAsync(id, dto);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                await LoadDropdownsAsync(Province_id);
                ViewBag.RuleId = id;
                ViewBag.ExistingWardIds = dto.Ward_ids ?? new List<int>();
                return View(dto);
            }

            SetSuccessMessage("Đã cập nhật quy tắc phân công vùng thành công");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffRegionRuleDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _staffRegionRuleApiClient.DeleteAsync(id);
            if (!success)
                SetErrorMessage(error ?? "Không thể xóa quy tắc phân công");
            else
                SetSuccessMessage("Đã xóa quy tắc phân công vùng");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetWardsByProvince(int provinceId, int? excludeRuleId = null)
        {
            var wards = await _wardApiClient.GetByProvinceIdAsync(provinceId);

            // Lấy tất cả ward đã được dùng bởi rule khác trong cùng tỉnh
            var existingRules = await _staffRegionRuleApiClient.GetAllAsync(provinceId: provinceId);
            var usedWardIds = existingRules
                .Where(r => excludeRuleId == null || r.Id != excludeRuleId.Value)
                .SelectMany(r => r.Ward_ids ?? new System.Collections.Generic.List<int>())
                .ToHashSet();

            return Json(wards.Select(w => new
            {
                w.Id,
                w.Ward_name,
                isUsed = usedWardIds.Contains(w.Id)
            }));
        }

        private async Task LoadDropdownsAsync(int? selectedProvinceId = null)
        {
            var provinces = await _provinceApiClient.GetAllAsync();
            var staffList = await _adminApiClient.GetAllStaffAsync();
            ViewBag.Provinces = provinces.ToList();
            ViewBag.StaffList = staffList.ToList();

            if (selectedProvinceId.HasValue)
            {
                var wards = await _wardApiClient.GetByProvinceIdAsync(selectedProvinceId.Value);
                ViewBag.Wards = wards.ToList();
            }
            else
            {
                ViewBag.Wards = new List<object>();
            }
        }
    }
}
