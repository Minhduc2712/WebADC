using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CategoryApiClient : ICategoryApiClient
    {
        private readonly HttpClient _http;

        public CategoryApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("category", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<CategoryDto>();
            var list = await response.Content.ReadFromJsonAsync<List<CategoryDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"category/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CategoryDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("category", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CategoryDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"category/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"category/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
