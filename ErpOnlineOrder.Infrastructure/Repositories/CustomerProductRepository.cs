using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
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

        public IQueryable<Customer_product> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.CustomerProducts.AsNoTracking();
            
            if (includeDetails)
            {
                query = query.Include(cp => cp.Customer).Include(cp => cp.Product);
            }
            
            return query.Where(cp => !cp.Is_deleted);
        }

        public async Task<Customer_product?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(cp => cp.Id == id);
        }

        public async Task<Customer_product?> GetByIdBasicAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(cp => cp.Id == id);
        }

        public async Task<Customer_product?> GetByCustomerAndProductAsync(int customerId, int productId)
        {
            return await GetBaseQuery()
               .FirstOrDefaultAsync(cp => cp.Customer_id == customerId && cp.Product_id == productId);
        }
        public async Task<IEnumerable<Customer_product>> GetByCustomerIdAsync(int customerId)
        {
            return await GetBaseQuery(true) 
                .Where(cp => cp.Customer_id == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerProductDto>> GetDtosByCustomerIdAsync(int customerId)
        {
            return await GetBaseQuery()
                .Where(cp => cp.Customer_id == customerId)
                .Select(cp => new CustomerProductDto
                {
                    Id = cp.Id,
                    Customer_id = cp.Customer_id,
                    Customer_name = cp.Customer != null ? cp.Customer.Full_name ?? "" : "",
                    Product_id = cp.Product_id,
                    Product_code = cp.Product != null ? cp.Product.Product_code ?? "" : "",
                    Product_name = cp.Product != null ? cp.Product.Product_name ?? "" : "",
                    Original_price = cp.Product != null ? cp.Product.Product_price : 0,
                    Max_quantity = cp.Max_quantity,
                    Is_active = cp.Is_active,
                    Is_Excluded = cp.Is_Excluded,
                    Created_at = cp.Created_at
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<Customer_product>> GetByCustomerIdWithDetailsAsync(int customerId)
        {
            return await _context.CustomerProducts
                .AsNoTracking()
                .Include(cp => cp.Product)
                    .ThenInclude(p => p.Product_Categories)
                        .ThenInclude(pc => pc.Category)
                .Where(cp => cp.Customer_id == customerId && cp.Is_active && !cp.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_product>> GetByProductIdAsync(int productId)
        {
            return await GetBaseQuery(true)
                .Where(cp => cp.Product_id == productId)
                .ToListAsync();
        }            

        public async Task<IEnumerable<Customer_product>> GetAllAsync()
        {
            return await GetBaseQuery(true).ToListAsync();
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
            var entity = await _context.CustomerProducts.FirstOrDefaultAsync(cp => cp.Id == id);
            if (entity != null)
            {
                entity.Is_deleted = true;
                entity.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int customerId, int productId, bool onlyActive = true)
        {
            var query = _context.CustomerProducts.Where(cp =>
                cp.Customer_id == customerId &&
                cp.Product_id == productId &&
                !cp.Is_deleted);

            if (onlyActive) query = query.Where(cp => cp.Is_active);

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsAsync(int staffId, int customerId)
        {
            return await _context.CustomerManagements
                .AsNoTracking()
                .AnyAsync(cm => cm.Staff_id == staffId && 
                                cm.Customer_id == customerId && 
                                !cm.Is_deleted);
        }
        
        public async Task<IEnumerable<int>> GetExistingProductIdsAsync(int customerId, IEnumerable<int> productIds)
        {
            return await _context.CustomerProducts
                .Where(cp => cp.Customer_id == customerId
                            && productIds.Contains(cp.Product_id)
                            && !cp.Is_deleted)
                .Select(cp => cp.Product_id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_product>> GetAllByProductIdsAsync(int customerId, IEnumerable<int> productIds)
        {
            return await _context.CustomerProducts
                .IgnoreQueryFilters()
                .Where(cp => cp.Customer_id == customerId && productIds.Contains(cp.Product_id))
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Customer_product> customerProducts)
        {
            var now = DateTime.Now;
            foreach (var cp in customerProducts)
                cp.Updated_at = now;
            _context.CustomerProducts.UpdateRange(customerProducts);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer_product?> GetWithFiltersAsync(int customerId, int productId)
        {
            return await _context.CustomerProducts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cp => cp.Customer_id == customerId && cp.Product_id == productId);
        }

        public async Task<IEnumerable<int>> GetExcludedProductIdsByCustomerAsync(int customerId)
        {
            return await _context.CustomerProducts
                .Where(cp => cp.Customer_id == customerId && cp.Is_Excluded && cp.Is_active && !cp.Is_deleted)
                .Select(cp => cp.Product_id)
                .ToListAsync();
        }
    }
}
