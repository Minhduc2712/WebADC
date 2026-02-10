using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CoverTypeApiClient : ICoverTypeApiClient
    {
        private readonly HttpClient _http;

        public CoverTypeApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<CoverTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("covertype", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<CoverTypeDto>();
            var list = await response.Content.ReadFromJsonAsync<List<CoverTypeDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CoverTypeDto>();
        }

        public async Task<CoverTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"covertype/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CoverTypeDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<CoverTypeDto?> CreateAsync(CreateCoverTypeDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("covertype", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CoverTypeDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCoverTypeDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"covertype/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"covertype/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
