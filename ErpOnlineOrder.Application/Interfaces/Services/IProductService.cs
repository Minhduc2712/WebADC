using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
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
        Task<Product> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}
