using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AdminApiClient : IAdminApiClient
    {
        private readonly HttpClient _http;

        public AdminApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("admin/staff", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<StaffAccountDto>();
            var list = await response.Content.ReadFromJsonAsync<List<StaffAccountDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<StaffAccountDto>();
        }

        public async Task<StaffAccountDto?> GetStaffByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"admin/staff/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StaffAccountDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<StaffAccountDto?> CreateStaffAsync(CreateStaffAccountDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("admin/staff", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StaffAccountDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateStaffAsync(int userId, UpdateStaffAccountDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"admin/staff/{userId}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteStaffAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"admin/staff/{userId}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> ToggleStaffStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var response = await _http.PatchAsync($"admin/staff/{userId}/status?isActive={isActive}", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"admin/staff/{dto.User_id}/reset-password", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
