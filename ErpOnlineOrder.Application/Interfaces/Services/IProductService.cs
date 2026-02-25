using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDTO?> GetByIdAsync(int id, int? userId = null);
        Task<Product?> GetEntityByIdAsync(int id);
        Task<IEnumerable<ProductDTO>> GetAllAsync(int? userId = null);
        Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher, int? userId = null);
        Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString, int? userId = null);
        Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId = null);
        Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId = null);
        Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId);
        Task<ProductDTO> CreateProductAsync(CreateProductDto dto, int createdBy);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto dto, int updatedBy);
        Task<bool> DeleteProductAsync(int id);
        Task<byte[]> ExportProductsToExcelAsync(string? search = null);
    }
}
