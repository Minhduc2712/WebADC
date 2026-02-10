using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ErpOnlineOrder.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            return MapToDto(product);
        }

        public async Task<Product?> GetEntityByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher)
        {
            var products = await _productRepository.SearchAsync(name, author, publisher);
            return products.Select(p => MapToDto(p));
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // Kiểm tra trùng lặp Product_code (mã sản phẩm phải duy nhất)
            var existingByCode = await _productRepository.GetByCodeAsync(product.Product_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Sản phẩm với mã '{product.Product_code}' đã tồn tại.");
            }

            product.Created_at = DateTime.UtcNow;
            product.Updated_at = DateTime.UtcNow;
            return await _productRepository.AddAsync(product);
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var existing = await _productRepository.GetByIdAsync(product.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp mã khi update (trừ bản ghi hiện tại)
            var existingByCode = await _productRepository.GetByCodeAsync(product.Product_code);
            if (existingByCode != null && existingByCode.Id != product.Id)
            {
                throw new InvalidOperationException($"Sản phẩm với mã '{product.Product_code}' đã tồn tại.");
            }

            existing.Product_code = product.Product_code;
            existing.Product_name = product.Product_name;
            existing.Product_price = product.Product_price;
            existing.Product_link = product.Product_link;
            existing.Product_description = product.Product_description;
            existing.Tax_rate = product.Tax_rate;
            existing.Cover_type_id = product.Cover_type_id;
            existing.Publisher_id = product.Publisher_id;
            existing.Distributor_id = product.Distributor_id;
            existing.Updated_by = product.Updated_by;
            existing.Updated_at = DateTime.UtcNow;

            // Đồng bộ Product_Categories và Product_Authors
            if (product.Product_Categories != null)
            {
                existing.Product_Categories ??= new List<Product_category>();
                existing.Product_Categories.Clear();
                var now = DateTime.UtcNow;
                foreach (var pc in product.Product_Categories)
                {
                    existing.Product_Categories.Add(new Product_category
                    {
                        Product_id = existing.Id,
                        Category_id = pc.Category_id,
                        Created_by = pc.Created_by,
                        Updated_by = product.Updated_by,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }
            if (product.Product_Authors != null)
            {
                existing.Product_Authors ??= new List<Product_author>();
                existing.Product_Authors.Clear();
                var now = DateTime.UtcNow;
                foreach (var pa in product.Product_Authors)
                {
                    existing.Product_Authors.Add(new Product_author
                    {
                        Product_id = existing.Id,
                        Author_id = pa.Author_id,
                        Created_by = pa.Created_by,
                        Updated_by = product.Updated_by,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }

            await _productRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
            return true;
        }

        private static ProductDTO MapToDto(Product p)
        {
            return new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description,
                Product_price = p.Product_price,
                Product_link = p.Product_link,
                Publisher_name = p.Publisher?.Publisher_name ?? "",
                Authors = p.Product_Authors?
                    .Where(pa => pa?.Author != null)
                    .Select(pa => pa.Author!.Author_name ?? "")
                    .ToList() ?? new List<string>(),
                Categories = p.Product_Categories?
                    .Where(pc => pc?.Category != null)
                    .Select(pc => pc.Category!.Category_name ?? "")
                    .ToList() ?? new List<string>(),
                Images = p.Product_Images?
                    .Where(pi => pi != null)
                    .Select(pi => pi.image_url ?? "")
                    .ToList() ?? new List<string>()
            };
        }
    }
}
