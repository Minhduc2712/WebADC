using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IPermissionService permissionService, ILogger<RoleController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        #region Danh sách Role
        [HttpGet]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _permissionService.GetAllRolesAsync();
                await LoadCurrentUserPermissions();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<RoleDto>());
            }
        }

        #endregion

        #region Tạo Role mới
        [HttpGet]
        [RequirePermission(PermissionCodes.RoleCreate)]
        public async Task<IActionResult> Create()
        {
            await LoadPermissionsAsync();
            return View(new CreateRoleDto());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RoleCreate)]
        public async Task<IActionResult> Create(CreateRoleDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Role_name))
            {
                ModelState.AddModelError("Role_name", "Tên role không được để trống");
                await LoadPermissionsAsync();
                return View(model);
            }

            try
            {
                var success = await _permissionService.CreateRoleAsync(model, GetCurrentUserId());

                if (success)
                {
                    SetSuccessMessage($"Tạo role '{model.Role_name}' thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Tạo role thất bại. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Role");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadPermissionsAsync();
            return View(model);
        }

        #endregion

        #region Cập nhật Role
        [HttpGet]
        [RequirePermission(PermissionCodes.RoleUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rolePermission = await _permissionService.GetRolePermissionsAsync(id);
                
                if (rolePermission == null)
                    return NotFound();

                var model = new UpdateRoleDto
                {
                    Id = rolePermission.Role_id,
                    Role_name = rolePermission.Role_name
                };

                ViewBag.CurrentPermissions = rolePermission.Permissions.Select(p => p.Id).ToList();
                await LoadPermissionsAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form chỉnh sửa Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RoleUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateRoleDto model)
        {
            if (id != model.Id)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Role_name))
            {
                ModelState.AddModelError("Role_name", "Tên role không được để trống");
                await LoadPermissionsAsync();
                return View(model);
            }

            try
            {
                var success = await _permissionService.UpdateRoleAsync(model, GetCurrentUserId());

                if (success)
                {
                    SetSuccessMessage("Cập nhật role thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Cập nhật thất bại. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Role");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadPermissionsAsync();
            return View(model);
        }

        #endregion

        #region Xóa Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RoleDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _permissionService.DeleteRoleAsync(id);

                if (success)
                {
                    SetSuccessMessage("Xóa role thành công!");
                }
                else
                {
                    SetErrorMessage("Xóa role thất bại!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Gán quyền cho Role
        [HttpGet]
        [RequirePermission(PermissionCodes.RoleAssignPermission)]
        public async Task<IActionResult> AssignPermissions(int id)
        {
            try
            {
                var rolePermission = await _permissionService.GetRolePermissionsAsync(id);
                
                if (rolePermission == null)
                    return NotFound();

                var allPermissions = await _permissionService.GetAllPermissionsAsync();
                var currentPermissionIds = rolePermission.Permissions.Select(p => p.Id).ToList();

                ViewBag.RoleId = id;
                ViewBag.RoleName = rolePermission.Role_name;
                ViewBag.CurrentPermissions = currentPermissionIds;
                ViewBag.AllPermissions = allPermissions.ToList();

                // Group permissions by module
                var groupedPermissions = allPermissions
                    .GroupBy(p => p.Module_name)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());
                ViewBag.GroupedPermissions = groupedPermissions;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form gán quyền cho Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.RoleAssignPermission)]
        public async Task<IActionResult> AssignPermissions(int id, List<int> Permission_ids)
        {
            try
            {
                var dto = new AssignPermissionsToRoleDto
                {
                    Role_id = id,
                    Permission_ids = Permission_ids ?? new List<int>()
                };

                var success = await _permissionService.AssignPermissionsToRoleAsync(dto);

                if (success)
                {
                    SetSuccessMessage("Gán quyền thành công!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Gán quyền thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán quyền cho Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(AssignPermissions), new { id });
        }

        #endregion

        #region Xem quyền của Role
        [HttpGet]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rolePermission = await _permissionService.GetRolePermissionsAsync(id);
                
                if (rolePermission == null)
                    return NotFound();

                // Group permissions by module
                var groupedPermissions = rolePermission.Permissions
                    .GroupBy(p => p.Module_name)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());
                ViewBag.GroupedPermissions = groupedPermissions;
                await LoadCurrentUserPermissions();

                return View(rolePermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết Role");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Helper Methods
        private async Task LoadPermissionsAsync()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                var groupedPermissions = permissions
                    .GroupBy(p => p.Module_name)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());
                ViewBag.GroupedPermissions = groupedPermissions;
                ViewBag.AllPermissions = permissions.ToList();
            }
            catch
            {
                ViewBag.GroupedPermissions = new Dictionary<string, List<PermissionDto>>();
                ViewBag.AllPermissions = new List<PermissionDto>();
            }
        }
        private async Task LoadCurrentUserPermissions()
        {
            try
            {
                // ROLE_ADMIN có toàn quyền
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains("ROLE_ADMIN"))
                {
                    ViewBag.CurrentUserPermissions = new List<string> { "ALL" };
                    ViewBag.CanCreate = true;
                    ViewBag.CanUpdate = true;
                    ViewBag.CanDelete = true;
                    ViewBag.CanAssign = true;
                    return;
                }

                var userId = GetCurrentUserId();
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();
                
                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.RoleCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.RoleUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.RoleDelete) ?? false;
                ViewBag.CanAssign = permissions?.Contains(PermissionCodes.RoleAssignPermission) ?? false;
            }
            catch
            {
                ViewBag.CurrentUserPermissions = new List<string>();
                ViewBag.CanCreate = false;
                ViewBag.CanUpdate = false;
                ViewBag.CanDelete = false;
                ViewBag.CanAssign = false;
            }
        }

        #endregion
    }
}
