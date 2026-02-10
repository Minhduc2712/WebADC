using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface ICategoryApiClient
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
