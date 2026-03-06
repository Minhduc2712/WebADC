using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        public IQueryable<Product?> GetByProductId(int id);
        Task<Product?> GetByCodeAsync(string code);
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
        Task<Product?> GetByNameAsync(string name);
        Task<Dictionary<int, ProductValidationInfoDto>> GetProductValidationMapAsync(int customerId, List<int> productIds);
        Task<PagedResult<Product>> GetPagedProductAsync(ProductFilterRequest request);
        Task<PagedResult<ProductDTO>> GetPagedProductsForShopDisplayAsync(int? customerId, ProductForShopFilterRequest request);
        Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId, ProductForShopFilterRequest request);
        Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, IEnumerable<string> categoryNames, int? customerId, int limit = 4);
        Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId);
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher);
        Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString);
        Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId);
        Task<IEnumerable<ProductDTO>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ProductSelectDto>> GetForSelectAsync();
        Task<Product> AddAsync(Product product);
        Task DeleteAsync(int id);
        Task UpdateAsync(Product product);
    }
}
