using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerApiClient : BaseApiClient, ICustomerApiClient
    {
        public CustomerApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerDTO>>("customer", cancellationToken) ?? Array.Empty<CustomerDTO>();
        }

        public async Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerSelectDto>>("customer/for-select", cancellationToken) ?? Array.Empty<CustomerSelectDto>();
        }

        public async Task<PagedResult<CustomerDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? regionId = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            if (regionId.HasValue) query.Add("regionId=" + regionId.Value);
            var path = "customer/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<CustomerDTO>>(path, cancellationToken) ?? new PagedResult<CustomerDTO> { Items = new List<CustomerDTO>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<CustomerDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerDTO>($"customer/{id}", cancellationToken);
        }

        public async Task<(CustomerDTO? Data, string? Error)> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateCustomerDto, CustomerDTO>("customer", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerByAdminDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"customer/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"customer/{id}", cancellationToken);
        }

        public async Task<CustomerDTO?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerDTO>($"customer/user/{userId}", cancellationToken);
        }

        public async Task<CustomerOrganizationViewDto?> GetOrganizationByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerOrganizationViewDto>($"customer/{customerId}/organization", cancellationToken);
        }

        public async Task<bool> UpdateOrganizationAsync(UpdateOrganizationByCustomerDto model, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PutAsync("customer/organization", model, cancellationToken);
            return success;
        }
    }
}
