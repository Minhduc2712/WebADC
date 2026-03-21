using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerManagementRepository : ICustomerManagementRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CustomerManagementRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Customer_management> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.CustomerManagements.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(cm => cm.Customer)
                             .Include(cm => cm.Staff)
                             .Include(cm => cm.Province)
                             .Include(cm => cm.Ward);
            }
            return query;
        }

        public async Task<Customer_management?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(cm => cm.Id == id);
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId)
        {
            return await GetBaseQuery(true)
                .Where(cm => cm.Staff_id == staffId)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetCustomerIdsByStaffAsync(int staffId)
        {
            return await GetBaseQuery()
                .Where(cm => cm.Staff_id == staffId)
                .Select(cm => cm.Customer_id)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId)
        {
            return await GetBaseQuery(true)
                .Where(cm => cm.Customer_id == customerId)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerBasicAsync(int customerId)
        {
            return await GetBaseQuery()
                .Include(cm => cm.Staff)
                .Where(cm => cm.Customer_id == customerId)
                .ToListAsync();
        }

        public async Task<Customer_management?> GetByStaffAndCustomerAsync(int staffId, int customerId)
        {
            return await GetBaseQuery(true)
                .FirstOrDefaultAsync(cm => cm.Staff_id == staffId && cm.Customer_id == customerId);
        }

        public async Task<bool> ExistsAsync(int staffId, int customerId, int? excludeId = null)
        {
            var query = GetBaseQuery()
                .Where(cm => cm.Staff_id == staffId && cm.Customer_id == customerId);
            if (excludeId.HasValue)
                query = query.Where(cm => cm.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Customer_management?> FindDeletedAsync(int staffId, int customerId)
        {
            return await _context.CustomerManagements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cm => cm.Staff_id == staffId && cm.Customer_id == customerId && cm.Is_deleted);
        }

        public async Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId)
        {
            return await GetBaseQuery(true)
                .Where(cm => cm.Province_id == provinceId)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task AddAsync(Customer_management customerManagement)
        {
            customerManagement.Created_at = DateTime.Now;
            customerManagement.Updated_at = DateTime.Now;
            await _context.CustomerManagements.AddAsync(customerManagement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer_management customerManagement)
        {
            customerManagement.Updated_at = DateTime.Now;
            _context.CustomerManagements.Update(customerManagement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customerManagement = await _context.CustomerManagements.FindAsync(id);
            if (customerManagement != null)
            {
                customerManagement.Is_deleted = true;
                customerManagement.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Customer_management?> FindAssignmentByProvinceAndWardAsync(int provinceId, int? wardId)
        {
            // Try exact match first (same province + same ward, or same province + null ward for province-wide)
            var result = await GetBaseQuery()
                .Where(cm => cm.Province_id == provinceId && cm.Ward_id == wardId)
                .FirstOrDefaultAsync();

            // If looking for a specific ward and no exact match, fall back to province-wide (Ward_id = null)
            if (result == null && wardId.HasValue)
            {
                result = await GetBaseQuery()
                    .Where(cm => cm.Province_id == provinceId && cm.Ward_id == null)
                    .FirstOrDefaultAsync();
            }

            return result;
        }
    }
}
