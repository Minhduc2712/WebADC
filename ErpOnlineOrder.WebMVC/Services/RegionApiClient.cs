using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.RegionDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class RegionApiClient : IRegionApiClient
    {
        private readonly HttpClient _http;

        public RegionApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<RegionDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("region", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<RegionDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<RegionDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<RegionDTO>();
        }

        public async Task<RegionDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("region/" + id, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RegionDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<RegionDTO?> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("region", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RegionDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRegionDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync("region/" + id, dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync("region/" + id, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
