using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IProductApiClient
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDTO>> GetForOrderAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDTO>> SearchAsync(string? search, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? categoryId = null, int? publisherId = null, CancellationToken cancellationToken = default);
        Task<ProductDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(ProductDTO? Data, string? Error)> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<byte[]> ExportToExcelAsync(string? search = null, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductDTO>> GetProductsForShopPagedAsync(int? customerId, ProductForShopFilterRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId, CancellationToken cancellationToken = default);
        Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, int? customerId, int limit, CancellationToken cancellationToken = default);
    }
}
