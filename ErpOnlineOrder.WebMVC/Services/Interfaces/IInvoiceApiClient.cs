using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IInvoiceApiClient
    {
        Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceDto>> GetForMergeAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<InvoiceDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SplitInvoiceResultDto?> SplitAsync(SplitInvoiceDto dto, CancellationToken cancellationToken = default);
        Task<MergeInvoiceResultDto?> MergeAsync(MergeInvoicesDto dto, CancellationToken cancellationToken = default);
        Task<bool> UndoSplitAsync(int parentInvoiceId, CancellationToken cancellationToken = default);
        Task<bool> UndoMergeAsync(int mergedInvoiceId, CancellationToken cancellationToken = default);
        
        // Task<CreateInvoiceFromOrderResultDto?> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);
        Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
        Task<PagedResult<InvoiceDto>> GetByCustomerIdPagedAsync(int customerId, InvoiceFilterRequest request, CancellationToken cancellationToken = default);
    }
}
