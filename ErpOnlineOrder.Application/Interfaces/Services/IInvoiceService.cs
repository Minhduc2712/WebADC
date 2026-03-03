using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto?> GetByIdAsync(int id, int? userId = null);
        Task<IEnumerable<InvoiceDto>> GetAllAsync(int? userId = null);
        Task<IEnumerable<InvoiceDto>> GetForMergeAsync(int? userId = null);
        Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(int? userId = null);
        Task<PagedResult<InvoiceDto>> GetAllPagedAsync(InvoiceFilterRequest request, int? userId = null);
        Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId);
        Task<PagedResult<InvoiceDto>> GetByCustomerIdPagedAsync(int customerId, InvoiceFilterRequest request);
        Task<SplitInvoiceResultDto> SplitInvoiceAsync(SplitInvoiceDto dto, int userId);
        Task<MergeInvoiceResultDto> MergeInvoicesAsync(MergeInvoicesDto dto, int userId);
        Task<bool> UndoSplitAsync(int parentInvoiceId, int userId);
        Task<bool> UndoMergeAsync(int mergedInvoiceId, int userId);
        Task<byte[]> ExportInvoicesToExcelAsync(string? status = null);
    }
}