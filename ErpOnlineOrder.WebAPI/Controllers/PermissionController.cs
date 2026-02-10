using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    public class CreatePermissionRequest
    {
        public string Permission_code { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #region Permissions
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        [HttpGet("permissions/module/{moduleCode}")]
        public async Task<IActionResult> GetPermissionsByModule(string moduleCode)
        {
            var permissions = await _permissionService.GetPermissionsByModuleAsync(moduleCode);
            return Ok(permissions);
        }
        [HttpGet("permissions/{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null) return NotFound();
            return Ok(permission);
        }
        [HttpGet("permissions/check")]
        public async Task<IActionResult> IsPermissionCodeExists([FromQuery] string code, [FromQuery] int? excludeId)
        {
            var exists = await _permissionService.IsPermissionCodeExistsAsync(code, excludeId);
            return Ok(new { exists });
        }
        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            var createdBy = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _permissionService.CreatePermissionAsync(request.Permission_code, createdBy);
            if (!result) return BadRequest(new { message = "Tạo quyền thất bại" });
            return Ok(new { success = true });
        }
        [HttpPut("permissions/{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] CreatePermissionRequest request)
        {
            var updatedBy = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _permissionService.UpdatePermissionAsync(id, request.Permission_code, updatedBy);
            if (!result) return NotFound(new { message = "Cập nhật thất bại" });
            return Ok(new { success = true });
        }
        [HttpDelete("permissions/{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (!result) return NotFound(new { message = "Xóa thất bại" });
            return Ok(new { success = true });
        }

        #endregion

        #region Roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            return Ok(roles);
        }
        [HttpGet("roles/{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);
            if (rolePermissions == null)
            {
                return NotFound(new { message = "Role không t?n t?i" });
            }
            return Ok(rolePermissions);
        }
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            // TODO: L?y userId t? token
            int createdBy = 1;

            var result = await _permissionService.CreateRoleAsync(dto, createdBy);
            if (!result)
            {
                return BadRequest(new { message = "Không th? t?o role" });
            }
            return Ok(new { success = true, message = "T?o role thành công" });
        }
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID không kh?p" });
            }

            // TODO: L?y userId t? token
            int updatedBy = 1;

            var result = await _permissionService.UpdateRoleAsync(dto, updatedBy);
            if (!result)
            {
                return NotFound(new { message = "Role không t?n t?i" });
            }
            return Ok(new { success = true, message = "C?p nh?t role thành công" });
        }
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _permissionService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Role không t?n t?i" });
            }
            return Ok(new { success = true, message = "X�a role th�nh c�ng" });
        }
        [HttpPost("roles/{roleId}/permissions")]
        public async Task<IActionResult> AssignPermissionsToRole(int roleId, [FromBody] AssignPermissionsToRoleDto dto)
        {
            if (roleId != dto.Role_id)
            {
                return BadRequest(new { message = "Role ID không kh?p" });
            }

            var result = await _permissionService.AssignPermissionsToRoleAsync(dto);
            if (!result)
            {
                return BadRequest(new { message = "Không th? gán permissions cho role" });
            }
            return Ok(new { success = true, message = "G�n permissions th�nh c�ng" });
        }
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            var result = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(new { success = true, message = "X�a permission th�nh c�ng" });
        }

        #endregion

        #region User Permissions
        [HttpGet("user/me")]
        public async Task<IActionResult> GetMyPermissions()
        {
            // TODO: L?y userId t? token
            int userId = 1;

            var permissions = await _permissionService.GetUserPermissionDetailsAsync(userId);
            if (permissions == null)
            {
                return NotFound(new { message = "User không t?n t?i" });
            }
            return Ok(permissions);
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var permissions = await _permissionService.GetUserPermissionDetailsAsync(userId);
            if (permissions == null)
            {
                return NotFound(new { message = "User không t?n t?i" });
            }
            return Ok(permissions);
        }
        [HttpGet("user/{userId}/check/{permissionCode}")]
        public async Task<IActionResult> CheckUserPermission(int userId, string permissionCode)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionCode);
            return Ok(new { has_permission = hasPermission });
        }
        [HttpPost("user/{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var result = await _permissionService.AssignRoleToUserAsync(userId, roleId);
            if (!result)
            {
                return BadRequest(new { message = "Không th? gán role cho user" });
            }
            return Ok(new { success = true, message = "G�n role th�nh c�ng" });
        }
        [HttpDelete("user/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var result = await _permissionService.RemoveRoleFromUserAsync(userId, roleId);
            return Ok(new { success = true, message = "X�a role th�nh c�ng" });
        }
        [HttpGet("user/{userId}/full")]
        public async Task<IActionResult> GetUserFullPermissions(int userId)
        {
            var result = await _permissionService.GetUserFullPermissionsAsync(userId);
            if (result == null) return NotFound(new { message = "User không tồn tại" });
            return Ok(result);
        }
        [HttpPost("user/assign-permissions")]
        public async Task<IActionResult> AssignPermissionsToUser([FromBody] AssignPermissionsToUserDto dto)
        {
            var grantedBy = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _permissionService.AssignPermissionsToUserAsync(dto, grantedBy);
            if (!result) return BadRequest(new { message = "Gán quyền thất bại" });
            return Ok(new { success = true, message = "Gán quyền thành công" });
        }
        [HttpPost("user/{userId}/remove-all-direct")]
        public async Task<IActionResult> RemoveAllDirectPermissionsFromUser(int userId)
        {
            var result = await _permissionService.RemoveAllDirectPermissionsFromUserAsync(userId);
            if (!result) return BadRequest(new { message = "Xóa quyền thất bại" });
            return Ok(new { success = true });
        }

        #endregion
    }
}
