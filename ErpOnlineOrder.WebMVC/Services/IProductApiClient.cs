using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IProductApiClient
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        /// <summary>Lấy sản phẩm cho order/customer-product (dùng quyền OrderCreate, OrderUpdate, CustomerProductView... thay vì ProductView)</summary>
        Task<IEnumerable<ProductDTO>> GetForOrderAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher, CancellationToken cancellationToken = default);
        Task<ProductDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductDTO?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
