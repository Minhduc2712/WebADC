using ErpOnlineOrder.Application.DTOs.AuthorDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IAuthorApiClient
    {
        Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(AuthorDto? Data, string? Error)> CreateAsync(CreateAuthorDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateAuthorDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
