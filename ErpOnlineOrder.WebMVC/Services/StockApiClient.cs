using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class StockApiClient : BaseApiClient, IStockApiClient
    {
        public StockApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<StockDto>> GetAllAsync(int? warehouseId = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (warehouseId.HasValue && warehouseId.Value > 0) query.Add($"warehouseId={warehouseId.Value}");
            if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

            var path = "stock" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            return await GetAsync<IEnumerable<StockDto>>(path, cancellationToken) ?? Array.Empty<StockDto>();
        }

        public async Task<StockDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<StockDto>($"stock/{id}", cancellationToken);
        }

        public async Task<(StockDto? Data, string? Error)> CreateAsync(CreateStockDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateStockDto, StockDto>("stock", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateStockDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"stock/{id}", dto, cancellationToken);
        }
    }
}
