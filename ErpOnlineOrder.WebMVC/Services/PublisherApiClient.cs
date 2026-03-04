using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class PublisherApiClient : IPublisherApiClient
    {
        private readonly HttpClient _http;

        public PublisherApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("publisher", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<PublisherDto>();
            var list = await response.Content.ReadFromJsonAsync<List<PublisherDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<PublisherDto>();
        }

        public async Task<PublisherDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"publisher/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PublisherDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<PublisherDto?> CreateAsync(CreatePublisherDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("publisher", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PublisherDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePublisherDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"publisher/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"publisher/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
