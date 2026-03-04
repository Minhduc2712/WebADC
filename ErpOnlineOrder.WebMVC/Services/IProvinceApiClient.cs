using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IProvinceApiClient
    {
        Task<IEnumerable<ProvinceDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProvinceDTO>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default);
        Task<ProvinceDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProvinceDTO?> CreateAsync(CreateProvinceDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateProvinceDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
