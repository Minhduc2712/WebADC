using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class ProvinceApiClient : BaseApiClient, IProvinceApiClient
    {
        public ProvinceApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<ProvinceDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<ProvinceDTO>>("province", cancellationToken) ?? new List<ProvinceDTO>();
        }

        public async Task<IEnumerable<ProvinceDTO>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<ProvinceDTO>>($"province/region/{regionId}", cancellationToken) ?? new List<ProvinceDTO>();
        }

        public async Task<ProvinceDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<ProvinceDTO>($"province/{id}", cancellationToken);
        }

        public async Task<(ProvinceDTO? Data, string? Error)> CreateAsync(CreateProvinceDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateProvinceDto, ProvinceDTO>("province", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProvinceDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"province/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"province/{id}", cancellationToken);
        }
    }
}
