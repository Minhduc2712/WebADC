using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IInvoiceApiClient
    {
        Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SplitInvoiceResultDto?> SplitAsync(SplitInvoiceDto dto, CancellationToken cancellationToken = default);
        Task<MergeInvoiceResultDto?> MergeAsync(MergeInvoicesDto dto, CancellationToken cancellationToken = default);
        Task<bool> UndoSplitAsync(int parentInvoiceId, CancellationToken cancellationToken = default);
        Task<bool> UndoMergeAsync(int mergedInvoiceId, CancellationToken cancellationToken = default);
    }
}
