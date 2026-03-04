using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class WarehouseExportRepository : IWarehouseExportRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public WarehouseExportRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse_export?> GetByIdAsync(int id)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Order)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Parent_export)
                .Include(e => e.Merged_into_export)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Warehouse)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Warehouse_export?> GetByCodeAsync(string code)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .FirstOrDefaultAsync(e => e.Warehouse_export_code == code);
        }

        public async Task<IEnumerable<Warehouse_export>> GetAllAsync()
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<PagedResult<Warehouse_export>> GetPagedWarehouseExportsAsync(WarehouseExportFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(e => ids.Contains(e.Customer_id));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(e => e.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    (e.Warehouse_export_code != null && e.Warehouse_export_code.ToLower().Contains(search)) ||
                    (e.Customer != null && e.Customer.Full_name != null && e.Customer.Full_name.ToLower().Contains(search))
                );
            }

            query = query.OrderByDescending(e => e.Created_at);
            return await query.ToPagedListAsync(request);
        }

        public async Task<IEnumerable<Warehouse_export>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Invoice_id == invoiceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Customer_id == customerId)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Warehouse_id == warehouseId)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByStatusAsync(string status)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetChildExportsAsync(int parentExportId)
        {
            return await _context.WarehouseExports
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Parent_export_id == parentExportId)
                .ToListAsync();
        }

        public async Task AddAsync(Warehouse_export export)
        {
            await _context.WarehouseExports.AddAsync(export);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse_export export)
        {
            _context.WarehouseExports.Update(export);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var export = await _context.WarehouseExports.FindAsync(id);
            if (export != null)
            {
                export.Is_deleted = true;
                export.Updated_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
