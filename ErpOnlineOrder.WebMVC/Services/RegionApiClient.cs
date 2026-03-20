using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class RegionApiClient : BaseApiClient, IRegionApiClient
    {
        public RegionApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<RegionDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<RegionDTO>>("region", cancellationToken) ?? new List<RegionDTO>();
        }

        public async Task<RegionDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<RegionDTO>($"region/{id}", cancellationToken);
        }

        public async Task<(RegionDTO? Data, string? Error)> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateRegionDto, RegionDTO>("region", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRegionDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"region/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"region/{id}", cancellationToken);
        }
    }
}
