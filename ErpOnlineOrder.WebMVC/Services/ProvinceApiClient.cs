using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class ProvinceApiClient : IProvinceApiClient
    {
        private readonly HttpClient _http;

        public ProvinceApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<ProvinceDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("province", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProvinceDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProvinceDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProvinceDTO>();
        }

        public async Task<IEnumerable<ProvinceDTO>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"province/region/{regionId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProvinceDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProvinceDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProvinceDTO>();
        }

        public async Task<ProvinceDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"province/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProvinceDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<ProvinceDTO?> CreateAsync(CreateProvinceDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("province", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProvinceDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProvinceDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"province/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"province/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
