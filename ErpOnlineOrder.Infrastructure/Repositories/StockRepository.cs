using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public StockRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Stock> GetBaseQuery()
        {
            return _context.Stocks
                .AsNoTracking()
                .Include(s => s.Warehouse)
                .Include(s => s.Product);
        }

        public async Task<IEnumerable<Stock>> GetAllAsync(int? warehouseId = null, string? searchTerm = null)
        {
            var query = GetBaseQuery();

            if (warehouseId.HasValue && warehouseId.Value > 0)
                query = query.Where(s => s.Warehouse_id == warehouseId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var keyword = searchTerm.Trim();
                query = query.Where(s => (s.Product != null && s.Product.Product_name.Contains(keyword))
                    || (s.Product != null && s.Product.Product_code.Contains(keyword))
                    || (s.Warehouse != null && s.Warehouse.Warehouse_name.Contains(keyword)));
            }

            return await query
                .OrderBy(s => s.Warehouse!.Warehouse_name)
                .ThenBy(s => s.Product!.Product_name)
                .ToListAsync();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Stock?> GetByWarehouseAndProductAsync(int warehouseId, int productId)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(s => s.Warehouse_id == warehouseId && s.Product_id == productId);
        }

        public async Task<Stock> AddAsync(Stock stock)
        {
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            var created = await GetByIdAsync(stock.Id);
            return created ?? stock;
        }

        public async Task UpdateAsync(Stock stock)
        {
            stock.Updated_at = DateTime.Now;
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync();
        }
    }
}
