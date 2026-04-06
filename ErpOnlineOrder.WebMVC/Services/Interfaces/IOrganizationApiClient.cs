using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IOrganizationApiClient
    {
        Task<IEnumerable<OrganizationDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OrganizationDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, UpdateOrganizationDto dto, CancellationToken cancellationToken = default);
    }
}
