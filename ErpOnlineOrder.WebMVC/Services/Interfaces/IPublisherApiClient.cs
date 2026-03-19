using ErpOnlineOrder.Application.DTOs.PublisherDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IPublisherApiClient
    {
        Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PublisherDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(PublisherDto? Data, string? Error)> CreateAsync(CreatePublisherDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePublisherDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
