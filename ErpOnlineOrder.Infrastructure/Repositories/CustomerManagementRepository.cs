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

        public async Task<Customer_management?> GetByIdAsync(int id)
        {
            return await _context.CustomerManagements
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .FirstOrDefaultAsync(cm => cm.Id == id && !cm.Is_deleted);
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync()
        {
            return await _context.CustomerManagements
                .Where(cm => !cm.Is_deleted)
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId)
        {
            return await _context.CustomerManagements
                .Where(cm => !cm.Is_deleted && cm.Staff_id == staffId)
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId)
        {
            return await _context.CustomerManagements
                .Where(cm => !cm.Is_deleted && cm.Customer_id == customerId)
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task<Customer_management?> GetByStaffAndCustomerAsync(int staffId, int customerId)
        {
            return await _context.CustomerManagements
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .FirstOrDefaultAsync(cm => !cm.Is_deleted && cm.Staff_id == staffId && cm.Customer_id == customerId);
        }

        public async Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId)
        {
            return await _context.CustomerManagements
                .Where(cm => !cm.Is_deleted && cm.Province_id == provinceId)
                .Include(cm => cm.Customer)
                .Include(cm => cm.Staff)
                .Include(cm => cm.Province)
                .OrderByDescending(cm => cm.Created_at)
                .ToListAsync();
        }

        public async Task AddAsync(Customer_management customerManagement)
        {
            await _context.CustomerManagements.AddAsync(customerManagement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer_management customerManagement)
        {
            _context.CustomerManagements.Update(customerManagement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customerManagement = await _context.CustomerManagements.FindAsync(id);
            if (customerManagement != null)
            {
                customerManagement.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
