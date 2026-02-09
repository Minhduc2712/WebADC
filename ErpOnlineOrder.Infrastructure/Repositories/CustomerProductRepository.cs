using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerProductRepository : ICustomerProductRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CustomerProductRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Customer_product?> GetByIdAsync(int id)
        {
            return await _context.CustomerProducts
                .Include(cp => cp.Customer)
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.Id == id && !cp.Is_deleted);
        }

        public async Task<Customer_product?> GetByCustomerAndProductAsync(int customerId, int productId)
        {
            return await _context.CustomerProducts
                .Include(cp => cp.Customer)
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.Customer_id == customerId 
                    && cp.Product_id == productId 
                    && !cp.Is_deleted);
        }

        public async Task<IEnumerable<Customer_product>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerProducts
                .Include(cp => cp.Customer)
                .Include(cp => cp.Product)
                .Where(cp => cp.Customer_id == customerId && !cp.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_product>> GetByProductIdAsync(int productId)
        {
            return await _context.CustomerProducts
                .Include(cp => cp.Customer)
                .Include(cp => cp.Product)
                .Where(cp => cp.Product_id == productId && !cp.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_product>> GetAllAsync()
        {
            return await _context.CustomerProducts
                .Include(cp => cp.Customer)
                .Include(cp => cp.Product)
                .Where(cp => !cp.Is_deleted)
                .ToListAsync();
        }

        public async Task<Customer_product> AddAsync(Customer_product customerProduct)
        {
            customerProduct.Created_at = DateTime.Now;
            customerProduct.Updated_at = DateTime.Now;
            await _context.CustomerProducts.AddAsync(customerProduct);
            await _context.SaveChangesAsync();
            return customerProduct;
        }

        public async Task AddRangeAsync(IEnumerable<Customer_product> customerProducts)
        {
            var now = DateTime.Now;
            foreach (var cp in customerProducts)
            {
                cp.Created_at = now;
                cp.Updated_at = now;
            }
            await _context.CustomerProducts.AddRangeAsync(customerProducts);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer_product customerProduct)
        {
            customerProduct.Updated_at = DateTime.Now;
            _context.CustomerProducts.Update(customerProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.CustomerProducts.FindAsync(id);
            if (entity != null)
            {
                entity.Is_deleted = true;
                entity.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int customerId, int productId)
        {
            return await _context.CustomerProducts
                .AnyAsync(cp => cp.Customer_id == customerId 
                    && cp.Product_id == productId 
                    && !cp.Is_deleted);
        }
    }
}
