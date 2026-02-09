using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public ProductRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .Include(p => p.Product_Images)
                .AsSplitQuery()  // Dung split query de tranh performance warning
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_deleted);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Product_code == code && !p.Is_deleted);
        }

        public async Task<Product?> GetByNameAsync(string name)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Product_name == name && !p.Is_deleted);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Dung LEFT JOIN (khong co Include) de tranh loi khi Publisher_id la NULL
            return await _context.Products
                .Where(p => !p.Is_deleted)
                .Include(p => p.Publisher)        // LEFT JOIN - cho phep NULL
                .Include(p => p.Cover_type)       // LEFT JOIN - cho phep NULL
                .Include(p => p.Distributor)      // LEFT JOIN - cho phep NULL
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string? name, string? author, string? publisher)
        {
            var query = _context.Products
                .Where(p => !p.Is_deleted)
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Product_name.Contains(name));
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(p => p.Product_Authors.Any(pa => pa.Author.Author_name.Contains(author)));
            }

            if (!string.IsNullOrEmpty(publisher))
            {
                query = query.Where(p => p.Publisher != null && p.Publisher.Publisher_name.Contains(publisher));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => !p.Is_deleted && p.Product_Categories.Any(pc => pc.Category_id == categoryId))
                .Include(p => p.Publisher)
                .Include(p => p.Cover_type)
                .Include(p => p.Distributor)
                .Include(p => p.Product_Categories).ThenInclude(pc => pc.Category)
                .Include(p => p.Product_Authors).ThenInclude(pa => pa.Author)
                .AsSplitQuery()
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
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
