using ErpOnlineOrder.Application.DTOs.DistributorDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IDistributorApiClient
    {
        Task<IEnumerable<DistributorDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DistributorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(DistributorDto? Data, string? Error)> CreateAsync(CreateDistributorDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDistributorDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
