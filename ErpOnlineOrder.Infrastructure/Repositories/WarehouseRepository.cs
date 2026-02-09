using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public WarehouseRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .Include(w => w.Province)
                .FirstOrDefaultAsync(w => w.Id == id && !w.Is_deleted);
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Warehouse_code == code && !w.Is_deleted);
        }

        public async Task<Warehouse?> GetByNameAsync(string name)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Warehouse_name == name && !w.Is_deleted);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .Include(w => w.Province)
                .Where(w => !w.Is_deleted)
                .OrderBy(w => w.Warehouse_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId)
        {
            return await _context.Warehouses
                .Include(w => w.Province)
                .Where(w => !w.Is_deleted && w.Province_id == provinceId)
                .OrderBy(w => w.Warehouse_name)
                .ToListAsync();
        }

        public async Task<Warehouse> AddAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                warehouse.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
