using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CustomerRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.User_id == userId);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<PagedResult<Customer>> GetPagedCustomersAsync(CustomerFilterRequest request)
        {
            var query = _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Customer_managements).ThenInclude(cm => cm.Province)
                .Include(c => c.Customer_Categories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    (c.Full_name != null && c.Full_name.ToLower().Contains(search)) ||
                    (c.Customer_code != null && c.Customer_code.ToLower().Contains(search)) ||
                    (c.Phone_number != null && c.Phone_number.Contains(search))
                );
            }

            if (request.RegionId.HasValue)
            {
                query = query.Where(c => c.Customer_managements.Any(cm => cm.Province != null && cm.Province.Region_id == request.RegionId.Value));
            }

            if (request.CustomerCategoryId.HasValue)
            {
                query = query.Where(c => c.Customer_Categories.Any(cc => cc.Category_id == request.CustomerCategoryId.Value));
            }

            query = query.OrderByDescending(c => c.Created_at);
            return await query.ToPagedListAsync(request);
        }

        private static IQueryable<CustomerSelectDto> ProjectToCustomerSelectDto(IQueryable<Customer> query)
        {
            return query.Select(c => new CustomerSelectDto
            {
                Id = c.Id,
                Customer_code = c.Customer_code ?? "",
                Full_name = c.Full_name ?? ""
            });
        }

        public async Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync()
        {
            var query = _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.Full_name ?? c.Customer_code ?? "");
            return await ProjectToCustomerSelectDto(query).ToListAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
