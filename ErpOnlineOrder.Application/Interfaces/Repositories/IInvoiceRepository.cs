using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByCodeAsync(string code);
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<PagedResult<Invoice>> GetPagedInvoicesAsync(InvoiceFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<IEnumerable<Invoice>> GetByCustomerIdsAsync(IEnumerable<int> customerIds);
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(string status);
        Task<IEnumerable<Invoice>> GetForMergeAsync(IEnumerable<int>? customerIds = null);
        Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(IEnumerable<int>? customerIds = null);
        Task<IEnumerable<Invoice>> GetChildInvoicesAsync(int parentInvoiceId);
        Task AddAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(int id);
    }
}
