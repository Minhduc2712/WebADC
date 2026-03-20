using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AdminApiClient : BaseApiClient, IAdminApiClient
    {
        private readonly ILogger<AdminApiClient> _logger;

        public AdminApiClient(IHttpClientFactory factory, ILogger<AdminApiClient> logger) : base(factory.CreateClient("ErpApi"))
        {
            _logger = logger;
        }

        public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<StaffAccountDto>>("admin/staff", cancellationToken) ?? Array.Empty<StaffAccountDto>();
        }

        public async Task<PagedResult<StaffAccountDto>> GetStaffPagedAsync(int page = 1, int pageSize = 20, int? roleId = null, bool? isActive = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (roleId.HasValue) query.Add("roleId=" + roleId.Value);
            if (isActive.HasValue) query.Add("isActive=" + isActive.Value.ToString().ToLowerInvariant());
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "admin/staff/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<StaffAccountDto>>(path, cancellationToken) ?? new PagedResult<StaffAccountDto> { Items = new List<StaffAccountDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<StaffAccountDto?> GetStaffByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<StaffAccountDto>($"admin/staff/{userId}", cancellationToken);
        }

        public async Task<(StaffAccountDto? Staff, string? Error)> CreateStaffAsync(CreateStaffAccountDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateStaffAccountDto, StaffAccountDto>("admin/staff", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateStaffAsync(int userId, UpdateStaffAccountDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"admin/staff/{userId}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteStaffAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"admin/staff/{userId}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> ToggleStaffStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
        {
            return await PatchAsync($"admin/staff/{userId}/status?isActive={isActive}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"admin/staff/{dto.User_id}/reset-password", dto, cancellationToken);
        }
    }
}
