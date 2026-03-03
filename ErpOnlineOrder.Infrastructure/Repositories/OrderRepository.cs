using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public OrderRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer).ThenInclude(c => c!.User)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Is_deleted);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.Is_deleted)
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetPagedOrdersAsync(OrderFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => !o.Is_deleted)
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(o => ids.Contains(o.Customer_id));
                else
                    return new PagedResult<Order> { Items = new List<Order>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(o => o.Order_status == request.Status);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(o => o.Order_date >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                var toDate = request.DateTo.Value.Date.AddDays(1);
                query = query.Where(o => o.Order_date < toDate);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(o =>
                    (o.Order_code != null && o.Order_code.ToLower().Contains(search)) ||
                    (o.Customer != null && o.Customer.Full_name != null && o.Customer.Full_name.ToLower().Contains(search))
                );
            }

            query = query.OrderByDescending(o => o.Order_date);
            return await query.ToPagedListAsync(request);
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdsAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<Order>();
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.Is_deleted && ids.Contains(o.Customer_id))
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.Order_date)
                .ToListAsync();
        }

        public async Task<Order?> GetByCodeAsync(string code)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Order_code == code && !o.Is_deleted);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}