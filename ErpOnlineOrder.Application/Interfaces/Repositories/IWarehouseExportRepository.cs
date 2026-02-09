using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IWarehouseExportRepository
    {
        Task<Warehouse_export?> GetByIdAsync(int id);
        Task<Warehouse_export?> GetByCodeAsync(string code);
        Task<IEnumerable<Warehouse_export>> GetAllAsync();
        Task<IEnumerable<Warehouse_export>> GetByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Warehouse_export>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Warehouse_export>> GetByWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<Warehouse_export>> GetByStatusAsync(string status);
        Task<IEnumerable<Warehouse_export>> GetChildExportsAsync(int parentExportId);
        Task AddAsync(Warehouse_export export);
        Task UpdateAsync(Warehouse_export export);
        Task DeleteAsync(int id);
    }
}
