using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IWarehouseExportService
    {
        #region CRUD c? b?n
        
        Task<WarehouseExportDto?> GetByIdAsync(int id);
        Task<IEnumerable<WarehouseExportDto>> GetAllAsync();
        Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<WarehouseExportDto>> GetByWarehouseIdAsync(int warehouseId);
        Task<WarehouseExportDto?> CreateExportFromInvoiceAsync(CreateWarehouseExportDto dto, int userId);
        
        Task<bool> UpdateDeliveryStatusAsync(int id, string status, int userId);
        Task<bool> ConfirmExportAsync(int id, int userId);
        Task<bool> CancelExportAsync(int id, int userId);
        Task<bool> DeleteExportAsync(int id);
        
        #endregion

        #region Tách/G?p phi?u xu?t kho
        Task<SplitExportResultDto> SplitExportAsync(SplitWarehouseExportDto dto, int userId);
        Task<MergeExportResultDto> MergeExportsAsync(MergeWarehouseExportsDto dto, int userId);
        Task<bool> UndoSplitAsync(int parentExportId, int userId);
        Task<bool> UndoMergeAsync(int mergedExportId, int userId);
        
        #endregion

        #region Ki?m tra
        Task<bool> HasExportForInvoiceAsync(int invoiceId);
        Task<IEnumerable<WarehouseExportDto>> GetChildExportsAsync(int parentExportId);
        
        #endregion
    }
}
