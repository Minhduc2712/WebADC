using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ApiController
    {
        private readonly IAdminService _adminService;
        private readonly IPermissionService _permissionService;

        public AdminController(IAdminService adminService, IPermissionService permissionService)
        {
            _adminService = adminService;
            _permissionService = permissionService;
        }

        #region Quản lý tài khoản nhân viên
        [HttpGet("staff")]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _adminService.GetAllStaffAsync(TryGetCurrentUserId());
            return Ok(staff);
        }

        [HttpGet("staff/paged")]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> GetStaffPaged([FromQuery] StaffFilterRequest request)
        {
            var result = await _adminService.GetStaffPagedAsync(request, TryGetCurrentUserId());
            return Ok(result);
        }
        [HttpGet("staff/{userId}")]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> GetStaffById(int userId)
        {
            var staff = await _adminService.GetStaffByIdAsync(userId, TryGetCurrentUserId());
            if (staff == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }
            return Ok(staff);
        }
        [HttpPost("staff")]
        [RequirePermission(PermissionCodes.StaffCreate)]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffAccountDto dto)
        {
            try
            {
                var result = await _adminService.CreateStaffAccountAsync(dto, GetCurrentUserId());
                if (result == null)
                {
                    return BadRequest(new { message = "Không thể tạo tài khoản nhân viên. Tên đăng nhập, email hoặc mã nhân viên có thể đã tồn tại." });
                }
                return CreatedAtAction(nameof(GetStaffById), new { userId = result.User_id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("staff/{userId}")]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> UpdateStaff(int userId, [FromBody] UpdateStaffAccountDto dto)
        {
            if (userId != dto.User_id)
            {
                return BadRequest(new { message = "User ID trên URL không khớp với dữ liệu cập nhật nhân viên." });
            }

            try
            {
                var result = await _adminService.UpdateStaffAccountAsync(dto, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên cần cập nhật." });
                }
                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("staff/{userId}")]
        [RequirePermission(PermissionCodes.StaffDelete)]
        public async Task<IActionResult> DeleteStaff(int userId)
        {
            var result = await _adminService.DeleteStaffAccountAsync(userId, GetCurrentUserId());
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên cần xóa." });
            }
            return Ok(new { success = true, message = "Xóa thành công" });
        }
        [HttpPatch("staff/{userId}/status")]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ToggleStaffStatus(int userId, [FromQuery] bool isActive)
        {
            var result = await _adminService.ToggleStaffStatusAsync(userId, isActive, GetCurrentUserId());
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên để cập nhật trạng thái hoạt động." });
            }
            return Ok(new { success = true, message = isActive ? "Đã kích hoạt tài khoản" : "Đã vô hiệu hóa tài khoản" });
        }
        [HttpPost("staff/{userId}/reset-password")]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
        {
            if (userId != dto.User_id)
            {
                return BadRequest(new { message = "User ID trên URL không khớp với dữ liệu đặt lại mật khẩu." });
            }

            var result = await _adminService.ResetPasswordAsync(dto, GetCurrentUserId());
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên để đặt lại mật khẩu." });
            }
            return Ok(new { success = true, message = "Đã reset mật khẩu thành công" });
        }

        #endregion

        #region Quản lý Roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            return Ok(roles);
        }

        #endregion
    }
}
