using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IPermissionService
    {
        // Kiểm tra quyền
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
        Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionCodes);
        Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionCodes);
        Task<bool> IsAdminAsync(int userId);
        
        // Lấy danh sách quyền
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<UserPermissionDto?> GetUserPermissionDetailsAsync(int userId);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string moduleCode);
        
        // Quản lý Permission (CRUD)
        Task<PermissionDto?> GetPermissionByIdAsync(int id);
        Task<bool> CreatePermissionAsync(string permissionCode, int createdBy);
        Task<bool> UpdatePermissionAsync(int id, string permissionCode, int updatedBy);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> IsPermissionCodeExistsAsync(string permissionCode, int? excludeId = null);
        
        // Quản lý Role
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RolePermissionDto?> GetRolePermissionsAsync(int roleId);
        Task<bool> CreateRoleAsync(CreateRoleDto dto, int createdBy);
        Task<bool> UpdateRoleAsync(UpdateRoleDto dto, int updatedBy);
        Task<bool> DeleteRoleAsync(int roleId);
        
        // Gán quyền cho Role
        Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto dto);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
        
        // Gán Role cho User
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        
        // Gán quyền TRỰC TIẾP cho User (không thông qua Role)
        Task<IEnumerable<UserDirectPermissionDto>> GetUserDirectPermissionsAsync(int userId);
        Task<UserFullPermissionDto?> GetUserFullPermissionsAsync(int userId);
        Task<bool> AssignPermissionsToUserAsync(AssignPermissionsToUserDto dto, int grantedBy);
        Task<bool> RemovePermissionFromUserAsync(int userId, int permissionId);
        Task<bool> RemoveAllDirectPermissionsFromUserAsync(int userId);
    }
}
