using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerPackageRepository : ICustomerPackageRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CustomerPackageRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Customer_package> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.CustomerPackages.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(cm => cm.Customer)
                             .Include(cm => cm.Package);
            }
            return query;
        }

        public async Task<Customer_package?> GetByIdAsync(int id, bool includeDetails)
        {
            return await GetBaseQuery(includeDetails).FirstOrDefaultAsync(cp => cp.Id == id);
        }

        public async Task<Customer_package?> GetByCustomerAndPackageAsync(int customerId, int packageId, bool includeDetails)
        {
            return await GetBaseQuery(includeDetails)
                .FirstOrDefaultAsync(cp => cp.Customer_id == customerId && cp.Package_id == packageId);
        }

        public async Task<IEnumerable<Customer_package>> GetByCustomerIdAsync(int customerId, bool includeDetails = false)
        {
            return await GetBaseQuery(includeDetails)
                .Where(cp => cp.Customer_id == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_package>> GetByPackageIdAsync(int packageId, bool includeDetails = false)
        {
            return await GetBaseQuery(includeDetails)
                .Where(cp => cp.Package_id == packageId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetProductIdsByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerPackages
                .Where(cp => cp.Customer_id == customerId && cp.Is_active)
                .Join(_context.PackageProducts.Where(pp => pp.Is_active),
                    cp => cp.Package_id, pp => pp.Package_id, (cp, pp) => pp.Product_id)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Customer_package> AddAsync(Customer_package entity)
        {
            entity.Created_at = entity.Updated_at = DateTime.Now;
            _context.CustomerPackages.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Customer_package entity)
        {
            entity.Updated_at = DateTime.Now;
            _context.CustomerPackages.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.CustomerPackages.FindAsync(id);
            if (entity != null)
            {
                entity.Is_deleted = true;
                entity.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int customerId, int packageId)
        {
            return await _context.CustomerPackages
                .AnyAsync(cp => cp.Customer_id == customerId && cp.Package_id == packageId);
        }
    }
}
