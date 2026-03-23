using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using System.Linq.Expressions;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        public CustomerRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Customer> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Customers.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(c => c.User);
            }
            return query;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByIdBasicAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(c => c.User_id == userId);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await GetBaseQuery(true).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Customer, bool>>? predicate = null)
        {
            var query = _context.Customers.AsNoTracking().Where(c => !c.Is_deleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            return await query.CountAsync();
        }

        public async Task<PagedResult<Customer>> GetPagedCustomersAsync(CustomerFilterRequest request)
        {
            var query = _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Customer_managements).ThenInclude(cm => cm.Province)
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

            query = query.OrderByDescending(c => c.Created_at);
            return await query.ToPagedListAsync(request);
        }

        private IQueryable<CustomerSelectDto> ProjectToCustomerSelectDto(IQueryable<Customer> query)
        {
            return query.ProjectTo<CustomerSelectDto>(_mapper.ConfigurationProvider);
        }

        public async Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync()
        {
            var query = GetBaseQuery().OrderBy(c => c.Full_name ?? c.Customer_code ?? "");
            return await ProjectToCustomerSelectDto(query).ToListAsync();
        }

        public async Task<bool> ExistsByPhoneAsync(string phone, int? excludeId = null)
        {
            var query = _context.Customers.AsNoTracking()
                .Where(c => !c.Is_deleted && c.Phone_number != null && c.Phone_number == phone);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            customer.Created_at = DateTime.Now;
            customer.Updated_at = DateTime.Now;
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            customer.Updated_at = DateTime.Now;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.Is_deleted = true;
                customer.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
