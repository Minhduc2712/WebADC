using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByIdForUpdateAsync(int id);
        Task<Invoice?> GetByCodeAsync(string code);
        Task<IEnumerable<InvoiceDto>> GetAllAsync();
        Task<PagedResult<InvoiceDto>> GetPagedInvoicesAsync(InvoiceFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<IEnumerable<InvoiceDto>> GetByCustomerIdsAsync(IEnumerable<int> customerIds);
        Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<InvoiceDto>> GetByStatusAsync(string status);
        Task<IEnumerable<InvoiceDto>> GetForMergeAsync(IEnumerable<int>? customerIds = null);
        Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(IEnumerable<int>? customerIds = null);
        Task<IEnumerable<Invoice>> GetChildInvoicesAsync(int parentInvoiceId);
        Task<int> GetMaxChildSuffixAsync(string codePrefix);
        Task<IEnumerable<Invoice>> GetByMergedIntoInvoiceIdAsync(int mergedInvoiceId);
        Task AddAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(int id);
    }
}
