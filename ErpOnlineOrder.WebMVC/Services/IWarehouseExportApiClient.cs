using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IWarehouseExportApiClient
    {
        Task<IEnumerable<WarehouseExportDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<WarehouseExportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task<WarehouseExportDto?> CreateAsync(CreateWarehouseExportDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateDeliveryStatusAsync(int id, string status, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ConfirmAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<SplitExportResultDto?> SplitAsync(SplitWarehouseExportDto dto, CancellationToken cancellationToken = default);
        Task<MergeExportResultDto?> MergeAsync(MergeWarehouseExportsDto dto, CancellationToken cancellationToken = default);
        Task<bool> UndoSplitAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UndoMergeAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasExportForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    }
}
