using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ErpOnlineOrder.Infrastructure.Extensions;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public ProductRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }


        private static IQueryable<ProductDTO> ProjectToProductDto(IQueryable<Product> query, int? customerId, bool includePublisherAndAuthors = true)
        {
            var hasCustomer = customerId.HasValue && customerId.Value > 0;
            return query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = "",
                Product_price_decimal = hasCustomer
                    ? p.Customer_Products
                        .Where(cp => cp.Customer_id == customerId && cp.Is_active && cp.Custom_price.HasValue)
                        .Select(cp => (decimal?)cp.Custom_price!.Value)
                        .FirstOrDefault()
                    : null,
                Product_price = hasCustomer ? "" : (p.Product_price ?? ""),
                Product_link = p.Product_link ?? "",
                Publisher_name = includePublisherAndAuthors ? (p.Publisher != null ? (p.Publisher.Publisher_name ?? "") : "") : "",
                Authors = includePublisherAndAuthors ? p.Product_Authors.Where(pa => pa.Author != null).Select(pa => pa.Author!.Author_name ?? "").ToList() : new List<string>(),
                Categories = p.Product_Categories.Where(pc => pc.Category != null).Select(pc => pc.Category!.Category_name ?? "").ToList(),
                Images = p.Product_Images.Select(pi => pi.image_url ?? "").ToList()
            });
        }

        private static void EnrichProductPrice(IEnumerable<ProductDTO> dtos)
        {
            foreach (var dto in dtos)
            {
                if (!dto.Product_price_decimal.HasValue && !string.IsNullOrEmpty(dto.Product_price))
                {
                    dto.Product_price_decimal = decimal.TryParse(dto.Product_price, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;
                }
                dto.Product_price = dto.Product_price_decimal?.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) ?? dto.Product_price ?? "";
            }
        }

        private static IQueryable<ProductDTO> ProjectToProductDtoLight(IQueryable<Product> query, int? customerId)
            => ProjectToProductDto(query, customerId, includePublisherAndAuthors: false);

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Product_code == code);
        }

        public async Task<Product?> GetByNameAsync(string name)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Product_name == name);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId, ProductForShopFilterRequest request)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            var list = await ProjectToProductDto(query, customerId).OrderByDescending(p => p.Id).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<PagedResult<Product>> GetPagedProductAsync(ProductFilterRequest request)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.Contains(search)) ||
                    (p.Product_code != null && p.Product_code.Contains(search)) ||
                    (p.Product_description != null && p.Product_description.Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(search))
                );
            }

            // Filter theo CategoryId
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.Product_Categories.Any(pc => pc.Category_id == request.CategoryId.Value));
            }

            // Filter theo PublisherId
            if (request.PublisherId.HasValue)
            {
                query = query.Where(p => p.Publisher_id == request.PublisherId.Value);
            }

            query = query.OrderByDescending(p => p.Id);

            return await query.ToPagedListAsync(request);
        }

        public async Task<PagedResult<Product>> GetPagedProductsForShopAsync(int? customerId, ProductForShopFilterRequest request)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .Include(p => p.Customer_Products)
                .AsSplitQuery()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.Contains(search)) ||
                    (p.Product_code != null && p.Product_code.Contains(search)) ||
                    (p.Product_description != null && p.Product_description.Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var cat = request.Category.Trim();
                query = query.Where(p => p.Product_Categories.Any(pc =>
                    pc.Category != null && pc.Category.Category_name == cat));
            }

            query = request.Sort switch
            {
                "price_asc" => query.OrderBy(p => p.Product_price ?? "0"),
                "price_desc" => query.OrderByDescending(p => p.Product_price ?? "0"),
                "name_asc" => query.OrderBy(p => p.Product_name ?? ""),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id)
            };

            return await query.ToPagedListAsync(request);
        }

        public async Task<PagedResult<ProductDTO>> GetPagedProductsForShopDisplayAsync(int? customerId, ProductForShopFilterRequest request)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.Contains(search)) ||
                    (p.Product_code != null && p.Product_code.Contains(search)) ||
                    (p.Product_description != null && p.Product_description.Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var cat = request.Category.Trim();
                query = query.Where(p => p.Product_Categories.Any(pc =>
                    pc.Category != null && pc.Category.Category_name == cat));
            }

            query = request.Sort switch
            {
                "price_asc" => query.OrderBy(p => p.Product_price ?? "0"),
                "price_desc" => query.OrderByDescending(p => p.Product_price ?? "0"),
                "name_asc" => query.OrderBy(p => p.Product_name ?? ""),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id)
            };

            var paged = await ProjectToProductDto(query, customerId).ToPagedListAsync(request);
            EnrichProductPrice(paged.Items);
            return paged;
        }

        public async Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, IEnumerable<string> categoryNames, int? customerId, int limit = 4)
        {
            var catList = categoryNames.ToList();
            if (catList.Count == 0)
                return new List<ProductDTO>();

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.Id != productId
                    && p.Product_Categories.Any(pc => pc.Category != null && catList.Contains(pc.Category.Category_name)))
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            var list = await ProjectToProductDtoLight(query.OrderByDescending(p => p.Id).Take(limit), customerId).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            var categories = await query
                .SelectMany(p => p.Product_Categories)
                .Where(pc => pc.Category != null)
                .Select(pc => pc.Category!.Category_name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories!;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var query = _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Id);

            var list = await ProjectToProductDto(query, null).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Product_name != null && p.Product_name.Contains(name));
            if (!string.IsNullOrEmpty(author))
                query = query.Where(p => p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(author)));
            if (!string.IsNullOrEmpty(publisher))
                query = query.Where(p => p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(publisher));

            var list = await ProjectToProductDto(query.OrderByDescending(p => p.Id), null).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return await GetAllAsync();

            var search = searchString.Trim();
            var query = _context.Products
                .AsNoTracking()
                .Where(p =>
                    (p.Product_name != null && p.Product_name.Contains(search)) ||
                     (p.Product_code != null && p.Product_code.Contains(search)) ||
                     (p.Product_description != null && p.Product_description.Contains(search)) ||
                     (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(search)) ||
                     p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(search)));

            var list = await ProjectToProductDto(query.OrderByDescending(p => p.Id), null).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.Contains(search)) ||
                    (p.Product_code != null && p.Product_code.Contains(search)) ||
                    (p.Product_description != null && p.Product_description.Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(search)));
            }

            var list = await ProjectToProductDto(query, customerId).OrderByDescending(p => p.Id).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<ProductDTO>> GetByCategoryIdAsync(int categoryId)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.Product_Categories.Any(pc => pc.Category_id == categoryId))
                .OrderByDescending(p => p.Id);

            var list = await ProjectToProductDto(query, null).ToListAsync();
            EnrichProductPrice(list);
            return list;
        }

        public async Task<IEnumerable<ProductSelectDto>> GetForSelectAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Product_name ?? p.Product_code ?? "")
                .Select(p => new ProductSelectDto
                {
                    Id = p.Id,
                    Product_code = p.Product_code ?? "",
                    Product_name = p.Product_name ?? ""
                })
                .ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Product product)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Products
                    .Include(p => p.Product_Categories)
                    .Include(p => p.Product_Authors)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
                if (existing == null) return;

                _context.Entry(existing).CurrentValues.SetValues(product);

                var categoryIds = (product.Product_Categories ?? Enumerable.Empty<Product_category>()).Select(x => x.Category_id).ToList();
                var toRemoveCat = existing.Product_Categories.Where(x => !categoryIds.Contains(x.Category_id)).ToList();
                var toAddCat = categoryIds
                    .Where(id => !existing.Product_Categories.Any(x => x.Category_id == id))
                    .Select(id =>
                    {
                        var pc = product.Product_Categories?.FirstOrDefault(x => x.Category_id == id);
                        var now = DateTime.UtcNow;
                        return new Product_category
                        {
                            Product_id = existing.Id,
                            Category_id = id,
                            Created_by = pc?.Created_by ?? product.Updated_by,
                            Updated_by = pc?.Updated_by ?? product.Updated_by,
                            Created_at = pc?.Created_at ?? now,
                            Updated_at = pc?.Updated_at ?? now,
                            Is_deleted = false
                        };
                    });
                foreach (var item in toRemoveCat) existing.Product_Categories.Remove(item);
                foreach (var item in toAddCat) existing.Product_Categories.Add(item);

                var authorIds = (product.Product_Authors ?? Enumerable.Empty<Product_author>()).Select(x => x.Author_id).ToList();
                var toRemoveAuth = existing.Product_Authors.Where(x => !authorIds.Contains(x.Author_id)).ToList();
                var toAddAuth = authorIds
                    .Where(id => !existing.Product_Authors.Any(x => x.Author_id == id))
                    .Select(id =>
                    {
                        var pa = product.Product_Authors?.FirstOrDefault(x => x.Author_id == id);
                        var now = DateTime.UtcNow;
                        return new Product_author
                        {
                            Product_id = existing.Id,
                            Author_id = id,
                            Created_by = pa?.Created_by ?? product.Updated_by,
                            Updated_by = pa?.Updated_by ?? product.Updated_by,
                            Created_at = pa?.Created_at ?? now,
                            Updated_at = pa?.Updated_at ?? now,
                            Is_deleted = false
                        };
                    });
                foreach (var item in toRemoveAuth) existing.Product_Authors.Remove(item);
                foreach (var item in toAddAuth) existing.Product_Authors.Add(item);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
