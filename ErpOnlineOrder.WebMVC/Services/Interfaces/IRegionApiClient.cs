using ErpOnlineOrder.Application.DTOs.RegionDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IRegionApiClient
    {
        Task<IEnumerable<RegionDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RegionDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(RegionDTO? Data, string? Error)> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRegionDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
