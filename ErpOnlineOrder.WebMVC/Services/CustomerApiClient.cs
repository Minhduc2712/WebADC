using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;

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

        public async Task<CustomerDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customer/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<CustomerDTO?> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("customer", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
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
