using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.PermissionDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class PermissionApiClient : IPermissionApiClient
    {
        private readonly HttpClient _http;

        public PermissionApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<string>> GetUserPermissionCodesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"permission/user/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<string>();
            var dto = await response.Content.ReadFromJsonAsync<UserPermissionDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return (IEnumerable<string>)(dto?.Permissions ?? new List<string>());
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("permission/roles", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<RoleDto>();
            var list = await response.Content.ReadFromJsonAsync<List<RoleDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<RoleDto>();
        }

        public async Task<UserFullPermissionDto?> GetUserFullPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"permission/user/{userId}/full", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserFullPermissionDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("permission/permissions", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<PermissionDto>();
            var list = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<PermissionDto>();
        }

        public async Task<(bool Success, string? Error)> AssignPermissionsToUserAsync(AssignPermissionsToUserDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("permission/user/assign-permissions", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<RolePermissionDto?> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"permission/roles/{roleId}/permissions", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RolePermissionDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("permission/roles", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> UpdateRoleAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"permission/roles/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteRoleAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"permission/roles/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsToRoleDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"permission/roles/{roleId}/permissions", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"permission/permissions/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PermissionDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<bool> IsPermissionCodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var url = "permission/permissions/check?code=" + Uri.EscapeDataString(code);
            if (excludeId.HasValue) url += "&excludeId=" + excludeId.Value;
            var response = await _http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return false;
            var obj = await response.Content.ReadFromJsonAsync<CheckExistsResponse>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return obj?.exists ?? false;
        }

        private class CheckExistsResponse { public bool exists { get; set; } }

        public async Task<(bool Success, string? Error)> CreatePermissionAsync(string permissionCode, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("permission/permissions", new { Permission_code = permissionCode }, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> UpdatePermissionAsync(int id, string permissionCode, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"permission/permissions/{id}", new { Permission_code = permissionCode }, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeletePermissionAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"permission/permissions/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> RemoveAllDirectPermissionsFromUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"permission/user/{userId}/remove-all-direct", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
