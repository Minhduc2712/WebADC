using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto?> GetByIdAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetAllAsync();
        Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId);
        Task<SplitInvoiceResultDto> SplitInvoiceAsync(SplitInvoiceDto dto, int userId);
        Task<MergeInvoiceResultDto> MergeInvoicesAsync(MergeInvoicesDto dto, int userId);
        Task<bool> UndoSplitAsync(int parentInvoiceId, int userId);
        Task<bool> UndoMergeAsync(int mergedInvoiceId, int userId);
    }
}