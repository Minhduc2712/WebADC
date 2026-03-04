using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerProductApiClient : ICustomerProductApiClient
    {
        private readonly HttpClient _http;

        public CustomerProductApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<CustomerProductDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customerproduct/customer/{customerId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<CustomerProductDto>();
            var list = await response.Content.ReadFromJsonAsync<List<CustomerProductDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CustomerProductDto>();
        }

        public async Task<IEnumerable<CustomerProductDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customerproduct/product/{productId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<CustomerProductDto>();
            var list = await response.Content.ReadFromJsonAsync<List<CustomerProductDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CustomerProductDto>();
        }

        public async Task<CustomerProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customerproduct/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerProductDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<CustomerProductDto?> CreateAsync(CreateCustomerProductDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("customerproduct", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerProductDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AssignProductsAsync(AssignProductsToCustomerDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("customerproduct/assign", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerProductDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"customerproduct/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"customerproduct/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
