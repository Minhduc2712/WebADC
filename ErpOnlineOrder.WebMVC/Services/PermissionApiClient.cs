using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class PermissionApiClient : BaseApiClient, IPermissionApiClient
    {
        public PermissionApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<string>> GetUserPermissionCodesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var dto = await GetAsync<UserPermissionDto>($"permission/user/{userId}", cancellationToken);
            return (IEnumerable<string>)(dto?.Permissions ?? new List<string>());
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<RoleDto>>("permission/roles", cancellationToken) ?? Array.Empty<RoleDto>();
        }

        public async Task<UserFullPermissionDto?> GetUserFullPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<UserFullPermissionDto>($"permission/user/{userId}/full", cancellationToken);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PermissionDto>>("permission/permissions", cancellationToken) ?? Array.Empty<PermissionDto>();
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsTreeAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PermissionDto>>("permission/permissions/tree", cancellationToken) ?? Array.Empty<PermissionDto>();
        }

        public async Task<IEnumerable<PermissionDto>> GetSpecialPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PermissionDto>>("permission/permissions/special", cancellationToken) ?? Array.Empty<PermissionDto>();
        }

        public async Task<(bool Success, string? Error)> AssignPermissionsToUserAsync(AssignPermissionsToUserDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("permission/user/assign-permissions", dto, cancellationToken);
        }

        public async Task<RolePermissionDto?> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<RolePermissionDto>($"permission/roles/{roleId}/permissions", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("permission/roles", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateRoleAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"permission/roles/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteRoleAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"permission/roles/{id}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsToRoleDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"permission/roles/{roleId}/permissions", dto, cancellationToken);
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<PermissionDto>($"permission/permissions/{id}", cancellationToken);
        }

        public async Task<bool> IsPermissionCodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var url = "permission/permissions/check?code=" + Uri.EscapeDataString(code);
            if (excludeId.HasValue) url += "&excludeId=" + excludeId.Value;
            var obj = await GetAsync<CheckExistsResponse>(url, cancellationToken);
            return obj?.exists ?? false;
        }

        private class CheckExistsResponse { public bool exists { get; set; } }

        public async Task<(bool Success, string? Error)> CreatePermissionAsync(string permissionCode, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("permission/permissions", new { Permission_code = permissionCode }, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdatePermissionAsync(int id, string permissionCode, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"permission/permissions/{id}", new { Permission_code = permissionCode }, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeletePermissionAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"permission/permissions/{id}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> RemoveAllDirectPermissionsFromUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"permission/user/{userId}/remove-all-direct", cancellationToken);
        }
    }
}
