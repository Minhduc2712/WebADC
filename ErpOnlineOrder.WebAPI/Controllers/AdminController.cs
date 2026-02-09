using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
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
                // TODO: L?y userId t? token
                int createdBy = 1;

                var result = await _adminService.CreateStaffAccountAsync(dto, createdBy);
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
                // TODO: L?y userId t? token
                int updatedBy = 1;

                var result = await _adminService.UpdateStaffAccountAsync(dto, updatedBy);
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
            // TODO: L?y userId t? token
            int deletedBy = 1;

            var result = await _adminService.DeleteStaffAccountAsync(userId, deletedBy);
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
            // TODO: L?y userId t? token
            int updatedBy = 1;

            var result = await _adminService.ToggleStaffStatusAsync(userId, isActive, updatedBy);
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

            // TODO: L?y userId t? token
            int updatedBy = 1;

            var result = await _adminService.ResetPasswordAsync(dto, updatedBy);
            if (!result)
            {
                return NotFound(new { message = "Nhân viên không t?n t?i" });
            }
            return Ok(new { success = true, message = "?ã reset m?t kh?u thành công" });
        }

        #endregion

        #region Th?ng kê Dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var staffCount = await _adminService.GetActiveStaffCountAsync();
            var customerCount = await _adminService.GetCustomerCountAsync();
            var orderCount = await _adminService.GetOrderCountAsync();

            return Ok(new
            {
                staff_count = staffCount,
                customer_count = customerCount,
                order_count = orderCount
            });
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
