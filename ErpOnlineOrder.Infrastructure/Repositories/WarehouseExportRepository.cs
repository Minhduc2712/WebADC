using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
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
                .FirstOrDefaultAsync(e => e.Id == id && !e.Is_deleted);
        }

        public async Task<Warehouse_export?> GetByCodeAsync(string code)
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .FirstOrDefaultAsync(e => e.Warehouse_export_code == code && !e.Is_deleted);
        }

        public async Task<IEnumerable<Warehouse_export>> GetAllAsync()
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => !e.Is_deleted)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Invoice_id == invoiceId && !e.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Customer_id == customerId && !e.Is_deleted)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WarehouseExports
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Warehouse_id == warehouseId && !e.Is_deleted)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByStatusAsync(string status)
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Where(e => e.Status == status && !e.Is_deleted)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetChildExportsAsync(int parentExportId)
        {
            return await _context.WarehouseExports
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Parent_export_id == parentExportId && !e.Is_deleted)
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
