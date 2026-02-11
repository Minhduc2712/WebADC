using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<Product?> GetEntityByIdAsync(int id);
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher);
        Task<ProductDTO> CreateProductAsync(CreateProductDto dto, int createdBy);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto dto, int updatedBy);
        Task<bool> DeleteProductAsync(int id);
    }
}
