using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerApiClient : ICustomerApiClient
    {
        private readonly HttpClient _http;

        public CustomerApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("customer", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<CustomerDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<CustomerDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CustomerDTO>();
        }

        public async Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("customer/for-select", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<CustomerSelectDto>();
            var list = await response.Content.ReadFromJsonAsync<List<CustomerSelectDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CustomerSelectDto>();
        }

        public async Task<PagedResult<CustomerDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? regionId = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            if (regionId.HasValue) query.Add("regionId=" + regionId.Value);
            var path = "customer/paged?" + string.Join("&", query);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new PagedResult<CustomerDTO> { Items = new List<CustomerDTO>(), Page = page, PageSize = pageSize, TotalCount = 0 };
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CustomerDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return result ?? new PagedResult<CustomerDTO>();
        }

        public async Task<CustomerDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customer/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(CustomerDTO? Data, string? Error)> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("customer", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));

            var data = await response.Content.ReadFromJsonAsync<CustomerDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return (data, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerByAdminDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"customer/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"customer/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
