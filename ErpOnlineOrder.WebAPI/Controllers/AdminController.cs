using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
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

        #region Qu?n lý tài kho?n nhân viên
        [HttpGet("staff")]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _adminService.GetAllStaffAsync();
            return Ok(staff);
        }
        [HttpGet("staff/{userId}")]
        [RequirePermission(PermissionCodes.StaffView)]
        public async Task<IActionResult> GetStaffById(int userId)
        {
            var staff = await _adminService.GetStaffByIdAsync(userId);
            if (staff == null)
            {
                return NotFound(new { message = "Nhân viên không t?n t?i" });
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
                    return BadRequest(new { message = "Không th? t?o tài kho?n nhân viên" });
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
                return BadRequest(new { message = "User ID không kh?p" });
            }

            try
            {
                var result = await _adminService.UpdateStaffAccountAsync(dto, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(new { message = "Nhân viên không t?n t?i" });
                }
                return Ok(new { success = true, message = "C?p nh?t thành công" });
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
                return NotFound(new { message = "Nhân viên không t?n t?i" });
            }
            return Ok(new { success = true, message = "X�a th�nh c�ng" });
        }
        [HttpPatch("staff/{userId}/status")]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ToggleStaffStatus(int userId, [FromQuery] bool isActive)
        {
            var result = await _adminService.ToggleStaffStatusAsync(userId, isActive, GetCurrentUserId());
            if (!result)
            {
                return NotFound(new { message = "Nhân viên không t?n t?i" });
            }
            return Ok(new { success = true, message = isActive ? "?ã kích ho?t tài kho?n" : "?ã vô hi?u hóa tài kho?n" });
        }
        [HttpPost("staff/{userId}/reset-password")]
        [RequirePermission(PermissionCodes.StaffUpdate)]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
        {
            if (userId != dto.User_id)
            {
                return BadRequest(new { message = "User ID không kh?p" });
            }

            var result = await _adminService.ResetPasswordAsync(dto, GetCurrentUserId());
            if (!result)
            {
                return NotFound(new { message = "Nhân viên không t?n t?i" });
            }
            return Ok(new { success = true, message = "?ã reset m?t kh?u thành công" });
        }

        #endregion

        #region Qu?n lý Roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            return Ok(roles);
        }

        #endregion
    }
}
