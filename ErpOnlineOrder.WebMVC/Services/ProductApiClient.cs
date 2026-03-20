using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class ProductApiClient : BaseApiClient, IProductApiClient
    {
        public ProductApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<ProductDTO>>("product", cancellationToken) ?? new List<ProductDTO>();
        }

        public async Task<IEnumerable<ProductDTO>> GetForOrderAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<ProductDTO>>("product/for-order", cancellationToken) ?? new List<ProductDTO>();
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? search, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(search)
                ? "product?search=" + Uri.EscapeDataString(search)
                : "product";
            return await GetAsync<IEnumerable<ProductDTO>>(path, cancellationToken) ?? new List<ProductDTO>();
        }

        public async Task<PagedResult<ProductDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? categoryId = null, int? publisherId = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            if (categoryId.HasValue) query.Add("categoryId=" + categoryId.Value);
            if (publisherId.HasValue) query.Add("publisherId=" + publisherId.Value);
            var path = "product/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<ProductDTO>>(path, cancellationToken) ?? new PagedResult<ProductDTO> { Items = new List<ProductDTO>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<ProductDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<ProductDTO>($"product/{id}", cancellationToken);
        }

        public async Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<Product>($"product/{id}/entity", cancellationToken);
        }

        public async Task<(ProductDTO? Data, string? Error)> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateProductDto, ProductDTO>("product", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"product/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"product/{id}", cancellationToken);
        }

        public async Task<byte[]> ExportToExcelAsync(string? search = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(search)
                ? "product/export?search=" + Uri.EscapeDataString(search)
                : "product/export";
            return await GetByteArrayAsync(path, cancellationToken);
        }

        public async Task<PagedResult<ProductDTO>> GetProductsForShopPagedAsync(int? customerId, ProductForShopFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={request.Page}", $"pageSize={request.PageSize}" };
            if (customerId.HasValue) query.Add($"customerId={customerId.Value}");
            if (!string.IsNullOrEmpty(request.SearchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
            if (!string.IsNullOrEmpty(request.Category)) query.Add($"category={Uri.EscapeDataString(request.Category)}");
            if (!string.IsNullOrEmpty(request.Sort)) query.Add($"sort={Uri.EscapeDataString(request.Sort)}");
            var path = "product/shop?" + string.Join("&", query);
            return await GetAsync<PagedResult<ProductDTO>>(path, cancellationToken) ?? new PagedResult<ProductDTO> { Items = new List<ProductDTO>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
        }

        public async Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId, CancellationToken cancellationToken = default)
        {
            var path = customerId.HasValue ? $"product/shop/categories?customerId={customerId.Value}" : "product/shop/categories";
            return await GetAsync<IEnumerable<string>>(path, cancellationToken) ?? Array.Empty<string>();
        }

        public async Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId, CancellationToken cancellationToken = default)
        {
            var obj = await GetAsync<CheckAssignedResponse>($"product/{productId}/check-assigned?customerId={customerId}", cancellationToken);
            return obj?.Assigned ?? false;
        }

        public async Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, int? customerId, int limit, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"limit={limit}" };
            if (customerId.HasValue) query.Add($"customerId={customerId.Value}");
            var path = $"product/shop/{productId}/related?" + string.Join("&", query);
            return await GetAsync<IEnumerable<ProductDTO>>(path, cancellationToken) ?? Array.Empty<ProductDTO>();
        }
        
        private class CheckAssignedResponse { public bool Assigned { get; set; } }
    }
}
