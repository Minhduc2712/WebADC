using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IAdminService _adminService;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(
            IPermissionService permissionService,
            IAdminService adminService,
            ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _adminService = adminService;
            _logger = logger;
        }

        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        #region Danh sách Permission
        [HttpGet]
        [RequirePermission(PermissionCodes.PermissionView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                await LoadCurrentUserPermissions();
                return View(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách Permission");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<PermissionDto>());
            }
        }

        #endregion

        #region Tạo Permission mới
        [HttpGet]
        [RequirePermission(PermissionCodes.PermissionCreate)]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PermissionCreate)]
        public async Task<IActionResult> Create(string Permission_code)
        {
            if (string.IsNullOrWhiteSpace(Permission_code))
            {
                ModelState.AddModelError("Permission_code", "Mã quyền không được để trống");
                return View();
            }

            var code = Permission_code.Trim().ToUpper();

            // Validate format MODULE_ACTION
            if (!code.Contains('_'))
            {
                ModelState.AddModelError("Permission_code", "Mã quyền phải theo định dạng MODULE_ACTION (VD: PRODUCT_VIEW)");
                ViewBag.InputCode = Permission_code;
                return View();
            }

            try
            {
                // Kiểm tra trùng
                if (await _permissionService.IsPermissionCodeExistsAsync(code))
                {
                    ModelState.AddModelError("Permission_code", $"Mã quyền '{code}' đã tồn tại");
                    ViewBag.InputCode = Permission_code;
                    return View();
                }

                var success = await _permissionService.CreatePermissionAsync(code, GetCurrentUserId());

                if (success)
                {
                    SetSuccessMessage($"Tạo quyền '{code}' thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Tạo quyền thất bại. Mã quyền có thể đã tồn tại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Permission");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            ViewBag.InputCode = Permission_code;
            return View();
        }

        #endregion

        #region Sửa Permission
        [HttpGet]
        [RequirePermission(PermissionCodes.PermissionUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                if (permission == null)
                    return NotFound();

                return View(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form sửa Permission");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PermissionUpdate)]
        public async Task<IActionResult> Edit(int id, string Permission_code)
        {
            if (string.IsNullOrWhiteSpace(Permission_code))
            {
                ModelState.AddModelError("Permission_code", "Mã quyền không được để trống");
                var perm = await _permissionService.GetPermissionByIdAsync(id);
                return View(perm);
            }

            var code = Permission_code.Trim().ToUpper();

            if (!code.Contains('_'))
            {
                ModelState.AddModelError("Permission_code", "Mã quyền phải theo định dạng MODULE_ACTION (VD: PRODUCT_VIEW)");
                var perm = await _permissionService.GetPermissionByIdAsync(id);
                if (perm != null) perm.Permission_code = Permission_code;
                return View(perm);
            }

            try
            {
                if (await _permissionService.IsPermissionCodeExistsAsync(code, id))
                {
                    ModelState.AddModelError("Permission_code", $"Mã quyền '{code}' đã tồn tại");
                    var perm = await _permissionService.GetPermissionByIdAsync(id);
                    if (perm != null) perm.Permission_code = Permission_code;
                    return View(perm);
                }

                var success = await _permissionService.UpdatePermissionAsync(id, code, GetCurrentUserId());

                if (success)
                {
                    SetSuccessMessage("Cập nhật quyền thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Permission");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            var permDto = await _permissionService.GetPermissionByIdAsync(id);
            return View(permDto);
        }

        #endregion

        #region Xóa Permission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PermissionDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _permissionService.DeletePermissionAsync(id);

                if (success)
                    SetSuccessMessage("Xóa quyền thành công!");
                else
                    SetErrorMessage("Xóa quyền thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Permission");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Gán quyền cho User
        [HttpGet]
        [RequirePermission(PermissionCodes.PermissionAssignUser)]
        public async Task<IActionResult> UserPermissions()
        {
            try
            {
                var staffList = await _adminService.GetAllStaffAsync();
                return View(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<Application.DTOs.AdminDTOs.StaffAccountDto>());
            }
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.PermissionAssignUser)]
        public async Task<IActionResult> UserPermissionDetails(int id)
        {
            try
            {
                var userFullPermissions = await _permissionService.GetUserFullPermissionsAsync(id);
                if (userFullPermissions == null)
                    return NotFound();

                var allPermissions = await _permissionService.GetAllPermissionsAsync();

                ViewBag.AllPermissions = allPermissions.ToList();
                ViewBag.GroupedPermissions = allPermissions
                    .GroupBy(p => p.Module_name)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return View(userFullPermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết quyền user");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(UserPermissions));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PermissionAssignUser)]
        public async Task<IActionResult> AssignToUser(int userId, List<int> Permission_ids, DateTime? ExpiresAt, string? Note)
        {
            try
            {
                var dto = new AssignPermissionsToUserDto
                {
                    User_id = userId,
                    Permission_ids = Permission_ids ?? new List<int>(),
                    ExpiresAt = ExpiresAt,
                    Note = Note
                };

                var success = await _permissionService.AssignPermissionsToUserAsync(dto, GetCurrentUserId());

                if (success)
                {
                    SetSuccessMessage("Gán quyền cho user thành công!");
                }
                else
                {
                    SetErrorMessage("Gán quyền thất bại!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán quyền cho user");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(UserPermissionDetails), new { id = userId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PermissionAssignUser)]
        public async Task<IActionResult> RemoveAllUserPermissions(int userId)
        {
            try
            {
                await _permissionService.RemoveAllDirectPermissionsFromUserAsync(userId);
                SetSuccessMessage("Đã xóa tất cả quyền trực tiếp!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa quyền user");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(UserPermissionDetails), new { id = userId });
        }

        #endregion

        #region Helper Methods

        private async Task LoadCurrentUserPermissions()
        {
            try
            {
                // ROLE_ADMIN có toàn quyền
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains("ROLE_ADMIN"))
                {
                    ViewBag.CanCreate = true;
                    ViewBag.CanUpdate = true;
                    ViewBag.CanDelete = true;
                    ViewBag.CanAssign = true;
                    return;
                }

                var userId = GetCurrentUserId();
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.PermissionCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.PermissionUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.PermissionDelete) ?? false;
                ViewBag.CanAssign = permissions?.Contains(PermissionCodes.PermissionAssignUser) ?? false;
            }
            catch
            {
                ViewBag.CanCreate = false;
                ViewBag.CanUpdate = false;
                ViewBag.CanDelete = false;
                ViewBag.CanAssign = false;
            }
        }

        #endregion
    }
}
