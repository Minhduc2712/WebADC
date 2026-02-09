using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerCategoryRepository : ICustomerCategoryRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CustomerCategoryRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Customer_category?> GetByIdAsync(int id)
        {
            return await _context.CustomerCategories
                .Include(cc => cc.Customer)
                .Include(cc => cc.Category)
                .FirstOrDefaultAsync(cc => cc.Id == id && !cc.Is_deleted);
        }

        public async Task<Customer_category?> GetByCustomerAndCategoryAsync(int customerId, int categoryId)
        {
            return await _context.CustomerCategories
                .Include(cc => cc.Customer)
                .Include(cc => cc.Category)
                .FirstOrDefaultAsync(cc => cc.Customer_id == customerId 
                    && cc.Category_id == categoryId 
                    && !cc.Is_deleted);
        }

        public async Task<IEnumerable<Customer_category>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerCategories
                .Include(cc => cc.Customer)
                .Include(cc => cc.Category)
                .Where(cc => cc.Customer_id == customerId && !cc.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_category>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.CustomerCategories
                .Include(cc => cc.Customer)
                .Include(cc => cc.Category)
                .Where(cc => cc.Category_id == categoryId && !cc.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_category>> GetAllAsync()
        {
            return await _context.CustomerCategories
                .Include(cc => cc.Customer)
                .Include(cc => cc.Category)
                .Where(cc => !cc.Is_deleted)
                .ToListAsync();
        }

        public async Task<Customer_category> AddAsync(Customer_category customerCategory)
        {
            customerCategory.Created_at = DateTime.Now;
            customerCategory.Updated_at = DateTime.Now;
            await _context.CustomerCategories.AddAsync(customerCategory);
            await _context.SaveChangesAsync();
            return customerCategory;
        }

        public async Task AddRangeAsync(IEnumerable<Customer_category> customerCategories)
        {
            var now = DateTime.Now;
            foreach (var cc in customerCategories)
            {
                cc.Created_at = now;
                cc.Updated_at = now;
            }
            await _context.CustomerCategories.AddRangeAsync(customerCategories);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer_category customerCategory)
        {
            customerCategory.Updated_at = DateTime.Now;
            _context.CustomerCategories.Update(customerCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.CustomerCategories.FindAsync(id);
            if (entity != null)
            {
                entity.Is_deleted = true;
                entity.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int customerId, int categoryId)
        {
            return await _context.CustomerCategories
                .AnyAsync(cc => cc.Customer_id == customerId 
                    && cc.Category_id == categoryId 
                    && !cc.Is_deleted);
        }
    }
}
