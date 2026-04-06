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
        private readonly IAuthorRepository _authorRepository;
        private readonly IPublisherRepository _publisherRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(
            IProductRepository productRepository,
            ICustomerProductRepository customerProductRepository,
            IPermissionService permissionService,
            IAuthorRepository authorRepository,
            IPublisherRepository publisherRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _customerProductRepository = customerProductRepository;
            _permissionService = permissionService;
            _authorRepository = authorRepository;
            _publisherRepository = publisherRepository;
            _categoryRepository = categoryRepository;
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

        public async Task<IEnumerable<string>> GetPublishersForShopAsync(int? customerId)
        {
            return await _productRepository.GetPublishersForShopAsync(customerId);
        }

        public async Task<IEnumerable<string>> GetAuthorsForShopAsync(int? customerId)
        {
            return await _productRepository.GetAuthorsForShopAsync(customerId);
        }

        public async Task<bool> IsProductAssignedToCustomerAsync(int productId, int customerId)
        {
            return await _customerProductRepository.ExistsAsync(customerId, productId);
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDto dto, int createdBy)
        {
            var codeExists = await _productRepository.ExistsByCodeAsync(dto.Product_code);
            if (codeExists)
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

            var codeExists = await _productRepository.ExistsByCodeAsync(dto.Product_code, id);
            if (codeExists)
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

        public async Task<byte[]> GetImportTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sản phẩm");

            // Header row
            ws.Cell(1, 1).Value = "Mã SP (*)";
            ws.Cell(1, 2).Value = "Tên sản phẩm (*)";
            ws.Cell(1, 3).Value = "Tác giả";
            ws.Cell(1, 4).Value = "Nhà xuất bản";
            ws.Cell(1, 5).Value = "Giá (*)";
            ws.Cell(1, 6).Value = "Danh mục";
            ws.Cell(1, 7).Value = "Thuế suất (%)";
            ws.Cell(1, 8).Value = "Mô tả";
            ws.Cell(1, 9).Value = "Link ảnh";

            var headerRange = ws.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a7f37");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Sample row
            ws.Cell(2, 1).Value = "SP001";
            ws.Cell(2, 2).Value = "Tên sách ví dụ";
            ws.Cell(2, 3).Value = "Tác giả A, Tác giả B";
            ws.Cell(2, 4).Value = "NXB Giáo dục";
            ws.Cell(2, 5).Value = 150000;
            ws.Cell(2, 6).Value = "Văn học, Giáo dục";
            ws.Cell(2, 7).Value = 0;
            ws.Cell(2, 8).Value = "Mô tả sản phẩm...";
            ws.Cell(2, 9).Value = "https://...";

            ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0fff4");

            // Ghi chú
            var noteCell = ws.Cell(4, 1);
            noteCell.Value = "Ghi chú: (*) bắt buộc. Tác giả và Danh mục nhập nhiều giá trị, phân cách bằng dấu phẩy. Nếu chưa tồn tại sẽ được tạo tự động.";
            noteCell.Style.Font.Italic = true;
            noteCell.Style.Font.FontColor = XLColor.Gray;
            ws.Range(4, 1, 4, 9).Merge();

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }

        public async Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream, int createdBy)
        {
            var result = new ImportProductResultDto();
            using var workbook = new XLWorkbook(excelStream);
            var ws = workbook.Worksheet(1);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                result.TotalRows++;
                var row = ws.Row(rowNum);

                var productCode = row.Cell(1).GetString().Trim();
                var productName = row.Cell(2).GetString().Trim();
                var authorsRaw = row.Cell(3).GetString().Trim();
                var publisherName = row.Cell(4).GetString().Trim();
                var priceRaw = row.Cell(5).GetValue<string>().Trim();
                var categoriesRaw = row.Cell(6).GetString().Trim();
                var taxRateRaw = row.Cell(7).GetString().Trim();
                var description = row.Cell(8).GetString().Trim();
                var link = row.Cell(9).GetString().Trim();

                // Bỏ qua hàng trống
                if (string.IsNullOrWhiteSpace(productCode) && string.IsNullOrWhiteSpace(productName))
                {
                    result.TotalRows--;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(productCode))
                {
                    result.Errors.Add(new ImportProductRowError { Row = rowNum, ProductCode = "", Message = "Thiếu Mã SP" });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(productName))
                {
                    result.Errors.Add(new ImportProductRowError { Row = rowNum, ProductCode = productCode, Message = "Thiếu Tên sản phẩm" });
                    continue;
                }

                if (!decimal.TryParse(priceRaw.Replace(",", "").Replace(".", ""), out var price) || price < 0)
                {
                    result.Errors.Add(new ImportProductRowError { Row = rowNum, ProductCode = productCode, Message = "Giá không hợp lệ" });
                    continue;
                }

                // Kiểm tra mã đã tồn tại
                if (await _productRepository.ExistsByCodeAsync(productCode))
                {
                    result.Errors.Add(new ImportProductRowError { Row = rowNum, ProductCode = productCode, Message = "Mã SP đã tồn tại, bỏ qua" });
                    result.Skipped++;
                    continue;
                }

                // Tìm hoặc tạo nhà xuất bản
                int? publisherId = null;
                if (!string.IsNullOrWhiteSpace(publisherName))
                {
                    var pub = await _publisherRepository.GetByNameAsync(publisherName);
                    if (pub == null)
                    {
                        pub = await _publisherRepository.AddAsync(new Publisher
                        {
                            Publisher_name = publisherName,
                            Publisher_code = "PUB_" + System.Guid.NewGuid().ToString("N")[..6].ToUpper(),
                            Created_by = createdBy,
                            Updated_by = createdBy,
                            Created_at = DateTime.UtcNow,
                            Updated_at = DateTime.UtcNow
                        });
                    }
                    publisherId = pub.Id;
                }

                // Tìm hoặc tạo tác giả
                var authorIds = new List<int>();
                foreach (var aName in authorsRaw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
                {
                    var author = await _authorRepository.GetByNameAsync(aName);
                    if (author == null)
                    {
                        author = await _authorRepository.AddAsync(new Author
                        {
                            Author_name = aName,
                            Author_code = "AUTH_" + System.Guid.NewGuid().ToString("N")[..6].ToUpper(),
                            Created_by = createdBy,
                            Updated_by = createdBy,
                            Created_at = DateTime.UtcNow,
                            Updated_at = DateTime.UtcNow
                        });
                    }
                    authorIds.Add(author.Id);
                }

                // Tìm hoặc tạo danh mục
                var categoryIds = new List<int>();
                foreach (var cName in categoriesRaw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
                {
                    var cat = await _categoryRepository.GetByNameAsync(cName);
                    if (cat == null)
                    {
                        await _categoryRepository.AddAsync(new Category
                        {
                            Category_name = cName,
                            Category_code = "CAT_" + System.Guid.NewGuid().ToString("N")[..6].ToUpper(),
                            Created_by = createdBy,
                            Updated_by = createdBy,
                            Created_at = DateTime.UtcNow,
                            Updated_at = DateTime.UtcNow
                        });
                        cat = await _categoryRepository.GetByNameAsync(cName);
                    }
                    if (cat != null) categoryIds.Add(cat.Id);
                }

                decimal.TryParse(taxRateRaw, out var taxRate);
                var now = DateTime.UtcNow;

                var product = new Product
                {
                    Product_code = productCode,
                    Product_name = productName,
                    Product_price = price,
                    Product_description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Product_link = string.IsNullOrWhiteSpace(link) ? null : link,
                    Tax_rate = taxRate,
                    Publisher_id = publisherId,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now,
                    Updated_at = now,
                    Product_Authors = authorIds.Select(aid => new Product_author
                    {
                        Author_id = aid,
                        Created_by = createdBy,
                        Updated_by = createdBy,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList(),
                    Product_Categories = categoryIds.Select(cid => new Product_category
                    {
                        Category_id = cid,
                        Created_by = createdBy,
                        Updated_by = createdBy,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList()
                };

                await _productRepository.AddAsync(product);
                result.Succeeded++;
            }

            return result;
        }

    }
}
