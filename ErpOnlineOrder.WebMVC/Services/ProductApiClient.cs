using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _http;

        public ProductApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("product", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProductDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProductDTO>();
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrEmpty(author)) query.Add($"author={Uri.EscapeDataString(author)}");
            if (!string.IsNullOrEmpty(publisher)) query.Add($"publisher={Uri.EscapeDataString(publisher)}");
            var path = query.Count > 0 ? "product?" + string.Join("&", query) : "product";
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProductDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProductDTO>();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("product/" + id, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProductDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("product/" + id + "/entity", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Product>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<ProductDTO?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("product", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProductDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync("product/" + id, dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync("product/" + id, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
