using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<ProductDTO> CreateProductAsync(CreateProductDto dto, int createdBy)
        {
            var existingByCode = await _productRepository.GetByCodeAsync(dto.Product_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Sản phẩm với mã '{dto.Product_code}' đã tồn tại.");

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Product_code = dto.Product_code,
                Product_name = dto.Product_name,
                Product_price = dto.Product_price ?? "",
                Product_link = dto.Product_link,
                Product_description = dto.Product_description,
                Tax_rate = dto.Tax_rate ?? 0,
                Cover_type_id = dto.Cover_type_id,
                Publisher_id = dto.Publisher_id,
                Distributor_id = dto.Distributor_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = now,
                Updated_at = now,
                Product_Categories = dto.CategoryIds?.Select(cid => new Product_category
                {
                    Category_id = cid,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                }).ToList() ?? new List<Product_category>(),
                Product_Authors = dto.AuthorIds?.Select(aid => new Product_author
                {
                    Author_id = aid,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                }).ToList() ?? new List<Product_author>()
            };
            var created = await _productRepository.AddAsync(product);
            return MapToDto(created);
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto, int updatedBy)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var existingByCode = await _productRepository.GetByCodeAsync(dto.Product_code);
            if (existingByCode != null && existingByCode.Id != id)
                throw new InvalidOperationException($"Sản phẩm với mã '{dto.Product_code}' đã tồn tại.");

            existing.Product_code = dto.Product_code;
            existing.Product_name = dto.Product_name;
            existing.Product_price = dto.Product_price ?? "";
            existing.Product_link = dto.Product_link;
            existing.Product_description = dto.Product_description;
            existing.Tax_rate = dto.Tax_rate ?? 0;
            existing.Cover_type_id = dto.Cover_type_id;
            existing.Publisher_id = dto.Publisher_id;
            existing.Distributor_id = dto.Distributor_id;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;

            var now = DateTime.UtcNow;
            existing.Product_Categories ??= new List<Product_category>();
            existing.Product_Categories.Clear();
            foreach (var cid in dto.CategoryIds ?? Array.Empty<int>())
            {
                existing.Product_Categories.Add(new Product_category
                {
                    Product_id = existing.Id,
                    Category_id = cid,
                    Created_by = updatedBy,
                    Updated_by = updatedBy,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                });
            }
            existing.Product_Authors ??= new List<Product_author>();
            existing.Product_Authors.Clear();
            foreach (var aid in dto.AuthorIds ?? Array.Empty<int>())
            {
                existing.Product_Authors.Add(new Product_author
                {
                    Product_id = existing.Id,
                    Author_id = aid,
                    Created_by = updatedBy,
                    Updated_by = updatedBy,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                });
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
