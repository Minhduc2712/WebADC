using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace ErpOnlineOrder.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICustomerProductRepository _customerProductRepository;
        private readonly IPermissionService _permissionService;

        public ProductService(
            IProductRepository productRepository,
            ICustomerProductRepository customerProductRepository,
            IPermissionService permissionService)
        {
            _productRepository = productRepository;
            _customerProductRepository = customerProductRepository;
            _permissionService = permissionService;
        }

        public async Task<ProductDTO?> GetByIdAsync(int id, int? userId = null)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            var dto = EntityMappers.ToProductDto(product);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            return dto;
        }

        public async Task<ProductDTO?> GetByProductIdAsync(int id, int? userId = null)
        {
            var query = _productRepository.GetByProductId(id);

            var dto = await query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code,
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description ?? "",
                Product_price = p.Product_price,
                Product_link = p.Product_link ?? "",
                Publisher_name = p.Publisher != null ? p.Publisher.Publisher_name : "",

                Authors = p.Product_Authors
                    .Select(pa => pa.Author.Author_name ?? "")
                    .ToList(),

                Categories = p.Product_Categories
                    .Select(pc => pc.Category.Category_name ?? "")
                    .ToList(),

                Images = p.Product_Images
                    .Select(pi => pi.image_url ?? "")
                    .ToList()
            }).FirstOrDefaultAsync();

            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            return dto;
        }
        public async Task<Product?> GetEntityByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<PagedResult<ProductDTO>> GetAllPagedAsync(ProductFilterRequest request, int? userId = null)
        {
            var paged = await _productRepository.GetPagedProductAsync(request);
            var dtos = paged.Items.Select(EntityMappers.ToProductDto).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.EnrichProduct(dto, permissions);
            }
            return new PagedResult<ProductDTO>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<IEnumerable<ProductSelectDto>> GetForSelectAsync()
        {
            return await _productRepository.GetForSelectAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(int? userId = null)
        {
            var list = (await _productRepository.GetAllAsync()).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichProduct(dto, permissions);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher, int? userId = null)
        {
            var list = (await _productRepository.SearchAsync(name, author, publisher)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichProduct(dto, permissions);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString, int? userId = null)
        {
            var list = (await _productRepository.SearchByAllAsync(searchString)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichProduct(dto, permissions);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId = null)
        {
            return (await _productRepository.GetProductsForShopAsync(customerId, new ProductForShopFilterRequest())).ToList();
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId = null)
        {
            return (await _productRepository.SearchByAllForShopAsync(searchString, customerId)).ToList();
        }

        public async Task<PagedResult<ProductDTO>> GetProductsForShopPagedAsync(int? customerId, ProductForShopFilterRequest request)
        {
            return await _productRepository.GetPagedProductsForShopDisplayAsync(customerId, request);
        }

        public async Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, IEnumerable<string> categoryNames, int? customerId, int limit = 4)
        {
            return await _productRepository.GetRelatedProductsForShopAsync(productId, categoryNames, customerId, limit);
        }

        public async Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId)
        {
            return await _productRepository.GetCategoriesForShopAsync(customerId);
        }

        public async Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId)
        {
            return await _customerProductRepository.ExistsAsync(customerId, productId);
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
                Product_price = dto.Product_price,
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
            return EntityMappers.ToProductDto(created);
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
            existing.Product_price = dto.Product_price;
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

        public async Task<byte[]> ExportProductsToExcelAsync(string? search = null)
        {
            var products = (await _productRepository.GetAllAsync()).AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                products = products.Where(p =>
                    (p.Product_name?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Product_code?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Product_description?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Publisher_name?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Authors?.Any(a => a?.ToLowerInvariant().Contains(q) ?? false) ?? false)
                );
            }
            var viCulture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sản phẩm");

            ws.Cell(1, 1).Value = "Mã SP";
            ws.Cell(1, 2).Value = "Tên sản phẩm";
            ws.Cell(1, 3).Value = "Tác giả";
            ws.Cell(1, 4).Value = "Nhà xuất bản";
            ws.Cell(1, 5).Value = "Giá";
            ws.Cell(1, 6).Value = "Danh mục";
            ws.Range(1, 1, 1, 6).Style.Font.Bold = true;

            int row = 2;
            foreach (var dto in products)
            {
                ExcelHelper.SetCellValue(ws.Cell(row, 1), dto.Product_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), dto.Product_name);
                ExcelHelper.SetCellValue(ws.Cell(row, 3), string.Join(", ", dto.Authors ?? new List<string>()));
                ExcelHelper.SetCellValue(ws.Cell(row, 4), dto.Publisher_name ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 5), dto.Product_price.ToString("N0", viCulture));
                ExcelHelper.SetCellValue(ws.Cell(row, 6), string.Join(", ", dto.Categories ?? new List<string>()));
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }

    }
}
