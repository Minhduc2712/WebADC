using ErpOnlineOrder.Application.DTOs.PermissionDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IPermissionApiClient
    {
        Task<IEnumerable<string>> GetUserPermissionCodesAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task<RolePermissionDto?> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateRoleAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsToRoleDto dto, CancellationToken cancellationToken = default);
        Task<UserFullPermissionDto?> GetUserFullPermissionsAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
        Task<PermissionDto?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsPermissionCodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> CreatePermissionAsync(string permissionCode, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdatePermissionAsync(int id, string permissionCode, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeletePermissionAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> AssignPermissionsToUserAsync(AssignPermissionsToUserDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> RemoveAllDirectPermissionsFromUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
