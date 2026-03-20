using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class WarehouseApiClient : BaseApiClient, IWarehouseApiClient
    {
        public WarehouseApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseDto>>("warehouse", cancellationToken) ?? Array.Empty<WarehouseDto>();
        }

        public async Task<IEnumerable<WarehouseSelectDto>> GetForSelectAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseSelectDto>>("warehouse/for-select", cancellationToken) ?? Array.Empty<WarehouseSelectDto>();
        }

        public async Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<WarehouseDto>($"warehouse/{id}", cancellationToken);
        }

        public async Task<IEnumerable<WarehouseDto>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseDto>>($"warehouse/province/{provinceId}", cancellationToken) ?? Array.Empty<WarehouseDto>();
        }

        public async Task<(WarehouseDto? Data, string? Error)> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateWarehouseDto, WarehouseDto>("warehouse", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"warehouse/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"warehouse/{id}", cancellationToken);
        }
    }
}
