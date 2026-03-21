using ErpOnlineOrder.Application.DTOs.WardDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IWardApiClient
    {
        Task<IEnumerable<WardDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WardDTO>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default);
        Task<WardDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(WardDTO? Data, string? Error)> CreateAsync(CreateWardDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWardDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
