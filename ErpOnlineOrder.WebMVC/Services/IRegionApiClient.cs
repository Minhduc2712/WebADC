using ErpOnlineOrder.Application.DTOs.RegionDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IRegionApiClient
    {
        Task<IEnumerable<RegionDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RegionDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<RegionDTO?> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRegionDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
