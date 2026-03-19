using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class StockApiClient : IStockApiClient
    {
        private readonly HttpClient _http;

        public StockApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<StockDto>> GetAllAsync(int? warehouseId = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (warehouseId.HasValue && warehouseId.Value > 0) query.Add($"warehouseId={warehouseId.Value}");
            if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

            var path = "stock" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<StockDto>();

            var list = await response.Content.ReadFromJsonAsync<List<StockDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<StockDto>();
        }

        public async Task<StockDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"stock/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StockDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(StockDto? Data, string? Error)> CreateAsync(CreateStockDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("stock", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));

            var data = await response.Content.ReadFromJsonAsync<StockDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return (data, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateStockDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"stock/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
