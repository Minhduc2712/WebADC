using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var dto = MapToDto(product);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            return dto;
        }

        public async Task<Product?> GetEntityByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(int? userId = null)
        {
            var products = await _productRepository.GetAllAsync();
            var list = products.Select(p => MapToDto(p)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                foreach (var dto in list)
                    await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher, int? userId = null)
        {
            var products = await _productRepository.SearchAsync(name, author, publisher);
            var list = products.Select(p => MapToDto(p)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                foreach (var dto in list)
                    await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString, int? userId = null)
        {
            var products = await _productRepository.SearchByAllAsync(searchString);
            var list = products.Select(p => MapToDto(p)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                foreach (var dto in list)
                    await RecordPermissionEnricher.EnrichProductAsync(dto, userId.Value, _permissionService);
            }
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId = null)
        {
            var products = await _productRepository.GetAllAsync();
            var list = products.Select(p => MapToDto(p)).ToList();

            if (customerId.HasValue && customerId.Value > 0)
            {
                var assigned = await _customerProductRepository.GetByCustomerIdAsync(customerId.Value);
                var allowedProductIds = assigned.Where(cp => cp.Is_active).Select(cp => cp.Product_id).ToHashSet();
                list = list.Where(p => allowedProductIds.Contains(p.Id)).ToList();
            }

            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId = null)
        {
            var products = await _productRepository.SearchByAllAsync(searchString);
            var list = products.Select(p => MapToDto(p)).ToList();

            if (customerId.HasValue && customerId.Value > 0)
            {
                var assigned = await _customerProductRepository.GetByCustomerIdAsync(customerId.Value);
                var allowedProductIds = assigned.Where(cp => cp.Is_active).Select(cp => cp.Product_id).ToHashSet();
                list = list.Where(p => allowedProductIds.Contains(p.Id)).ToList();
            }

            return list;
        }

        public async Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId)
        {
            var cp = await _customerProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            return cp != null && !cp.Is_deleted && cp.Is_active;
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

        public async Task<byte[]> ExportProductsToExcelAsync(string? search = null)
        {
            var products = await _productRepository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                products = products.Where(p =>
                    (p.Product_name?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Product_code?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Product_description?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Publisher?.Publisher_name?.ToLowerInvariant().Contains(q) ?? false) ||
                    (p.Product_Authors?.Any(pa => pa.Author?.Author_name?.ToLowerInvariant().Contains(q) ?? false) ?? false)
                ).ToList();
            }
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
            foreach (var p in products)
            {
                var dto = MapToDto(p);
                ExcelHelper.SetCellValue(ws.Cell(row, 1), dto.Product_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), dto.Product_name);
                ExcelHelper.SetCellValue(ws.Cell(row, 3), string.Join(", ", dto.Authors ?? new List<string>()));
                ExcelHelper.SetCellValue(ws.Cell(row, 4), dto.Publisher_name ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 5), dto.Product_price ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 6), string.Join(", ", dto.Categories ?? new List<string>()));
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }

        private static ProductDTO MapToDto(Product p)
        {
            return new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description ?? "",
                Product_price = p.Product_price ?? "",
                Product_link = p.Product_link ?? "",
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
