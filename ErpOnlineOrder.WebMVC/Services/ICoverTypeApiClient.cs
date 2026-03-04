using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface ICoverTypeApiClient
    {
        Task<IEnumerable<CoverTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CoverTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CoverTypeDto?> CreateAsync(CreateCoverTypeDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCoverTypeDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
