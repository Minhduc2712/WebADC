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

        public async Task<(IEnumerable<Customer_package> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? search = null, int? staffUserId = null)
        {
            var query = GetBaseQuery(includeDetails: true)
                .Where(cp => !cp.Is_deleted);

            // Nhân viên chỉ xem được khách hàng mình quản lý
            if (staffUserId.HasValue)
            {
                var managedCustomerIds = _context.Set<Customer_management>()
                    .AsNoTracking()
                    .Where(cm => cm.Staff != null && cm.Staff.User_id == staffUserId.Value)
                    .Select(cm => cm.Customer_id);

                query = query.Where(cp => managedCustomerIds.Contains(cp.Customer_id));
            }

            // Tìm kiếm theo tên khách hàng, mã gói, tên gói
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(cp =>
                    (cp.Customer != null && cp.Customer.Full_name.Contains(term)) ||
                    (cp.Package != null && (cp.Package.Package_code.Contains(term) || cp.Package.Package_name.Contains(term)))
                );
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(cp => cp.Created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Customer_package>> GetAllAsync(bool includeDetails = false)
        {
            return await GetBaseQuery(includeDetails)
                .Where(cp => !cp.Is_deleted)
                .OrderByDescending(cp => cp.Created_at)
                .ToListAsync();
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
