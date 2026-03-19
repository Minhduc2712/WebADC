using ErpOnlineOrder.Application.DTOs.StockDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IStockApiClient
    {
        Task<IEnumerable<StockDto>> GetAllAsync(int? warehouseId = null, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<StockDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(StockDto? Data, string? Error)> CreateAsync(CreateStockDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateStockDto dto, CancellationToken cancellationToken = default);
    }
}
