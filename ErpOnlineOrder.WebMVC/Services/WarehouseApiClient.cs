using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class WarehouseApiClient : IWarehouseApiClient
    {
        private readonly HttpClient _http;

        public WarehouseApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("warehouse", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<WarehouseDto>();
            var list = await response.Content.ReadFromJsonAsync<List<WarehouseDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<WarehouseDto>();
        }

        public async Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouse/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WarehouseDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<IEnumerable<WarehouseDto>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouse/province/{provinceId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<WarehouseDto>();
            var list = await response.Content.ReadFromJsonAsync<List<WarehouseDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<WarehouseDto>();
        }

        public async Task<WarehouseDto?> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("warehouse", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WarehouseDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"warehouse/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"warehouse/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
