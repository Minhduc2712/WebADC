using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CategoryRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Category> GetBaseQuery()
        {
            return _context.Categories.AsNoTracking();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(c => c.Category_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(c => c.Category_code == code);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(c => c.Category_name == name);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await GetBaseQuery().ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            category.Created_at = DateTime.Now;
            category.Updated_at = DateTime.Now;
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            category.Updated_at = DateTime.Now;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.Is_deleted = true;
                category.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
