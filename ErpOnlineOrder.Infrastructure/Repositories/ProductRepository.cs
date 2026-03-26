using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using Microsoft.EntityFrameworkCore.Query;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

        public ProductRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        
        private static IQueryable<ProductDTO> ProjectToProductDtoWithCustomer(IQueryable<Product> query, int customerId, bool includePublisherAndAuthors = true)
        {
            return query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description ?? "",
                Product_price = p.Product_price,
                Product_link = p.Product_link ?? "",
                Publisher_name = includePublisherAndAuthors ? (p.Publisher != null ? (p.Publisher.Publisher_name ?? "") : "") : "",
                Authors = includePublisherAndAuthors
                    ? p.Product_Authors.Where(pa => pa.Author != null).Select(pa => pa.Author!.Author_name ?? "").ToList()
                    : new List<string>(),
                Categories = p.Product_Categories.Where(pc => pc.Category != null).Select(pc => pc.Category!.Category_name ?? "").ToList(),
                Images = p.Product_Images.Select(pi => pi.image_url ?? "").ToList()
            });
        }

        private IQueryable<ProductDTO> ProjectToProductDto(IQueryable<Product> query)
        {
            return query.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider);
        }

        private IQueryable<ProductSelectDto> ProjectToProductSelectDto(IQueryable<Product> query)
        {
            return query.ProjectTo<ProductSelectDto>(_mapper.ConfigurationProvider);
        }

        private IQueryable<Product> GetBaseQuery()
        {
            return _context.Products.Where(p => !p.Is_deleted).AsNoTracking();
        }

        public async Task<Dictionary<int, ProductValidationInfoDto>> GetProductValidationMapAsync(int customerId, List<int> productIds)
        {
            return await GetBaseQuery()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => new ProductValidationInfoDto
                {
                    Id = p.Id,
                    Product_name = p.Product_name ?? "Unknown",
                    Price = p.Product_price,
                    HasSetting = true
                });
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await GetBaseQuery()
                .Include(p => p.Publisher)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdBasicAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Id == id);
        }

        public IQueryable<Product?> GetByProductId(int id)
        {
            return GetBaseQuery()
                .Where(p => p.Id == id);
        }

        public async Task<ProductDTO?> GetNameByProductId(int id)
        {
            return await GetBaseQuery()
                .Where(p => p.Id == id)
                .Select(p => new ProductDTO
                {
                    Product_name = p.Product_name ?? ""
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetCategeoryIdsByProductIdAsync(int id)
        {
            return await GetBaseQuery()
                .Where(p => p.Id == id)
                .SelectMany(p => p.Product_Categories.Select(pc => pc.Category_id))
                .ToListAsync();
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(p => p.Product_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(p => p.Product_code == code);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Product?> GetByNameAsync(string name)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(p => p.Product_name == name);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsForShopAsync(int? customerId, ProductForShopFilterRequest request)
        {
            var query = GetBaseQuery()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            var projected = customerId.HasValue && customerId.Value > 0
                ? ProjectToProductDtoWithCustomer(query, customerId.Value)
                : ProjectToProductDto(query);
            return await projected.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<PagedResult<Product>> GetPagedProductAsync(ProductFilterRequest request)
        {
            var query = GetBaseQuery()
                .Include(p => p.Publisher)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .AsSplitQuery()
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.ToLower().Contains(search)) ||
                    (p.Product_code != null && p.Product_code.ToLower().Contains(search)) ||
                    (p.Product_description != null && p.Product_description.ToLower().Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.ToLower().Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.ToLower().Contains(search))
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

        public async Task<PagedResult<ProductDTO>> GetPagedProductsForShopDisplayAsync(int? customerId, ProductForShopFilterRequest request)
        {
            var query = GetBaseQuery()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.ToLower().Contains(search)) ||
                    (p.Product_code != null && p.Product_code.ToLower().Contains(search)) ||
                    (p.Product_description != null && p.Product_description.ToLower().Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.ToLower().Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var cat = request.Category.Trim();
                query = query.Where(p => p.Product_Categories.Any(pc =>
                    pc.Category != null && pc.Category.Category_name == cat));
            }

            query = request.Sort switch
            {
                "price_asc" => query.OrderBy(p => p.Product_price),
                "price_desc" => query.OrderByDescending(p => p.Product_price),
                "name_asc" => query.OrderBy(p => p.Product_name ?? ""),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id)
            };

            var projectedQuery = customerId.HasValue && customerId.Value > 0
                ? ProjectToProductDtoWithCustomer(query, customerId.Value)
                : ProjectToProductDto(query);
            return await projectedQuery.ToPagedListAsync(request);
        }

        public async Task<IEnumerable<ProductDTO>> GetRelatedProductsForShopAsync(int productId, IEnumerable<string> categoryNames, int? customerId, int limit = 4)
        {
            var catList = categoryNames.ToList();
            if (catList.Count == 0)
                return new List<ProductDTO>();

            var query = GetBaseQuery()
                .Where(p => p.Id != productId
                    && p.Product_Categories.Any(pc => pc.Category != null && catList.Contains(pc.Category.Category_name)))
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            var projectedQuery = customerId.HasValue && customerId.Value > 0
                ? ProjectToProductDtoWithCustomer(
                    query.OrderByDescending(p => p.Id).Take(limit),
                    customerId.Value,
                    includePublisherAndAuthors: false)
                : ProjectToProductDto(query.OrderByDescending(p => p.Id).Take(limit));

            return await projectedQuery.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesForShopAsync(int? customerId)
        {
            var query = GetBaseQuery()
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
            var query = GetBaseQuery()
                .OrderByDescending(p => p.Id);

            return await ProjectToProductDto(query).ToListAsync();
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string? name, string? author, string? publisher)
        {
            var query = GetBaseQuery()
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Product_name != null && p.Product_name.Contains(name));
            if (!string.IsNullOrEmpty(author))
                query = query.Where(p => p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.Contains(author)));
            if (!string.IsNullOrEmpty(publisher))
                query = query.Where(p => p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.Contains(publisher));

            return await ProjectToProductDto(query.OrderByDescending(p => p.Id)).ToListAsync();
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllAsync(string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return await GetAllAsync();

            var search = searchString.Trim().ToLowerInvariant();
            var query = GetBaseQuery()
                .Where(p =>
                    ((p.Product_name != null && p.Product_name.ToLower().Contains(search)) ||
                     (p.Product_code != null && p.Product_code.ToLower().Contains(search)) ||
                     (p.Product_description != null && p.Product_description.ToLower().Contains(search)) ||
                     (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.ToLower().Contains(search)) ||
                     p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.ToLower().Contains(search))));

            return await ProjectToProductDto(query.OrderByDescending(p => p.Id)).ToListAsync();
        }

        public async Task<IEnumerable<ProductDTO>> SearchByAllForShopAsync(string? searchString, int? customerId)
        {
            var query = GetBaseQuery()
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(p => p.Customer_Products.Any(cp =>
                    cp.Customer_id == customerId.Value && cp.Is_active));
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    (p.Product_name != null && p.Product_name.ToLower().Contains(search)) ||
                    (p.Product_code != null && p.Product_code.ToLower().Contains(search)) ||
                    (p.Product_description != null && p.Product_description.ToLower().Contains(search)) ||
                    (p.Publisher != null && p.Publisher.Publisher_name != null && p.Publisher.Publisher_name.ToLower().Contains(search)) ||
                    p.Product_Authors.Any(pa => pa.Author != null && pa.Author.Author_name != null && pa.Author.Author_name.ToLower().Contains(search)));
            }

            var projected = customerId.HasValue && customerId.Value > 0
                ? ProjectToProductDtoWithCustomer(query, customerId.Value)
                : ProjectToProductDto(query);
            return await projected.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetByCategoryIdAsync(int categoryId)
        {
            var query = GetBaseQuery()
                .Where(p => p.Product_Categories.Any(pc => pc.Category_id == categoryId))
                .OrderByDescending(p => p.Id);
            return await ProjectToProductDto(query).ToListAsync();
        }

        public async Task<IEnumerable<ProductSelectDto>> GetForSelectAsync()
        {
            var query = GetBaseQuery()
                .OrderBy(p => p.Product_name ?? p.Product_code ?? "");
            return await ProjectToProductSelectDto(query).ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            product.Created_at = DateTime.Now;
            product.Updated_at = DateTime.Now;
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
                product.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Product product)
        {
            product.Updated_at = DateTime.Now;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetPriceByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && !p.Is_deleted)
                .Select(p => p.Product_price)
                .FirstOrDefaultAsync();
        }
    }
}
