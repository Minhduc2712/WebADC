using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class DistributorApiClient : IDistributorApiClient
    {
        private readonly HttpClient _http;

        public DistributorApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<DistributorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("distributor", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<DistributorDto>();
            var list = await response.Content.ReadFromJsonAsync<List<DistributorDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<DistributorDto>();
        }

        public async Task<DistributorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"distributor/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DistributorDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<DistributorDto?> CreateAsync(CreateDistributorDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("distributor", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DistributorDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDistributorDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"distributor/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"distributor/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
