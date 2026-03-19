using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IWarehouseApiClient
    {
        Task<IEnumerable<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseSelectDto>> GetForSelectAsync(CancellationToken cancellationToken = default);
        Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WarehouseDto>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default);
        Task<(WarehouseDto? Data, string? Error)> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
