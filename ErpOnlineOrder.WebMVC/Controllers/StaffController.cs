using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class StaffController : BaseController
    {
        private readonly IAdminApiClient _adminApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IAdminApiClient adminApiClient, IPermissionApiClient permissionApiClient, ILogger<StaffController> logger)
        {
            _adminApiClient = adminApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
        }
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        #region Danh sách nhân viên
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var staffList = await _adminApiClient.GetAllStaffAsync();

                // Truyền quyền của user hiện tại để hiển thị/ẩn các nút
                await LoadCurrentUserPermissions();

                return View(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<StaffAccountDto>());
            }
        }

        #endregion

        #region Chi tiết nhân viên
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var userFullPermissions = await _permissionApiClient.GetUserFullPermissionsAsync(id);
                ViewBag.UserFullPermissions = userFullPermissions;
                await LoadCurrentUserPermissions();

                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Tạo nhân viên mới
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffCreate)]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách roles để hiển thị trong dropdown
            await LoadRolesAsync();

            return View(new CreateStaffAccountDto());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffCreate)]
        public async Task<IActionResult> Create(CreateStaffAccountDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesAsync();
                return View(model);
            }

            try
            {
                var result = await _adminApiClient.CreateStaffAsync(model);

                if (result != null)
                {
                    SetSuccessMessage($"Tạo tài khoản nhân viên '{model.Username}' thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Tạo tài khoản thất bại. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tài khoản nhân viên");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadRolesAsync();
            return View(model);
        }

        #endregion

        #region Cập nhật nhân viên
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var allRoles = await _permissionApiClient.GetAllRolesAsync();
                var currentRoleIds = new List<int>();
                
                // Map tên role sang ID
                if (staff.Roles != null && staff.Roles.Any())
                {
                    foreach (var roleName in staff.Roles)
                    {
                        var role = allRoles.FirstOrDefault(r => r.Role_name == roleName);
                        if (role != null)
                        {
                            currentRoleIds.Add(role.Id);
                        }
                    }
                }

                var model = new UpdateStaffAccountDto
                {
                    User_id = staff.User_id,
                    Email = staff.Email,
                    Full_name = staff.Full_name,
                    Phone_number = staff.Phone_number,
                    Is_active = staff.Is_active,
                    Role_ids = currentRoleIds
                };

                ViewBag.StaffCode = staff.Staff_code;
                ViewBag.Username = staff.Username;
                ViewBag.CurrentRoles = staff.Roles ?? new List<string>();
                ViewBag.Roles = allRoles.ToList();
                await LoadCurrentUserPermissions();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form chỉnh sửa nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateStaffAccountDto model)
        {
            if (id != model.User_id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadEditViewDataAsync(id);
                return View(model);
            }

            try
            {
                var (success, _) = await _adminApiClient.UpdateStaffAsync(id, model);

                if (success)
                {
                    SetSuccessMessage("Cập nhật thông tin nhân viên thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Cập nhật thất bại. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật nhân viên");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadEditViewDataAsync(id);
            return View(model);
        }

        #endregion

        #region Xóa nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, _) = await _adminApiClient.DeleteStaffAsync(id);

                if (success)
                {
                    SetSuccessMessage("Xóa tài khoản nhân viên thành công!");
                }
                else
                {
                    SetErrorMessage("Xóa tài khoản thất bại! Nhân viên không tồn tại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Kích hoạt/Vô hiệu hóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            try
            {
                var (success, _) = await _adminApiClient.ToggleStaffStatusAsync(id, isActive);

                if (success)
                {
                    var status = isActive ? "kích hoạt" : "vô hiệu hóa";
                    SetSuccessMessage($"Đã {status} tài khoản thành công!");
                }
                else
                {
                    SetErrorMessage("Thao tác thất bại! Tài khoản không tồn tại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái nhân viên");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Đặt lại mật khẩu
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var model = new ResetPasswordDto
                {
                    User_id = id
                };

                ViewBag.StaffName = staff.Full_name;
                ViewBag.Username = staff.Username;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form đặt lại mật khẩu");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadResetPasswordViewDataAsync(model.User_id);
                return View(model);
            }

            try
            {
                var (success, _) = await _adminApiClient.ResetPasswordAsync(model);

                if (success)
                {
                    SetSuccessMessage("Đặt lại mật khẩu thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Đặt lại mật khẩu thất bại! Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại mật khẩu");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadResetPasswordViewDataAsync(model.User_id);
            return View(model);
        }

        #endregion

        #region Gán Role cho nhân viên
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffAssignRole)]
        public async Task<IActionResult> AssignRoles(int id)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var allRoles = await _permissionApiClient.GetAllRolesAsync();
                var currentRoleIds = new List<int>();
                
                // Map tên role sang ID
                if (staff.Roles != null && staff.Roles.Any())
                {
                    foreach (var roleName in staff.Roles)
                    {
                        var role = allRoles.FirstOrDefault(r => r.Role_name == roleName);
                        if (role != null)
                        {
                            currentRoleIds.Add(role.Id);
                        }
                    }
                }

                ViewBag.Staff = staff;
                ViewBag.AllRoles = allRoles.ToList();
                ViewBag.CurrentRoleIds = currentRoleIds;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form gán vai trò");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffAssignRole)]
        public async Task<IActionResult> AssignRoles(int id, List<int> Role_ids)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var model = new UpdateStaffAccountDto
                {
                    User_id = id,
                    Role_ids = Role_ids ?? new List<int>()
                };

                var (success, _) = await _adminApiClient.UpdateStaffAsync(id, model);

                if (success)
                {
                    SetSuccessMessage($"Gán vai trò cho '{staff.Full_name}' thành công!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Gán vai trò thất bại! Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán vai trò");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(AssignRoles), new { id });
        }

        #endregion

        #region Gán Permission TRỰC TIẾP cho nhân viên
        [HttpGet]
        [RequirePermission(PermissionCodes.StaffAssignRole)]
        public async Task<IActionResult> AssignPermissions(int id)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var allPermissions = await _permissionApiClient.GetAllPermissionsAsync();
                var userFullPermissions = await _permissionApiClient.GetUserFullPermissionsAsync(id);
                var currentDirectPermissionIds = userFullPermissions?.DirectPermissions
                    .Where(p => p.IsValid)
                    .Select(p => p.PermissionId)
                    .ToList() ?? new List<int>();

                // Group permissions by module
                var groupedPermissions = allPermissions
                    .GroupBy(p => p.Module_name)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.Staff = staff;
                ViewBag.GroupedPermissions = groupedPermissions;
                ViewBag.CurrentDirectPermissionIds = currentDirectPermissionIds;
                ViewBag.UserFullPermissions = userFullPermissions;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form gán quyền trực tiếp");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.StaffAssignRole)]
        public async Task<IActionResult> AssignPermissions(int id, List<int> Permission_ids, DateTime? ExpiresAt, string? Note)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(id);
                if (staff == null)
                    return NotFound();

                var dto = new AssignPermissionsToUserDto
                {
                    User_id = id,
                    Permission_ids = Permission_ids ?? new List<int>(),
                    ExpiresAt = ExpiresAt,
                    Note = Note
                };

                var (success, _) = await _permissionApiClient.AssignPermissionsToUserAsync(dto);

                if (success)
                {
                    SetSuccessMessage($"Gán quyền cho '{staff.Full_name}' thành công!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Gán quyền thất bại! Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán quyền trực tiếp");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(AssignPermissions), new { id });
        }

        #endregion

        #region Helper Methods
        private async Task LoadEditViewDataAsync(int userId)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(userId);
                ViewBag.StaffCode = staff?.Staff_code ?? "";
                ViewBag.Username = staff?.Username ?? "";
                ViewBag.CurrentRoles = staff?.Roles ?? new List<string>();
            }
            catch
            {
                ViewBag.StaffCode = "";
                ViewBag.Username = "";
                ViewBag.CurrentRoles = new List<string>();
            }
            await LoadRolesAsync();
            await LoadCurrentUserPermissions();
        }
        private async Task LoadResetPasswordViewDataAsync(int userId)
        {
            try
            {
                var staff = await _adminApiClient.GetStaffByIdAsync(userId);
                ViewBag.StaffName = staff?.Full_name ?? "";
                ViewBag.Username = staff?.Username ?? "";
            }
            catch
            {
                ViewBag.StaffName = "";
                ViewBag.Username = "";
            }
        }
        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _permissionApiClient.GetAllRolesAsync();
                ViewBag.Roles = roles?.ToList() ?? new List<RoleDto>();
            }
            catch
            {
                ViewBag.Roles = new List<RoleDto>();
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
                var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();
                
                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.StaffCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.StaffUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.StaffDelete) ?? false;
                ViewBag.CanAssign = permissions?.Contains(PermissionCodes.StaffAssignRole) ?? false;
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
