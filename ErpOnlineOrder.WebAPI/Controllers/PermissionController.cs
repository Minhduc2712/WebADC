using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    public class CreatePermissionRequest
    {
        public string Permission_code { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ApiController
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
            return Ok(ApiResponse<object>.Ok(permissions));
        }
        [HttpGet("permissions/module/{moduleCode}")]
        public async Task<IActionResult> GetPermissionsByModule(string moduleCode)
        {
            var permissions = await _permissionService.GetPermissionsByModuleAsync(moduleCode);
            return Ok(ApiResponse<object>.Ok(permissions));
        }
        [HttpGet("permissions/tree")]
        public async Task<IActionResult> GetPermissionsTree()
        {
            var tree = await _permissionService.GetPermissionsTreeAsync();
            return Ok(ApiResponse<object>.Ok(tree));
        }
        [HttpGet("permissions/special")]
        public async Task<IActionResult> GetSpecialPermissions()
        {
            var specials = await _permissionService.GetSpecialPermissionsAsync();
            return Ok(ApiResponse<object>.Ok(specials));
        }
        [HttpGet("permissions/{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy quyền."));
            return Ok(ApiResponse<object>.Ok(permission));
        }
        [HttpGet("permissions/check")]
        public async Task<IActionResult> IsPermissionCodeExists([FromQuery] string code, [FromQuery] int? excludeId)
        {
            var exists = await _permissionService.IsPermissionCodeExistsAsync(code, excludeId);
            return Ok(ApiResponse<object>.Ok(new { exists }));
        }
        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            var result = await _permissionService.CreatePermissionAsync(request.Permission_code, GetCurrentUserId());
            if (!result) return BadRequest(ApiResponse<object>.Fail("Không thể tạo quyền. Mã quyền có thể đã tồn tại hoặc không đúng định dạng."));
            return Ok(ApiResponse<object>.Ok(null));
        }
        [HttpPut("permissions/{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] CreatePermissionRequest request)
        {
            var result = await _permissionService.UpdatePermissionAsync(id, request.Permission_code, GetCurrentUserId());
            if (!result) return NotFound(ApiResponse<object>.Fail("Không thể cập nhật quyền. Quyền không tồn tại hoặc dữ liệu cập nhật không hợp lệ."));
            return Ok(ApiResponse<object>.Ok(null));
        }
        [HttpDelete("permissions/{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (!result) return NotFound(ApiResponse<object>.Fail("Không thể xóa quyền. Quyền không tồn tại hoặc đang được sử dụng bởi vai trò/người dùng khác."));
            return Ok(ApiResponse<object>.Ok(null));
        }

        #endregion

        #region Roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            return Ok(ApiResponse<object>.Ok(roles));
        }
        [HttpGet("roles/{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);
            if (rolePermissions == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy vai trò."));
            }
            return Ok(ApiResponse<object>.Ok(rolePermissions));
        }
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var result = await _permissionService.CreateRoleAsync(dto, GetCurrentUserId());
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Không thể tạo vai trò. Tên vai trò có thể đã tồn tại hoặc không hợp lệ."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Tạo role thành công"));
        }
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<object>.Fail("ID trên URL không khớp với ID vai trò trong dữ liệu cập nhật."));
            }

            var result = await _permissionService.UpdateRoleAsync(dto, GetCurrentUserId());
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy vai trò cần cập nhật."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Cập nhật role thành công"));
        }
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _permissionService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy vai trò cần xóa."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa role thành công"));
        }
        [HttpPost("roles/{roleId}/permissions")]
        public async Task<IActionResult> AssignPermissionsToRole(int roleId, [FromBody] AssignPermissionsToRoleDto dto)
        {
            if (roleId != dto.Role_id)
            {
                return BadRequest(ApiResponse<object>.Fail("Role ID trên URL không khớp với dữ liệu phân quyền."));
            }

            var result = await _permissionService.AssignPermissionsToRoleAsync(dto);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Không thể gán quyền cho vai trò. Vui lòng kiểm tra danh sách quyền và trạng thái vai trò."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Gán quyền thành công"));
        }
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            var result = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(ApiResponse<object>.Ok(null, "Xóa quyền thành công"));
        }

        #endregion

        #region User Permissions
        [HttpGet("user/me")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var permissions = await _permissionService.GetUserPermissionDetailsAsync(GetCurrentUserId());
            if (permissions == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng hiện tại để lấy quyền."));
            }
            return Ok(ApiResponse<object>.Ok(permissions));
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var permissions = await _permissionService.GetUserPermissionDetailsAsync(userId);
            if (permissions == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng để lấy thông tin quyền."));
            }
            return Ok(ApiResponse<object>.Ok(permissions));
        }
        [HttpGet("user/{userId}/check/{permissionCode}")]
        public async Task<IActionResult> CheckUserPermission(int userId, string permissionCode)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionCode);
            return Ok(ApiResponse<object>.Ok(new { has_permission = hasPermission }));
        }
        [HttpPost("user/{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var result = await _permissionService.AssignRoleToUserAsync(userId, roleId);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Không thể gán vai trò cho người dùng. Vui lòng kiểm tra người dùng, vai trò và dữ liệu phân quyền."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Gán role thành công"));
        }
        [HttpDelete("user/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var result = await _permissionService.RemoveRoleFromUserAsync(userId, roleId);
            return Ok(ApiResponse<object>.Ok(null, "Xóa role thành công"));
        }
        [HttpGet("user/{userId}/full")]
        public async Task<IActionResult> GetUserFullPermissions(int userId)
        {
            var result = await _permissionService.GetUserFullPermissionsAsync(userId);
            if (result == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng để lấy quyền đầy đủ."));
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpPost("user/assign-permissions")]
        public async Task<IActionResult> AssignPermissionsToUser([FromBody] AssignPermissionsToUserDto dto)
        {
            var result = await _permissionService.AssignPermissionsToUserAsync(dto, GetCurrentUserId());
            if (!result) return BadRequest(ApiResponse<object>.Fail("Không thể gán quyền trực tiếp cho người dùng. Vui lòng kiểm tra danh sách quyền gửi lên."));
            return Ok(ApiResponse<object>.Ok(null, "Gán quyền thành công"));
        }
        [HttpPost("user/{userId}/remove-all-direct")]
        public async Task<IActionResult> RemoveAllDirectPermissionsFromUser(int userId)
        {
            var result = await _permissionService.RemoveAllDirectPermissionsFromUserAsync(userId);
            if (!result) return BadRequest(ApiResponse<object>.Fail("Không thể xóa toàn bộ quyền trực tiếp của người dùng. Người dùng có thể không tồn tại."));
            return Ok(ApiResponse<object>.Ok(null));
        }

        #endregion
    }
}
