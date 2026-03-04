using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AuthorApiClient : IAuthorApiClient
    {
        private readonly HttpClient _http;

        public AuthorApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("author", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<AuthorDto>();
            var list = await response.Content.ReadFromJsonAsync<List<AuthorDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<AuthorDto>();
        }

        public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"author/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthorDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<AuthorDto?> CreateAsync(CreateAuthorDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("author", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthorDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateAuthorDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"author/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"author/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
