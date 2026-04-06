using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IWarehouseExportApiClient
    {
        Task<IEnumerable<WarehouseExportDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseExportDto>> GetMergeableAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<WarehouseExportDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<WarehouseExportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseExportDto>> GetMyExportsAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWarehouseExportDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateDeliveryStatusAsync(int id, string status, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ConfirmAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<SplitExportResultDto?> SplitAsync(SplitWarehouseExportDto dto, CancellationToken cancellationToken = default);
        Task<MergeExportResultDto?> MergeAsync(MergeWarehouseExportsDto dto, CancellationToken cancellationToken = default);
        Task<bool> UndoSplitAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UndoMergeAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasExportForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default);
        Task<byte[]> DownloadDocumentAsync(int id, string format = "pdf", string template = "standard", CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
        Task<PagedResult<WarehouseExportDto>> GetByCustomerIdPagedAsync(int customerId, WarehouseExportFilterRequest request, CancellationToken cancellationToken = default);
    }
}
