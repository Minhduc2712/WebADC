using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IWarehouseExportRepository
    {
        Task<Warehouse_export?> GetByIdAsync(int id);
        Task<Warehouse_export?> GetByCodeAsync(string code);
        Task<IEnumerable<Warehouse_export>> GetAllAsync();
        Task<PagedResult<Warehouse_export>> GetPagedWarehouseExportsAsync(WarehouseExportFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<IEnumerable<Warehouse_export>> GetByCustomerIdsAsync(IEnumerable<int> customerIds);
        Task<IEnumerable<Warehouse_export>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Warehouse_export>> GetByWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<Warehouse_export>> GetByStatusAsync(string status);
        Task<IEnumerable<Warehouse_export>> GetByExcludedStatusesAsync(params string[] excludeStatuses);
        Task<IEnumerable<Warehouse_export>> GetChildExportsAsync(int parentExportId);
        Task<IEnumerable<Warehouse_export>> GetByMergedIntoExportIdAsync(int mergedExportId);
        Task<PagedResult<(Warehouse_export Export, int IndentLevel)>> GetPagedHierarchicalExportsAsync(WarehouseExportFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<int> GetMaxChildSuffixAsync(string codePrefix);
        Task AddAsync(Warehouse_export export);
        Task UpdateAsync(Warehouse_export export);
        Task DeleteAsync(int id);
    }
}
