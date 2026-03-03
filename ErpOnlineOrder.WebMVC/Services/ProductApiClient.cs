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

        public async Task<IEnumerable<ProductDTO>> GetForOrderAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("product/for-order", cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProductDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProductDTO>();
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? search, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(search)
                ? "product?search=" + Uri.EscapeDataString(search)
                : "product";
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProductDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<ProductDTO>();
        }

        public async Task<PagedResult<ProductDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? categoryId = null, int? publisherId = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            if (categoryId.HasValue) query.Add("categoryId=" + categoryId.Value);
            if (publisherId.HasValue) query.Add("publisherId=" + publisherId.Value);
            var path = "product/paged?" + string.Join("&", query);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new PagedResult<ProductDTO>
            {
                Items = new List<ProductDTO>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
            var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return result ?? new PagedResult<ProductDTO>();
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
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
            {
                var msg = await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(msg ?? $"Lỗi khi tải sản phẩm: {response.StatusCode}");
            }
            return await response.Content.ReadFromJsonAsync<Product>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<ProductDTO?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("product", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(msg ?? "Thêm sản phẩm thất bại.");
            }
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

        public async Task<byte[]> ExportToExcelAsync(string? search = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(search)
                ? "product/export?search=" + Uri.EscapeDataString(search)
                : "product/export";
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<byte>();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
