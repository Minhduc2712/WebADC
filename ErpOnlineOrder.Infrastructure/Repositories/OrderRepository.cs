using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
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
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Is_deleted);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Where(o => !o.Is_deleted)
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .ToListAsync();
        }

        public async Task<Order?> GetByCodeAsync(string code)
        {
            return await _context.Orders
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